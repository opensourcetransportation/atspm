#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TransitSignalPriorityRequest/TransitSignalPriorityService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.TransitSignalPriority;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;

namespace Utah.Udot.Atspm.Business.TransitSignalPriorityRequest
{
    public class TransitSignalPriorityService
    {
        private static readonly short[] EventCodes =
            (new short[] { 1, 3, 4, 5, 6, 7, 8, 10, 11 })
            .Concat(Enumerable.Range(130, 20).Select(x => (short)x))
            .ToArray();

        private static readonly int _maxDegreeOfParallelism = 5;


        private static List<int> _ring1 = new List<int> { 1, 2, 3, 4 };
        private static List<int> _ring2 = new List<int> { 5, 6, 7, 8 };
        private static List<int> _ring3 = new List<int> { 9, 10, 11, 12 };
        private static List<int> _ring4 = new List<int> { 13, 14, 15, 16 };

        private readonly IServiceProvider _serviceProvider;
        private readonly PlanService _planService;
        private readonly CycleService _cycleService;
        private readonly ILogger<TransitSignalPriorityService> _logger;

        // New block that gets all data for a single inputParameters (across multiple dates)
        private readonly TransformBlock<TransitSignalLoadParameters, (TransitSignalLoadParameters, List<IndianaEvent>)?> _loadLocationDataBlock;
        private readonly TransformBlock<(TransitSignalLoadParameters, List<IndianaEvent>), (TransitSignalLoadParameters, Dictionary<string, List<IndianaEvent>>)> _classifyEventsBlock;
        private readonly TransformBlock<(TransitSignalLoadParameters, Dictionary<string, List<IndianaEvent>>), (TransitSignalLoadParameters, List<TransitSignalPriorityCycle>, Dictionary<string, List<IndianaEvent>>)> _processCyclesBlock;
        private readonly TransformBlock<(TransitSignalLoadParameters, List<TransitSignalPriorityCycle>, Dictionary<string, List<IndianaEvent>>), (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)> _processPlansBlock;
        private readonly TransformBlock<(TransitSignalLoadParameters, List<IndianaEvent>)?, (TransitSignalLoadParameters, List<IndianaEvent>)> _filteringBlock;
        private readonly TransformBlock<(TransitSignalLoadParameters, List<TransitSignalPriorityPlan>), (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)> _calculateTSPMaxBlock;
        private readonly TransformBlock<(TransitSignalLoadParameters, List<TransitSignalPriorityPlan>), (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)> _calculateMaxExtensionBlock;

        public TransitSignalPriorityService(
            IServiceProvider serviceProvider,
            PlanService planService,
            CycleService cycleService,
            ILogger<TransitSignalPriorityService> logger)
        {
            _serviceProvider = serviceProvider;
            _planService = planService;
            _cycleService = cycleService;
            _logger = logger;

            // Block to load inputParameters and all events (aggregated over all dates)
            _loadLocationDataBlock = new TransformBlock<TransitSignalLoadParameters, (TransitSignalLoadParameters, List<IndianaEvent>)?>(
                LoadLocationAndEventsForDates(),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded } // No explicit limitation on concurrent tasks
            );

            _filteringBlock = new TransformBlock<(TransitSignalLoadParameters, List<IndianaEvent>)?, (TransitSignalLoadParameters, List<IndianaEvent>)>(
                FilterNull(),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
);

            // Reuse (or slightly adjust) the existing classification block.
            _classifyEventsBlock = new TransformBlock<(TransitSignalLoadParameters, List<IndianaEvent>), (TransitSignalLoadParameters, Dictionary<string, List<IndianaEvent>>)>(
                ClassifyEvents(),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
            );

            _processCyclesBlock = new TransformBlock<(TransitSignalLoadParameters, Dictionary<string, List<IndianaEvent>>), (TransitSignalLoadParameters, List<TransitSignalPriorityCycle>, Dictionary<string, List<IndianaEvent>>)>(
                input => ProcessCycles(input),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
            );


            _processPlansBlock = new TransformBlock<(TransitSignalLoadParameters, List<TransitSignalPriorityCycle>, Dictionary<string, List<IndianaEvent>>), (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)>(
                input => ProcessPlansAsync(input),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
            );

            _calculateTSPMaxBlock = new TransformBlock<(TransitSignalLoadParameters, List<TransitSignalPriorityPlan>), (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)>(
                input => CalculateTransitSignalPriorityMax(input),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
            );

            _calculateMaxExtensionBlock = new TransformBlock<(TransitSignalLoadParameters, List<TransitSignalPriorityPlan>), (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)>(
                input => CalculateMaxExtension(input),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
            );

        }


        public async Task<List<TransitSignalPriorityResult>> GetChartDataAsync(TransitSignalPriorityOptions options)
        {
            // Link the blocks, and add a predicate to filteringBlock's output if needed.
            _loadLocationDataBlock.LinkTo(
                _filteringBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );

            // Optionally, filter out any default values in filteringBlock (if default is a valid non-data marker).
            _filteringBlock.LinkTo(
                _classifyEventsBlock,
                new DataflowLinkOptions { PropagateCompletion = true },
                item => item != default
            );
            _classifyEventsBlock.LinkTo(
                _processCyclesBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );
            _processCyclesBlock.LinkTo(
                _processPlansBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );

            _processPlansBlock.LinkTo(
                _calculateTSPMaxBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );
            _calculateTSPMaxBlock.LinkTo(
                _calculateMaxExtensionBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );

            // For each inputParameters, post one item containing all dates.
            foreach (var location in options.LocationsAndPhases)
            {
                var inputParmeters = new TransitSignalLoadParameters
                {
                    LocationPhases = location,
                    Dates = options.Dates.ToList()
                };
                _logger.LogInformation($"Posting location {location.LocationIdentifier} with {options.Dates.ToList().Count} dates");
                bool posted = _loadLocationDataBlock.Post(inputParmeters);
                if (!posted)
                    _logger.LogError($"Failed to post data for location {location.LocationIdentifier}");
            }
            _loadLocationDataBlock.Complete();
            var results = new List<TransitSignalPriorityResult>();
            while (await _calculateMaxExtensionBlock.OutputAvailableAsync())
            {
                while (_calculateMaxExtensionBlock.TryReceive(out var result))
                {
                    results.Add(new TransitSignalPriorityResult { LocationPhases = result.Item1.LocationPhases, TransitSignalPlans = result.Item2 });
                }
            }
            await _calculateMaxExtensionBlock.Completion;

            return results;
        }

        private static Func<(TransitSignalLoadParameters, List<IndianaEvent>)?, (TransitSignalLoadParameters, List<IndianaEvent>)> FilterNull()
        {
            return item =>
            {
                if (item.HasValue)
                {
                    return item.Value;
                }
                // Decide how to handle null values. Here, we return a default value that can be filtered later.
                return default;
            };
        }

        public static (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>) CalculateTransitSignalPriorityMax(
            (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>) input)
        {
            var (parameters, plans) = input;

            foreach (var plan in plans)
            {
                foreach (var phase in plan.Phases)
                {
                    // If all phase values are zero, set the recommended TSP Max to null and note that the phase is not in use
                    if (phase.ProgrammedSplit == 0
                        && phase.MinTime == 0
                        && phase.PercentileSplit50th == 0
                        && phase.PercentileSplit85th == 0
                        && phase.PercentSkips == 0
                        && phase.MinGreen == 0
                        && phase.Yellow == 0
                        && phase.RedClearance == 0)
                    {
                        phase.RecommendedTSPMax = 0;
                        phase.Notes = "Phase not in use";
                        continue;
                    }

                    if (phase.ProgrammedSplit == 0)
                    {
                        phase.RecommendedTSPMax = 0;
                        phase.Notes = "Programmed split is zero, manually calculate";
                        continue;
                    }

                    //phase.SkipsGreaterThan70TSPMax = Math.Round(phase.ProgrammedSplit - phase.MinTime, 1);
                    //phase.ForceOffsLessThan40TSPMax = Math.Round(phase.ProgrammedSplit - (phase.MinTime + phase.PercentileSplit50th) / 2, 1);
                    //phase.ForceOffsLessThan60TSPMax = Math.Round(phase.ProgrammedSplit - phase.PercentileSplit50th, 1);
                    //phase.ForceOffsLessThan80TSPMax = Math.Round(phase.ProgrammedSplit - phase.PercentileSplit85th, 1);

                    phase.SkipsGreaterThan70TSPMax = Math.Round(phase.ProgrammedSplit - phase.MinTime, 1);
                    phase.ForceOffsLessThan40TSPMax = Math.Round(phase.ProgrammedSplit - (phase.MinTime + phase.PercentileSplit50th) / 2, 1);
                    phase.ForceOffsLessThan60TSPMax = Math.Round(phase.ProgrammedSplit - phase.PercentileSplit50th, 1);
                    phase.ForceOffsLessThan80TSPMax = Math.Round(phase.ProgrammedSplit - phase.PercentileSplit85th, 1);

                    if (phase.IsSkipsGreaterThan70TSPMax)
                    {
                        phase.RecommendedTSPMax = Math.Floor(phase.SkipsGreaterThan70TSPMax);
                        phase.Notes = "Skips greater than 70%";
                    }
                    else if (phase.IsForceOffsLessThan40TSPMax)
                    {
                        phase.RecommendedTSPMax = Math.Floor(phase.ForceOffsLessThan40TSPMax);
                        phase.Notes = "Force offs less than 40%";
                    }
                    else if (phase.IsForceOffsLessThan60TSPMax)
                    {
                        phase.RecommendedTSPMax = Math.Floor(phase.ForceOffsLessThan60TSPMax);
                        phase.Notes = "Force offs less than 60%";
                    }
                    else if (phase.IsForceOffsLessThan80TSPMax)
                    {
                        phase.RecommendedTSPMax = Math.Floor(phase.ForceOffsLessThan80TSPMax);
                        phase.Notes = "Force offs less than 80%";
                    }
                    else
                    {
                        phase.RecommendedTSPMax = 0; // Not recommended
                        phase.Notes = "No recommended TSP Max";
                    }

                    // Ensure RecommendedTSPMax is null if it is negative
                    if (phase.RecommendedTSPMax.HasValue && phase.RecommendedTSPMax < 0)
                    {
                        phase.RecommendedTSPMax = 0;
                        phase.Notes = "No recommended TSP Max";
                    }
                }
            }

            return (parameters, plans);
        }

        public static (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>) CalculateMaxExtension((TransitSignalLoadParameters, List<TransitSignalPriorityPlan>) input)
        {
            var (parameters, plans) = input;

            foreach (var plan in plans)
            {
                var inactivePhases = new HashSet<TransitSignalPhase>();
                foreach (var phase in plan.Phases)
                {

                    if ((phase.ProgrammedSplit == 0
                        && phase.MinTime == 0
                        && phase.PercentileSplit50th == 0
                        && phase.PercentileSplit85th == 0
                        && phase.PercentSkips == 0
                        && phase.MinGreen == 0
                        && phase.Yellow == 0
                        && phase.RedClearance == 0)
                        || phase.RecommendedTSPMax < 0

                        )
                    {
                        phase.MaxReduction = 0;
                        phase.MaxExtension = 0;
                        phase.PriorityMin = 0;
                        phase.PriorityMax = 0;
                        inactivePhases.Add(phase);
                        continue;
                    }
                    //Max extension is only on designated phases, and will not have a max reduction
                    //Max extension is based of the max reduction of the other phases in the ring
                    //Max reduction is on non designated phases, and will not have a max extension
                    //TSP Max is only for non designated phases and is the max reductions
                    //Max extension is the sum of the max reductions of the other phases in the ring
                    //then compare ring 1 and 2 and take the lower value

                    //pri min and max should use max extension not tsp max
                    if (phase.PhaseNumber >= 1 && phase.PhaseNumber <= 16)
                    {
                        var ring = GetRingForPhase(phase.PhaseNumber);
                        var otherDesignatedPhasesInRing = ring.Intersect(parameters.LocationPhases.DesignatedPhases.Where(p => p != phase.PhaseNumber)?.ToList());

                        //1. Set Max reduction for non designated phases to TSP Max
                        if (!parameters.LocationPhases.DesignatedPhases.Contains(phase.PhaseNumber) || (parameters.LocationPhases.DesignatedPhases.Contains(phase.PhaseNumber) && otherDesignatedPhasesInRing != null))
                        {
                            phase.MaxReduction = phase.RecommendedTSPMax.HasValue ? Convert.ToInt32(Math.Round(phase.RecommendedTSPMax.Value)) : 0;
                        }
                    }
                }


                //2. Using ring 1 to 2, and 3 to 4 determine which has the lower summed max reduction and set that as the max extension for the designated phase
                List<int> ring1DesignatedPhases = GetDesignatedPhasesInRing(_ring1, parameters.LocationPhases.DesignatedPhases);
                var ring1MaxReductionScenarios = new Dictionary<int, int>();
                foreach (var ringPhase in ring1DesignatedPhases)
                {
                    var nonDesignatedPhasesInRing = _ring1.Except(parameters.LocationPhases.DesignatedPhases).ToList();
                    var ring1PhaseMaxReduction = plan.Phases
                        .Where(p => nonDesignatedPhasesInRing.Contains(p.PhaseNumber))
                        .Sum(p => p.MaxReduction);
                    ring1MaxReductionScenarios.Add(ringPhase, ring1PhaseMaxReduction);
                }


                List<int> ring2DesignatedPhases = GetDesignatedPhasesInRing(_ring2, parameters.LocationPhases.DesignatedPhases);
                var ring2MaxReductionScenarios = new Dictionary<int, int>();
                foreach (var ringPhase in ring2DesignatedPhases)
                {
                    var nonDesignatedPhasesInRing = _ring2.Except(parameters.LocationPhases.DesignatedPhases).ToList();
                    var ring2PhaseMaxReduction = plan.Phases
                        .Where(p => nonDesignatedPhasesInRing.Contains(p.PhaseNumber))
                        .Sum(p => p.MaxReduction);
                    ring2MaxReductionScenarios.Add(ringPhase, ring2PhaseMaxReduction);
                }


                List<int> ring3DesignatedPhases = GetDesignatedPhasesInRing(_ring3, parameters.LocationPhases.DesignatedPhases);
                var ring3MaxReductionScenarios = new Dictionary<int, int>();
                foreach (var ringPhase in ring3DesignatedPhases)
                {
                    var nonDesignatedPhasesInRing = _ring3.Except(parameters.LocationPhases.DesignatedPhases).ToList();
                    var ring3PhaseMaxReduction = plan.Phases
                        .Where(p => nonDesignatedPhasesInRing.Contains(p.PhaseNumber))
                        .Sum(p => p.MaxReduction);
                    ring3MaxReductionScenarios.Add(ringPhase, ring3PhaseMaxReduction);
                }


                List<int> ring4DesignatedPhases = GetDesignatedPhasesInRing(_ring4, parameters.LocationPhases.DesignatedPhases);
                var ring4MaxReductionScenarios = new Dictionary<int, int>();
                foreach (var ringPhase in ring4DesignatedPhases)
                {
                    var nonDesignatedPhasesInRing = _ring4.Except(parameters.LocationPhases.DesignatedPhases).ToList();
                    var ring4PhaseMaxReduction = plan.Phases
                        .Where(p => nonDesignatedPhasesInRing.Contains(p.PhaseNumber))
                        .Sum(p => p.MaxReduction);
                    ring4MaxReductionScenarios.Add(ringPhase, ring4PhaseMaxReduction);
                }


                //Loop through each designated phase, determine the ring,
                //if the ring is 1 or 2 set the max extension to lower of ring 1 or 2,
                //if the ring is 3 or 4 set the max extension to the lower of ring 3 or 4



                foreach (var phase in plan.Phases)
                {
                    if (inactivePhases.Contains(phase))
                        continue;
                    if (parameters.LocationPhases.DesignatedPhases.Contains(phase.PhaseNumber))
                    {
                        var matchingPhase = GetMatchingPhase(phase.PhaseNumber);
                        if (phase.PhaseNumber >= 1 && phase.PhaseNumber <= 4)
                        {
                            //This only really works if they have designated match phases in ring 1 and 2 ie 2,6 or 4,8
                            var existingMatchingPhase = ring2MaxReductionScenarios.Where(i => i.Key == matchingPhase);
                            phase.MaxExtension = Math.Min(ring1MaxReductionScenarios[phase.PhaseNumber], existingMatchingPhase.IsNullOrEmpty() ? ring1MaxReductionScenarios[phase.PhaseNumber] : ring2MaxReductionScenarios[matchingPhase]);
                        }
                        else if (phase.PhaseNumber >= 5 && phase.PhaseNumber <= 8)
                        {
                            var existingMatchingPhase = ring1MaxReductionScenarios.Where(i => i.Key == matchingPhase);
                            phase.MaxExtension = Math.Min(ring2MaxReductionScenarios[phase.PhaseNumber], existingMatchingPhase.IsNullOrEmpty() ? ring2MaxReductionScenarios[phase.PhaseNumber] : ring1MaxReductionScenarios[matchingPhase]);
                        }
                        else if (phase.PhaseNumber >= 9 && phase.PhaseNumber <= 12)
                        {
                            var existingMatchingPhase = ring4MaxReductionScenarios.Where(i => i.Key == matchingPhase);
                            phase.MaxExtension = Math.Min(ring3MaxReductionScenarios[phase.PhaseNumber], existingMatchingPhase.IsNullOrEmpty() ? ring3MaxReductionScenarios[phase.PhaseNumber] : ring4MaxReductionScenarios[matchingPhase]);
                        }
                        else if (phase.PhaseNumber >= 13 && phase.PhaseNumber <= 16)
                        {
                            var existingMatchingPhase = ring3MaxReductionScenarios.Where(i => i.Key == matchingPhase);
                            phase.MaxExtension = Math.Min(ring4MaxReductionScenarios[phase.PhaseNumber], existingMatchingPhase.IsNullOrEmpty() ? ring4MaxReductionScenarios[phase.PhaseNumber] : ring3MaxReductionScenarios[matchingPhase]);
                        }
                    }
                }

                //3. Set the priority min and max to the programmed split +/- the max extension
                foreach (var phase in plan.Phases)
                {
                    // Keep the zero limits established above for inactive or invalid phases.
                    if (inactivePhases.Contains(phase))
                        continue;
                    phase.PriorityMin = Convert.ToInt32(Math.Round(phase.ProgrammedSplit)) - phase.MaxReduction;
                    phase.PriorityMax = Convert.ToInt32(Math.Round(phase.ProgrammedSplit)) + phase.MaxExtension;
                }

            }

            //these need to move to after all max reductions and max extensions are calculated
            //should use max reduction
            //phase.PriorityMin = phase.ProgrammedSplit > 0 ? Convert.ToInt32(Math.Round(phase.ProgrammedSplit - phase.MaxReduction)) : 0; // Program split minus tsp max
            //                                                                                                                             //should use max extension
            //phase.PriorityMax = phase.ProgrammedSplit > 0 ? Convert.ToInt32(Math.Round(phase.ProgrammedSplit + phase.MaxExtension)) : 0; // Program split minus tsp max; //Program split plus the max extension

            //phase.MaxReduction = phase.RecommendedTSPMax.HasValue ? Convert.ToInt32(Math.Round(phase.RecommendedTSPMax.Value)) : 0; //TSP MAX
            //phase.PriorityMin = phase.ProgrammedSplit > 0 ? Convert.ToInt32(Math.Round(phase.ProgrammedSplit)) : 0; // Program split minus tsp max
            //phase.PriorityMax = phase.ProgrammedSplit > 0 ? Convert.ToInt32(Math.Round(phase.ProgrammedSplit)) : 0; // Program split minus tsp max; //Program split plus the max extension




            return (parameters, plans);

        }

        private static int GetMatchingPhase(int phaseNumber)
        {
            return phaseNumber switch
            {
                1 => 5,
                2 => 6,
                3 => 7,
                4 => 8,
                5 => 1,
                6 => 2,
                7 => 3,
                8 => 4,
                9 => 13,
                10 => 14,
                11 => 15,
                12 => 16,
                13 => 9,
                14 => 10,
                15 => 11,
                16 => 12,
                _ => 0,
            };
        }

        private static List<int> GetDesignatedPhasesInRing(List<int> ring, List<int> designatedPhases)
        {
            //return the matching phases from the ring that are also in designated phases
            return ring.Intersect(designatedPhases).ToList();

        }

        private static List<int> GetPhasesForMaxReduction(List<int> ring, List<int> designatedPhases)
        {
            //check to see if ring has more than one designated phase if so return all phases in the ring
            if (ring.Intersect(designatedPhases).Count() > 1)
            {
                return ring;
            }
            return ring.Except(designatedPhases).ToList();
        }

        private static List<int> GetNonDesignatedPhasesInRing(List<int> ring1, List<int> ring2, List<int> ring3, List<int> ring4, TransitSignalLoadParameters parameters, TransitSignalPhase phase)
        {
            return phase.PhaseNumber switch
            {
                >= 1 and <= 4 => ring1.Except(parameters.LocationPhases.DesignatedPhases).ToList(),
                >= 5 and <= 8 => ring2.Except(parameters.LocationPhases.DesignatedPhases).ToList(),
                >= 9 and <= 12 => ring3.Except(parameters.LocationPhases.DesignatedPhases).ToList(),
                >= 13 and <= 16 => ring4.Except(parameters.LocationPhases.DesignatedPhases).ToList(),
                _ => new List<int>()
            };
        }

        private static List<int> GetRingForPhase(int phaseNumber)
        {
            return phaseNumber switch
            {
                >= 1 and <= 4 => _ring1,
                >= 5 and <= 8 => _ring2,
                >= 9 and <= 12 => _ring3,
                >= 13 and <= 16 => _ring4,
                _ => new List<int>()
            };
        }

        private (TransitSignalLoadParameters, List<TransitSignalPriorityCycle>, Dictionary<string, List<IndianaEvent>>) ProcessCycles((TransitSignalLoadParameters, Dictionary<string, List<IndianaEvent>>) input)
        {
            var (inputParameters, eventGroups) = input;
            var cycleEvents = eventGroups["cycleEvents"];
            var terminationEvents = eventGroups["terminationEvents"];
            var minGreenEvents = eventGroups["minGreenEvents"];
            var phases = cycleEvents.Select(c => c.EventParam).Distinct();
            var cycles = new List<TransitSignalPriorityCycle>();

            foreach (var phase in phases)
            {
                if (cycleEvents.Any(e => e.EventParam == phase))
                {
                    cycles.AddRange(_cycleService.GetTransitSignalPriorityCycles(
                        phase,
                        cycleEvents.Where(e => e.EventParam == phase).ToList(),
                        terminationEvents.Where(e => e.EventParam == phase).ToList(),
                        minGreenEvents.Where(e => e.EventParam == phase).ToList()
                    ));
                }
            }

            // Return eventGroups along with cycles
            return (inputParameters, cycles, eventGroups);
        }


        private async Task<(TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)> ProcessPlansAsync((TransitSignalLoadParameters, List<TransitSignalPriorityCycle>, Dictionary<string, List<IndianaEvent>>) input)
        {
            var (inputParameters, cycles, eventGroups) = input;
            var splitsEvents = eventGroups["splitsEvents"];
            var firstDate = inputParameters.Dates.OrderBy(d => d).First();
            var endDate = inputParameters.Dates.OrderBy(d => d).Last().AddDays(1);
            var planData = await _planService.GetPlansAsync(inputParameters.LocationPhases.LocationIdentifier, firstDate, endDate, splitsEvents);

            var plans = GetTransitSignalPriorityPlans(
                firstDate,
                endDate,
                inputParameters.LocationPhases,
                planData,
                splitsEvents,
                cycles
            ).OrderBy(p => p.PlanNumber).ToList();
            return (inputParameters, plans);
        }
        private static Func<(TransitSignalLoadParameters, List<IndianaEvent>), (TransitSignalLoadParameters, Dictionary<string, List<IndianaEvent>>)> ClassifyEvents()
        {
            return tuple =>
            {
                var (location, events) = tuple;
                var categorizedEvents = new Dictionary<string, List<IndianaEvent>>
                    {
                        { "cycleEvents", new List<IndianaEvent>() },
                        { "splitsEvents", new List<IndianaEvent>() },
                        { "minGreenEvents", new List<IndianaEvent>() },
                        { "terminationEvents", new List<IndianaEvent>() }
                    };

                foreach (var ev in events)
                {
                    if (new List<short> { 1, 8, 10, 11 }.Contains(ev.EventCode))
                        categorizedEvents["cycleEvents"].Add(ev);

                    if (new List<short> { 3 }.Contains(ev.EventCode))
                        categorizedEvents["minGreenEvents"].Add(ev);

                    if (Enumerable.Range(130, 20).Contains(ev.EventCode))
                        categorizedEvents["splitsEvents"].Add(ev);

                    if (new List<short> { 4, 5, 6, 7 }.Contains(ev.EventCode))
                        categorizedEvents["terminationEvents"].Add(ev);
                }

                return (location, categorizedEvents);
            };
        }

        private Func<TransitSignalLoadParameters, Task<(TransitSignalLoadParameters, List<IndianaEvent>)?>> LoadLocationAndEventsForDates()
        {
            return async input =>
            {
                try
                {
                    // Start fetching location concurrently with event retrieval
                    var locationTask = GetLocation(input.LocationPhases.LocationIdentifier, input.Dates.First());
                    //Get a list of dates that have a gap of at least 1 day between the previous date
                    var datesWithGaps = input.Dates.OrderBy(d => d).Where((date, index) => index > 0 && (date - input.Dates[index - 1]).TotalDays > 1).ToList();

                    var eventTasks = input.Dates.Select(async date =>
                    {
                        try
                        {
                            var events = new List<IndianaEvent>();
                            var eventTask = GetEvents(input.LocationPhases.LocationIdentifier, date);
                            if (datesWithGaps.Contains(date))
                            {
                                events.AddRange(await GetSplitEventsForThePreviousDay(input.LocationPhases.LocationIdentifier, date.AddDays(-1)));
                            }
                            events.AddRange(await eventTask);
                            return events ?? new List<IndianaEvent>(); // Ensure non-null
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error getting events for location {input.LocationPhases} on {date}");
                            return new List<IndianaEvent>(); // Return empty list on failure
                        }
                    }).ToList();


                    // Await location retrieval
                    input.Location = await locationTask;
                    if (input.Location == null)
                    {
                        _logger.LogWarning($"Location not found for {input.LocationPhases} on {input.Dates.First()}");
                        return null;
                    }
                    else
                    {
                        input.LocationPhases.ControllerManufacturer = input.Location.Devices.Where(d => d.DeviceType == Data.Enums.DeviceTypes.SignalController)?.FirstOrDefault()?.DeviceConfiguration.Product.Manufacturer;
                    }

                    _logger.LogInformation($"Location found: {input.LocationPhases}");

                    // Await all event retrieval tasks
                    var eventsForAllDates = (await Task.WhenAll(eventTasks)).SelectMany(e => e).ToList();

                    return (input, eventsForAllDates);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error processing {input.LocationPhases}");
                    return null;
                }
            };
        }

        private async Task<Location> GetLocation(string locationIdentifier, DateTime date)
        {
            // Create a scope to get the inputParameters.
            using (var scope = _serviceProvider.CreateScope())
            {
                var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
                return locationRepository.GetLatestVersionOfLocationWithDevice(locationIdentifier, date);
            }
        }

        private async Task<List<IndianaEvent>> GetEvents(string locationIdentifier, DateTime date)
        {
            using (var innerScope = _serviceProvider.CreateScope())
            {
                var eventLogRepository = innerScope.ServiceProvider.GetRequiredService<IIndianaEventLogRepository>();
                var events = eventLogRepository
                    .GetEventsByEventCodes(locationIdentifier, date, date.AddDays(1), EventCodes)
                    .ToList();
                _logger.LogInformation($"Found {events.Count} events for location {locationIdentifier} on {date}");

                // Perform manual garbage collection after retrieving event logs.
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return events;
            }
        }

        private async Task<List<IndianaEvent>> GetSplitEventsForThePreviousDay(string locationIdentifier, DateTime date)
        {
            using (var innerScope = _serviceProvider.CreateScope())
            {
                var eventLogRepository = innerScope.ServiceProvider.GetRequiredService<IIndianaEventLogRepository>();
                var events = eventLogRepository
                    .GetEventsByEventCodes(locationIdentifier, date, date.AddDays(1), Enumerable.Range(130, 20).Select(x => (short)x).ToArray())
                    .OrderBy(e => e.Timestamp)
                    .ToList();
                _logger.LogInformation($"Found {events.Count} events for location {locationIdentifier} on {date}");

                // Perform manual garbage collection after retrieving event logs.
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return events;
            }
        }


        public List<TransitSignalPriorityPlan> GetTransitSignalPriorityPlans(
            DateTime startDate,
            DateTime endDate,
            LocationPhases locationPhases,
            IReadOnlyList<Plan> planData,
            IReadOnlyList<IndianaEvent> splitEvents,
            List<TransitSignalPriorityCycle> cycles)
        {
            if (planData == null)
                throw new ArgumentNullException(nameof(planData));
            if (splitEvents == null)
                throw new ArgumentNullException(nameof(splitEvents));
            if (cycles == null)
                throw new ArgumentNullException(nameof(cycles));
            var plans = _planService.GetTransitSignalPriorityBasicPlans(startDate, endDate, locationPhases.LocationIdentifier, planData).OrderBy(p => p.Start).ToList();
            var validSplitEvents = new List<IndianaEvent>();
            for (int i = 0; i < plans.Count(); i++)
            {
                var programmedSplits = new SortedDictionary<int, int>();
                validSplitEvents.AddRange(splitEvents.Where(s => s.Timestamp >= plans[i].Start && s.Timestamp < plans[i].End).ToList());
                SetProgrammedSplits(programmedSplits, validSplitEvents);
                plans[i].ProgrammedSplits = programmedSplits;
                var zeroPhaseList = new List<int>();
                foreach (var split in programmedSplits)
                {
                    if (split.Value == 0)
                    {
                        zeroPhaseList.Add(split.Key);
                    }
                }
                foreach (var zeroPhase in zeroPhaseList)
                {
                    programmedSplits[zeroPhase] = GetPreviousSplit(zeroPhase, plans, i - 1);
                }
            }
            //Get all the planNumbers and splits with a zero value
            List<Tuple<string, int, int>> zeroValues = new List<Tuple<string, int, int>>();
            List<Tuple<string, int, int>> nonZeroValues = new List<Tuple<string, int, int>>();
            foreach (var plan in plans)
            {
                foreach (var split in plan.ProgrammedSplits)
                {
                    if (split.Value == 0)
                    {
                        zeroValues.Add(new Tuple<string, int, int>(plan.PlanNumber, split.Key, split.Value));
                    }
                    else
                    {
                        nonZeroValues.Add(new Tuple<string, int, int>(plan.PlanNumber, split.Key, split.Value));
                    }
                }
            }
            var distinctZeroValues = zeroValues.Distinct().ToList();
            var distinctNonZeroValues = nonZeroValues.Distinct().ToList();
            foreach (var zeroValue in distinctZeroValues)
            {
                //find any distinct non zero values for the same phase and planNumber
                var nonZeroValue = distinctNonZeroValues.FirstOrDefault(n => n.Item1 == zeroValue.Item1 && n.Item2 == zeroValue.Item2);
                //find the plan and split for the zero value
                if (nonZeroValue != null)
                {
                    var plansWithMatchingZero = plans.Where(p => p.PlanNumber == zeroValue.Item1 && p.ProgrammedSplits.ContainsKey(nonZeroValue.Item2) && p.ProgrammedSplits[nonZeroValue.Item2] == 0).ToList();
                    foreach (var plan in plansWithMatchingZero)
                    {
                        plan.ProgrammedSplits[zeroValue.Item2] = nonZeroValue.Item3;
                    }
                }
            }


            // Build the complete list of phases from cycles (fill in any gaps)
            var completePhases = GetCompletePhaseList(cycles);

            // Process each distinct plan number
            var tspPlans = new List<TransitSignalPriorityPlan>();
            var distinctPlanNumbers = plans.Select(p => p.PlanNumber).Distinct();

            foreach (var planNumber in distinctPlanNumbers)
            {
                var programmedSplits = plans
                    .Where(p => p.PlanNumber == planNumber)
                    .OrderByDescending(p => p.ProgrammedSplits.Count)
                    .First()
                    .ProgrammedSplits;
                var tspPlan = new TransitSignalPriorityPlan { PlanNumber = Convert.ToInt32(planNumber) };

                // Get all plans with the same plan number
                var plansForNumber = plans.Where(p => p.PlanNumber == planNumber).ToList();
                var planCycles = plansForNumber.SelectMany(plan => cycles.Where(c => c.GreenEvent >= plan.Start && c.GreenEvent < plan.End)).ToList();

                if (!planCycles.Any()) continue;

                // Group cycles by phase based on the complete phase list 
                var phaseCyclesDict = GetPhaseCyclesDictionary(planCycles, completePhases);
                if (!phaseCyclesDict.Any()) continue;

                int maxCycleCount = phaseCyclesDict.Values.Max(cycleList => cycleList.Count);
                tspPlan.NumberOfCycles = maxCycleCount;

                foreach (var (phaseNumber, cyclesForPhase) in phaseCyclesDict)
                {
                    if (!cyclesForPhase.Any())
                    {
                        tspPlan.Phases.Add(new TransitSignalPhase
                        {
                            PhaseNumber = phaseNumber,
                        });
                        continue;
                    }
                    ;

                    //Still add phase to tsp even if not in programmed splits
                    double skippedCycles = maxCycleCount - cyclesForPhase.Count;
                    double percentSkips = maxCycleCount > 0 ? Math.Round((skippedCycles / maxCycleCount) * 100, 1) : 0;
                    double percentGapOuts = maxCycleCount > 0 ? Math.Round(cyclesForPhase.Count(c => c.TerminationEvent == 4) / (double)maxCycleCount * 100, 1) : 0;
                    double averageSplit = cyclesForPhase.Any() ? Math.Round(cyclesForPhase.Average(c => c.DurationSeconds), 1) : 0;

                    var forceOffsForPlans = new List<double>();
                    var skipsForPlans = new List<double>();
                    var percentileSplit50th = new List<double>();
                    var percentileSplit85th = new List<double>();

                    foreach (var plan in plansForNumber)
                    {
                        var cyclesForPlan = cycles.Where(c => c.GreenEvent >= plan.Start && c.GreenEvent < plan.End).ToList();
                        var phaseCyclesForPlanDict = GetPhaseCyclesDictionary(cyclesForPlan, completePhases);
                        var phaseCycles = phaseCyclesForPlanDict.GetValueOrDefault(phaseNumber, new List<TransitSignalPriorityCycle>());

                        if (!phaseCycles.Any()) continue;

                        int maxCycleCountForPlan = phaseCyclesForPlanDict.Values.Max(cycleList => cycleList.Count);
                        forceOffsForPlans.Add(Math.Round(GetPercentMaxOutForceOffs(planNumber, maxCycleCountForPlan, phaseCycles) * 100, 1));
                        skipsForPlans.Add(maxCycleCountForPlan > 0 ? Math.Round((maxCycleCountForPlan - phaseCycles.Count) / (double)maxCycleCountForPlan * 100, 1) : 0);
                        percentileSplit50th.Add(GetPercentSplit(phaseCycles.Count, 0.5, phaseCycles));
                        percentileSplit85th.Add(GetPercentSplit(phaseCycles.Count, 0.85, phaseCycles));
                    }

                    var minTime = Math.Round(cyclesForPhase.Max(c => c.MinTime), 1);
                    var minGreen = cyclesForPhase.OrderBy(c => c.MinGreenDurationSeconds).First().MinGreenDurationSeconds;
                    var yellow = cyclesForPhase.First().YellowDurationSeconds;
                    var redClearance = cyclesForPhase.First().RedDurationSeconds;

                    if (!programmedSplits.ContainsKey(phaseNumber))
                    {
                        tspPlan.Phases.Add(new TransitSignalPhase
                        {
                            PhaseNumber = phaseNumber,
                            PercentSkips = percentSkips,
                            PercentGapOuts = percentGapOuts,
                            PercentMaxOutsForceOffs = Math.Round(GetPercentMaxOutForceOffs(planNumber, maxCycleCount, cyclesForPhase) * 100, 1),
                            AverageSplit = averageSplit,
                            MinTime = minTime,
                            PercentileSplit50th = percentileSplit50th.Average(),
                            PercentileSplit85th = percentileSplit85th.Average(),
                            MinGreen = minGreen,
                            Yellow = yellow,
                            RedClearance = redClearance
                        });
                        continue;
                    }


                    tspPlan.Phases.Add(new TransitSignalPhase
                    {
                        PhaseNumber = phaseNumber,
                        PercentSkips = percentSkips,
                        PercentGapOuts = percentGapOuts,
                        PercentMaxOutsForceOffs = Math.Round(GetPercentMaxOutForceOffs(planNumber, maxCycleCount, cyclesForPhase) * 100, 1),
                        IsSkipsGreaterThan70TSPMax = skipsForPlans.Min() >= 70,
                        IsForceOffsLessThan40TSPMax = forceOffsForPlans.Max() <= 40,
                        IsForceOffsLessThan60TSPMax = forceOffsForPlans.Max() <= 60,
                        IsForceOffsLessThan80TSPMax = forceOffsForPlans.Max() <= 80,
                        AverageSplit = averageSplit,
                        MinTime = minTime,
                        ProgrammedSplit = programmedSplits[phaseNumber],
                        PercentileSplit85th = percentileSplit85th.Average(),
                        PercentileSplit50th = percentileSplit50th.Average(),
                        MinGreen = minGreen,
                        Yellow = yellow,
                        RedClearance = redClearance
                    });
                }

                tspPlans.Add(tspPlan);
            }

            SetMinGreenTimeToMinGreenOfFree(tspPlans);


            return tspPlans;
        }

        private static void SetMinGreenTimeToMinGreenOfFree(List<TransitSignalPriorityPlan> tspPlans)
        {
            var freePlan = tspPlans.Where(p => p.PlanNumber == 254 || p.PlanNumber == 100)?.FirstOrDefault();
            if (freePlan != null && !freePlan.Phases.IsNullOrEmpty())
            {
                var freePlanMinGreens = new Dictionary<int, double>();
                foreach (var phase in freePlan.Phases)
                {
                    freePlanMinGreens.Add(phase.PhaseNumber, phase.MinGreen);
                }

                foreach (var plan in tspPlans.Where(p => p.PlanNumber != 254 && p.PlanNumber != 100))
                {
                    foreach (var phase in plan.Phases)
                    {
                        if (freePlanMinGreens.ContainsKey(phase.PhaseNumber))
                        {
                            //As long as freePlanMinGreens doesnt have a value of 0 or lower, use the min value of the two, else continue to use the min green from the phase
                            var minGreen = freePlanMinGreens[phase.PhaseNumber] > 0 ? Math.Min(freePlanMinGreens[phase.PhaseNumber], phase.MinGreen) : phase.MinGreen;
                            phase.MinGreen = minGreen;
                            phase.MinTime = phase.MinGreen + phase.Yellow + phase.RedClearance;
                        }
                    }
                }
            }
        }

        private int GetPreviousSplit(int v, List<TransitSignalPriorityBasicPlan> plans, int plansIndex)
        {
            // Base case: If we are out of bounds, return 0 (or any default value)
            if (plansIndex < 0)
            {
                return 0;
            }

            // Get the current plan
            var currentPlan = plans[plansIndex];

            // Check if the key exists in ProgrammedSplits
            if (currentPlan.ProgrammedSplits.TryGetValue(v, out int value))
            {
                // If the value is not zero, return it
                if (value != 0)
                {
                    return value;
                }
            }

            // Recur with the previous plan
            return GetPreviousSplit(v, plans, plansIndex - 1);
        }


        private static double GetPercentMaxOutForceOffs(string planNumber, int highCycleCount, List<TransitSignalPriorityCycle> cycles)
        {
            var phaseNumber = cycles.FirstOrDefault().PhaseNumber;
            var sixes = cycles.Count(c => c.TerminationEvent == 6);
            var fives = cycles.Count(c => c.TerminationEvent == 5);
            if (highCycleCount == 0)
            {
                return 0;
            }
            return planNumber == "254" || planNumber == "100" ? Convert.ToDouble(fives) / highCycleCount :
                                        Convert.ToDouble(sixes) / highCycleCount;
        }

        /// <summary>
        /// Creates plan objects based on cleaned events and the given time range.
        /// </summary>
        private List<TransitSignalPriorityBasicPlan> CreatePlansFromEvents(List<IndianaEvent> cleanedEvents, DateTime startDate, DateTime endDate)
        {
            var plans = new List<TransitSignalPriorityBasicPlan>();

            for (int i = 0; i < cleanedEvents.Count; i++)
            {
                DateTime planStart = cleanedEvents[i].Timestamp;
                DateTime planEnd = (i == cleanedEvents.Count - 1) ? endDate : cleanedEvents[i + 1].Timestamp;

                // If the plan's duration is longer than a day, adjust the end to be at the start of the next day.
                if ((planEnd - planStart).TotalDays > 1)
                {
                    planEnd = planStart.Date.AddDays(1);
                }

                plans.Add(new TransitSignalPriorityBasicPlan(cleanedEvents[i].EventParam.ToString(), planStart, planEnd));
            }

            return plans;
        }

        /// <summary>
        /// Builds a complete list of phases based on the cycles provided,
        /// filling in any missing phase numbers between the minimum and maximum.
        /// </summary>
        private List<int> GetCompletePhaseList(List<TransitSignalPriorityCycle> cycles)
        {
            var phaseNumbers = cycles.Select(c => c.PhaseNumber).Distinct().ToList();
            if (!phaseNumbers.Any())
                return new List<int>();

            int minPhase = phaseNumbers.Min();
            int maxPhase = phaseNumbers.Max();

            return Enumerable.Range(1, maxPhase).ToList();
        }

        /// <summary>
        /// Groups the given plan cycles into a dictionary keyed by phase number.
        /// </summary>
        private Dictionary<int, List<TransitSignalPriorityCycle>> GetPhaseCyclesDictionary(List<TransitSignalPriorityCycle> planCycles, List<int> allPhases)
        {
            var dictionary = new Dictionary<int, List<TransitSignalPriorityCycle>>();

            foreach (var phase in allPhases)
            {
                // Even if there are no cycles for a phase, add an empty list so the phase is represented.
                if (!planCycles.Any(c => c.PhaseNumber == phase))
                {
                    dictionary.Add(phase, new List<TransitSignalPriorityCycle>());
                }
                else
                {
                    var cyclesForPhase = planCycles.Where(c => c.PhaseNumber == phase).ToList();
                    dictionary.Add(phase, cyclesForPhase);
                }
            }

            return dictionary;
        }


        private double GetPercentSplit(double highCycleCount, double percentile, List<TransitSignalPriorityCycle> cycles)
        {
            if (cycles.Count <= 2)
                return 0;
            var orderedCycles = cycles.OrderBy(c => c.DurationSeconds).ToList();

            var percentilIndex = percentile * orderedCycles.Count;
            if ((percentilIndex % 1).AreEqual(0))
            {
                return orderedCycles.ElementAt(Convert.ToInt16(percentilIndex) - 1).DurationSeconds + orderedCycles.ElementAt(Convert.ToInt16(percentilIndex) - 1).RedDurationSeconds;
            }
            else
            {
                var indexMod = percentilIndex % 1;
                //subtracting .5 leaves just the integer after the convert.
                //There was probably another way to do that, but this is easy.
                int indexInt = Convert.ToInt16(percentilIndex - .5);

                var step1 = orderedCycles.ElementAt(Convert.ToInt16(indexInt) - 1).DurationSeconds + orderedCycles.ElementAt(Convert.ToInt16(indexInt) - 1).RedDurationSeconds;
                var step2 = orderedCycles.ElementAt(Convert.ToInt16(indexInt)).DurationSeconds + orderedCycles.ElementAt(Convert.ToInt16(indexInt)).RedDurationSeconds;
                var stepDiff = step2 - step1;
                var step3 = stepDiff * indexMod;
                return Math.Round(step1 + step3, 1);
            }
        }

        public void SetProgrammedSplits(SortedDictionary<int, int> splits, List<IndianaEvent> locationEvents)
        {
            var eventCodes = new List<short>();
            for (short i = 130; i <= 151; i++)
                eventCodes.Add(i);
            var splitsDt = locationEvents.Where(s => eventCodes.Contains(s.EventCode)).OrderBy(s => s.Timestamp);
            foreach (var row in splitsDt)
            {

                if (row.EventCode == 134 && !splits.ContainsKey(1))
                    splits.Add(1, row.EventParam);
                else if (row.EventCode == 134 && row.EventParam > 0)
                    splits[1] = row.EventParam;

                if (row.EventCode == 135 && !splits.ContainsKey(2))
                    splits.Add(2, row.EventParam);
                else if (row.EventCode == 135 && row.EventParam > 0)
                    splits[2] = row.EventParam;

                if (row.EventCode == 136 && !splits.ContainsKey(3))
                    splits.Add(3, row.EventParam);
                else if (row.EventCode == 136 && row.EventParam > 0)
                    splits[3] = row.EventParam;

                if (row.EventCode == 137 && !splits.ContainsKey(4))
                    splits.Add(4, row.EventParam);
                else if (row.EventCode == 137 && row.EventParam > 0)
                    splits[4] = row.EventParam;

                if (row.EventCode == 138 && !splits.ContainsKey(5))
                    splits.Add(5, row.EventParam);
                else if (row.EventCode == 138 && row.EventParam > 0)
                    splits[5] = row.EventParam;

                if (row.EventCode == 139 && !splits.ContainsKey(6))
                    splits.Add(6, row.EventParam);
                else if (row.EventCode == 139 && row.EventParam > 0)
                    splits[6] = row.EventParam;

                if (row.EventCode == 140 && !splits.ContainsKey(7))
                    splits.Add(7, row.EventParam);
                else if (row.EventCode == 140 && row.EventParam > 0)
                    splits[7] = row.EventParam;

                if (row.EventCode == 141 && !splits.ContainsKey(8))
                    splits.Add(8, row.EventParam);
                else if (row.EventCode == 141 && row.EventParam > 0)
                    splits[8] = row.EventParam;

                if (row.EventCode == 142 && !splits.ContainsKey(9))
                    splits.Add(9, row.EventParam);
                else if (row.EventCode == 142 && row.EventParam > 0)
                    splits[9] = row.EventParam;

                if (row.EventCode == 143 && !splits.ContainsKey(10))
                    splits.Add(10, row.EventParam);
                else if (row.EventCode == 143 && row.EventParam > 0)
                    splits[10] = row.EventParam;

                if (row.EventCode == 144 && !splits.ContainsKey(11))
                    splits.Add(11, row.EventParam);
                else if (row.EventCode == 144 && row.EventParam > 0)
                    splits[11] = row.EventParam;

                if (row.EventCode == 145 && !splits.ContainsKey(12))
                    splits.Add(12, row.EventParam);
                else if (row.EventCode == 145 && row.EventParam > 0)
                    splits[12] = row.EventParam;

                if (row.EventCode == 146 && !splits.ContainsKey(13))
                    splits.Add(13, row.EventParam);
                else if (row.EventCode == 146 && row.EventParam > 0)
                    splits[13] = row.EventParam;

                if (row.EventCode == 147 && !splits.ContainsKey(14))
                    splits.Add(14, row.EventParam);
                else if (row.EventCode == 147 && row.EventParam > 0)
                    splits[14] = row.EventParam;

                if (row.EventCode == 148 && !splits.ContainsKey(15))
                    splits.Add(15, row.EventParam);
                else if (row.EventCode == 148 && row.EventParam > 0)
                    splits[15] = row.EventParam;

                if (row.EventCode == 149 && !splits.ContainsKey(16))
                    splits.Add(16, row.EventParam);
                else if (row.EventCode == 149 && row.EventParam > 0)
                    splits[16] = row.EventParam;
            }

            if (splits.Count == 0)
                for (var i = 0; i < 16; i++)
                    splits.Add(i, 0);
        }
    }
}
