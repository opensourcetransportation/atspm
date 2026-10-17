#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/IndianaEventExtensions.cs
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

using Utah.Udot.Atspm.Analysis.PedestrianDelay;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Provides extension methods for working with collections of <see cref="IndianaEvent"/> objects.
    /// </summary>
    public static class IndianaEventExtensions
    {
        /// <summary>
        /// Filters the collection of <see cref="IndianaEvent"/> to include only those events whose <see cref="IndianaEvent.EventCode"/>
        /// matches any of the specified codes.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to filter.</param>
        /// <param name="codes">The event codes to include.</param>
        /// <returns>An enumerable of <see cref="IndianaEvent"/> matching the specified codes.</returns>
        public static IEnumerable<IndianaEvent> WhereCode(this IEnumerable<IndianaEvent> events, params short[] codes)
        {
            var codeSet = new HashSet<short>(codes);
            return events.Where(e => codeSet.Contains(e.EventCode));
        }

        /// <summary>
        /// Filters the collection of <see cref="IndianaEvent"/> to include only those events whose <see cref="IndianaEvent.EventParam"/>
        /// matches any of the specified parameters.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to filter.</param>
        /// <param name="param">The event parameters to include.</param>
        /// <returns>An enumerable of <see cref="IndianaEvent"/> matching the specified parameters.</returns>
        public static IEnumerable<IndianaEvent> WhereParam(this IEnumerable<IndianaEvent> events, params short[] param)
        {
            var paramSet = new HashSet<short>(param);
            return events.Where(e => paramSet.Contains(e.EventParam));
        }

        /// <summary>
        /// Creates a lookup from <see cref="IndianaEvent.EventCode"/> to <see cref="IndianaEvent"/> for fast event code-based grouping.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to group.</param>
        /// <returns>An <see cref="ILookup{TKey, TElement}"/> keyed by event code.</returns>
        public static ILookup<short, IndianaEvent> ToCodeLookup(this IEnumerable<IndianaEvent> events)
        {
            return events.ToLookup(e => e.EventCode);
        }

        /// <summary>
        /// Creates a lookup from <see cref="IndianaEvent.EventParam"/> to <see cref="IndianaEvent"/> for fast event parameter-based grouping.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to group.</param>
        /// <returns>An <see cref="ILookup{TKey, TElement}"/> keyed by event parameter.</returns>
        public static ILookup<short, IndianaEvent> ToParamLookup(this IEnumerable<IndianaEvent> events)
        {
            return events.ToLookup(e => e.EventParam);
        }

        public static IReadOnlyList<GreenToGreenCycle> IdentifyGreenToGreenCycles(this IEnumerable<IndianaEvent> events)
        {
            var expectedSequence = new List<short>()
            {
                (short)IndianaEnumerations.PhaseBeginGreen,
                (short)IndianaEnumerations.PhaseBeginYellowChange,
                (short)IndianaEnumerations.PhaseEndYellowChange,
                (short)IndianaEnumerations.PhaseBeginGreen,};

            return events.IdentifyPhaseCycles<GreenToGreenCycle>(expectedSequence);
        }

        /// <summary>
        /// <list type="number">
        /// <listheader>Steps to create the <see cref="RedToRedCycle"/></listheader>
        /// 
        /// <item>
        /// <term>Identify the Beginning of Each Cycle</term>
        /// <description>
        /// The beginning of the cycle
        /// for a given phase is defined as the end of <see cref="IndianaEnumerations.PhaseEndYellowChange"/>. The
        /// event log is queried to find the records where the Event Code is 9. Each instance
        /// of <see cref="IndianaEnumerations.PhaseEndYellowChange"/> is indicated as the start of the cycle.
        /// </description>
        /// </item>
        /// 
        /// <item>
        /// <term>Identify the Change to Green for Each Cycle</term>
        /// <description>
        /// During this step, the event log is queried to find the records where the Event Code <see cref=")IndianaEnumerations.PhaseBeginGreen"/>.
        /// The duration from the beginning of the cycle to when the given phasechanges to green(total red interval)
        /// is calculated in reference to the first redevent (begin) of the cycle
        /// </description>
        /// </item>
        /// 
        /// <item>
        /// <term>Identify the Change to Yellow for Each Cycle</term>
        /// <description>
        /// During this step, the event log is queried to find the record where the Event Code <see cref="IndianaEnumerations.PhaseBeginYellowChange"/>.
        /// The duration from the beginning of the cycle to when the given phase
        /// changes to yellow(total green interval) is calculated in reference to the first red event (begin) of the cycle
        /// </description>
        /// </item>
        /// 
        /// <item>
        /// <term>Identify the Change to Red at the End of Each Cycle</term>
        /// <description>
        /// During this step, the event log is queried to find the records where the Event Code <see cref="IndianaEnumerations.PhaseEndYellowChange"/>. 
        /// The duration from the beginning of the cycle to when the given phase changes to red(yellow clearance interval)
        /// is calculated in reference to the firstred event (begin) of the cycle
        /// </description>
        /// </item>
        /// 
        /// </list>
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public static IReadOnlyList<RedToRedCycle> IdentifyRedToRedCycles(this IEnumerable<IndianaEvent> events)
        {
            var expectedSequence = new List<short>()
            {
                (short)IndianaEnumerations.PhaseEndYellowChange,
                (short)IndianaEnumerations.PhaseBeginGreen,
                (short)IndianaEnumerations.PhaseBeginYellowChange,
                (short)IndianaEnumerations.PhaseEndYellowChange };

            return events.IdentifyPhaseCycles<RedToRedCycle>(expectedSequence);
        }

        /// <summary>
        /// Identifies phase cycles from a sequence of <see cref="IndianaEvent"/> instances
        /// based on a specified cycle sequence.
        /// </summary>
        /// <typeparam name="T">
        /// The type of cycle aggregation to return. Must inherit from <see cref="CycleBase"/> and have a parameterless constructor.
        /// </typeparam>
        /// <param name="events">
        /// The collection of <see cref="IndianaEvent"/> instances to analyze.
        /// </param>
        /// <param name="cycleSequence">
        /// The ordered sequence of event codes that defines a valid phase cycle.
        /// </param>
        /// <returns>
        /// A read-only list of <typeparamref name="T"/> instances representing identified phase cycles.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method filters the input events using <see cref="IndianaPhaseCycleChangesDataSpecification"/> and groups them
        /// by location identifier and phase number. It then applies sequential filtering to retain only the first occurrences
        /// of key events (green, yellow change, end of yellow change).
        /// </para>
        /// <para>
        /// Using a sliding window, the method matches event sequences against the provided <paramref name="cycleSequence"/>.
        /// For each matching window, it constructs a new <typeparamref name="T"/> cycle object with start and end timestamps,
        /// and interval spans for green, yellow, and red phases.
        /// </para>
        /// </remarks>
        public static IReadOnlyList<T> IdentifyPhaseCycles<T>(this IEnumerable<IndianaEvent> events, IEnumerable<short> cycleSequence) where T : CycleBase, new()
        {
            var filtered = events
                .FromSpecification(new IndianaPhaseCycleChangesDataSpecification());

            return filtered
                .GroupBy(e => (e.LocationIdentifier, PhaseNumber: e.EventParam))
                .SelectMany(group =>
                {
                    var windows = group
                        .OrderBy(e => e.Timestamp)
                        //.KeepFirstSequentialEvent(IndianaEnumerations.PhaseBeginGreen)
                        //.KeepFirstSequentialEvent(IndianaEnumerations.PhaseBeginYellowChange)
                        //.KeepFirstSequentialEvent(IndianaEnumerations.PhaseEndYellowChange)
                        .SlidingWindow(cycleSequence.Count())
                        .Where(window => window.Select(e => e.EventCode).SequenceEqual(cycleSequence));

                    return windows.Select(window =>
                    {
                        var pairs = window.SlidingWindow(2);

                        var greenPair = pairs.FirstOrDefault(p =>
                            p[0].EventCode == (short)IndianaEnumerations.PhaseBeginGreen &&
                            p[1].EventCode == (short)IndianaEnumerations.PhaseBeginYellowChange);

                        var yellowPair = pairs.FirstOrDefault(p =>
                            p[0].EventCode == (short)IndianaEnumerations.PhaseBeginYellowChange &&
                            p[1].EventCode == (short)IndianaEnumerations.PhaseEndYellowChange);

                        var redPair = pairs.FirstOrDefault(p =>
                            p[0].EventCode == (short)IndianaEnumerations.PhaseEndYellowChange &&
                            p[1].EventCode == (short)IndianaEnumerations.PhaseBeginGreen);

                        return new T
                        {
                            LocationIdentifier = group.Key.LocationIdentifier,
                            PhaseNumber = group.Key.PhaseNumber,
                            Start = window[0].Timestamp,
                            End = window[cycleSequence.Count() - 1].Timestamp,
                            GreenInterval = greenPair != null ? new StartEndRange { Start = greenPair[0].Timestamp, End = greenPair[1].Timestamp } : null,
                            YellowInterval = yellowPair != null ? new StartEndRange { Start = yellowPair[0].Timestamp, End = yellowPair[1].Timestamp } : null,
                            RedInterval = redPair != null ? new StartEndRange { Start = redPair[0].Timestamp, End = redPair[1].Timestamp } : null
                        };
                    });
                })
                .ToList();
        }



        /// <summary>
        /// Counts the number of imputed pedestrian calls within the specified range.
        /// Imputed calls are detected by finding a sequence where a pedestrian call event (code 90) follows a walk or overlap event (code 21, 67, or 0).
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to analyze.</param>
        /// <param name="range">The time range to consider for imputed calls.</param>
        /// <returns>The count of imputed pedestrian calls in the specified range.</returns>
        public static int CountImputedCalls(this IEnumerable<IndianaEvent> events, StartEndRange range)
        {
            var filter = events
                .Where(e => e.EventCode is 90 or 21 or 67 or 0)
                .OrderBy(e => e.Timestamp)
                .ToList();

            return filter.SlidingWindow(2)
                .Where(x => x[0].EventCode is 21 or 67 or 0 && x[1].EventCode == 90)
                .Count(w => range.InRange(w[0].Timestamp));
        }

        /// <summary>
        /// Counts the number of unique pedestrian detector activations within the specified range.
        /// A unique detection is defined as a detector activation (code 90) that occurs at least 15 seconds after the previous activation.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to analyze.</param>
        /// <param name="range">The time range to consider for unique detections.</param>
        /// <returns>The count of unique pedestrian detector activations in the specified range.</returns>
        public static int CountUniquePedDetections(this IEnumerable<IndianaEvent> events, StartEndRange range)
        {
            var filter = events
                .Where(w => w.EventCode == (short)IndianaEnumerations.PedDetectorOn)
                .OrderBy(o => o.Timestamp)
                .ToList();

            return filter.SlidingWindow(2)
                .Where(x => x[1].Timestamp - x[0].Timestamp > TimeSpan.FromSeconds(15))
                .Count(w => range.InRange(w[1].Timestamp));
        }

        /// <summary>
        /// Returns all pedestrian detector activation events (<see cref="IndianaEnumerations.PedDetectorOn"/>) within the specified time range.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to filter.</param>
        /// <param name="range">The time range to include events.</param>
        /// <returns>A read-only list of <see cref="IndianaEvent"/> representing pedestrian detector activations in the range.</returns>
        public static IReadOnlyList<IndianaEvent> PedRequests(this IEnumerable<IndianaEvent> events, StartEndRange range)
        {
            return events.Where(w => w.EventCode == (short)IndianaEnumerations.PedDetectorOn && range.InRange(w.Timestamp)).ToList();
        }

        /// <summary>
        /// Returns all pedestrian call registered events (<see cref="IndianaEnumerations.PedestrianCallRegistered"/>) within the specified time range.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to filter.</param>
        /// <param name="range">The time range to include events.</param>
        /// <returns>A read-only list of <see cref="IndianaEvent"/> representing pedestrian call registrations in the range.</returns>
        public static IReadOnlyList<IndianaEvent> PedCallsRegistered(this IEnumerable<IndianaEvent> events, StartEndRange range)
        {
            return events.Where(w => w.EventCode == (short)IndianaEnumerations.PedestrianCallRegistered && range.InRange(w.Timestamp)).ToList();
        }

        /// <summary>
        /// Identifies pedestrian cycles from a sequence of <see cref="IndianaEvent"/> objects.
        /// Matches walk and change interval events to construct a list of <see cref="PedCycle"/> instances,
        /// each representing a pedestrian service interval with associated event counts and metrics.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to analyze for pedestrian cycles.</param>
        /// <param name="isPedPhaseOverlap">
        /// If <c>true</c>, uses overlap pedestrian event codes for matching cycles; otherwise, uses standard pedestrian event codes.
        /// </param>
        /// <returns>
        /// A read-only list of <see cref="PedCycle"/> objects, each representing a detected pedestrian cycle
        /// with timing and event count information for walk intervals, detector requests, imputed calls, unique detections, and call registrations.
        /// </returns>
        public static IReadOnlyList<PedCycle> IdentifyPedCycles(this IEnumerable<IndianaEvent> events, bool isPedPhaseOverlap = false)
        {
            var (a, b) = isPedPhaseOverlap
                ? ((short)IndianaEnumerations.PedestrianOverlapBeginWalk, (short)IndianaEnumerations.PedestrianOverlapBeginClearance)
                : ((short)IndianaEnumerations.PedestrianBeginWalk, (short)IndianaEnumerations.PedestrianBeginChangeInterval);

            var filter = events
                .WhereCode([
                    (short)a,
                            (short)b
                ])
                .KeepFirstSequentialEvent(IndianaEnumerations.PedestrianBeginChangeInterval)
                .OrderBy(o => o.Timestamp)
                .ToList();

            var result = filter.SlidingWindow(3)
                .Where(w => w[1].EventCode == 21)
                .Select(s =>
                {
                    var test = new PedCycle()
                    {
                        Start = s[0].EventCode == 21 ? s[1].Timestamp : s[0].Timestamp,
                        PedestrianBeginWalk = s[1].Timestamp,
                        End = s[2].Timestamp,
                    };

                    test.PedRequests = events.PedRequests(test);
                    test.UniquePedDetections = events.CountUniquePedDetections(test);
                    test.ImputedCalls = events.CountImputedCalls(test);
                    test.PedCallsRegistered = events.PedCallsRegistered(test);

                    return test;
                }).ToList();

            return result;
        }

        /// <summary>
        /// Identifies pedestrian cycles from a sequence of <see cref="IndianaEvent"/> objects.
        /// Filters and matches pedestrian detector activations with corresponding walk and change interval events
        /// to construct a list of <see cref="PedDelayCycle"/> instances representing pedestrian service intervals.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to analyze for pedestrian cycles.</param>
        /// <param name="isPedPhaseOverlap">
        /// If <c>true</c>, uses overlap pedestrian event codes for matching cycles; otherwise, uses standard pedestrian event codes.
        /// </param>
        /// <returns>
        /// A read-only list of <see cref="PedDelayCycle"/> objects, each representing a detected pedestrian cycle
        /// with timing information for detector activation, walk start, and change interval.
        /// </returns>
        public static IReadOnlyList<PedDelayCycle> IdentifyPedDelayCycles(this IEnumerable<IndianaEvent> events, bool isPedPhaseOverlap = false)
        {
            var (a, b) = isPedPhaseOverlap
                ? ((short)IndianaEnumerations.PedestrianOverlapBeginWalk, (short)IndianaEnumerations.PedestrianOverlapBeginClearance)
                : ((short)IndianaEnumerations.PedestrianBeginWalk, (short)IndianaEnumerations.PedestrianBeginChangeInterval);

            var filter = events
                .WhereCode([
                    (short)a,
                    (short)b,
                    (short)IndianaEnumerations.PedDetectorOn
                ])
                .OrderBy(o => o.Timestamp)
                .KeepFirstSequentialEvent(IndianaEnumerations.PedDetectorOn)
                .ToList();

            var result = filter
                .Select((t, index) => new { Item = t, Index = index })
                .Where(x => x.Item.EventCode == 90 && x.Index > 0 && x.Index < filter.Count - 1)
                .Select(x =>
                {
                    var prev = filter[x.Index - 1];
                    var next = filter[x.Index + 1];

                    var pedCycle = new PedDelayCycle()
                    {
                        PedDetectorOn = x.Item.Timestamp
                    };

                    if (prev.EventCode == a && next.EventCode == b)
                    {
                        pedCycle.Start = prev.Timestamp;
                        pedCycle.BeginWalk = prev.Timestamp;
                        pedCycle.End = next.Timestamp;
                    }

                    else if (prev.EventCode == a && next.EventCode == a)
                    {
                        pedCycle.Start = prev.Timestamp;
                        pedCycle.BeginWalk = next.Timestamp;
                        pedCycle.End = next.Timestamp;
                    }

                    else if (prev.EventCode == b && next.EventCode == a)
                    {
                        pedCycle.Start = prev.Timestamp;
                        pedCycle.BeginWalk = next.Timestamp;
                        pedCycle.End = next.Timestamp;
                    }

                    return pedCycle;
                })
                .Where(w => w.BeginWalk > DateTime.MinValue)
                .ToList();

            return result;
        }

        /// <summary>
        /// Filters a sequence of events to keep only the first occurrence in any consecutive run of the same EventCode.
        /// </summary>
        /// <param name="events">The sequence of IndianaEvents to process.</param>
        /// <returns>An enumerable containing events where no two adjacent items share the same EventCode.</returns>
        public static IEnumerable<IndianaEvent> KeepFirstSequentialEvent(this IEnumerable<IndianaEvent> events)
        {
            var sortedEvents = events.OrderBy(o => o.Timestamp).ToList();
            return sortedEvents.Where((w, i) => i == 0 || w.EventCode != sortedEvents[i - 1].EventCode);
        }

        /// <summary>
        /// Filters a sequence of events to ensure that a specific EventCode does not appear consecutively. 
        /// Other EventCodes are preserved as they appear in the timeline.
        /// </summary>
        /// <param name="events">The sequence of IndianaEvents to process.</param>
        /// <param name="eventCode">The specific EventCode to monitor for sequential duplicates.</param>
        /// <returns>An enumerable where the specified EventCode only appears when it represents a state change.</returns>
        public static IEnumerable<IndianaEvent> KeepFirstSequentialEvent(this IEnumerable<IndianaEvent> events, IndianaEnumerations eventCode)
        {
            var sort = events.OrderBy(o => o.Timestamp).ToList();
            return sort.Where((w, i) => i == 0 || w.EventCode != (int)eventCode || (w.EventCode == (int)eventCode && w.EventCode != sort[i - 1].EventCode));
        }

        /// <summary>
        /// Filters a sequence of events to keep only the first occurrence in any consecutive run of the same EventParam.
        /// </summary>
        /// <param name="events">The sequence of IndianaEvents to process.</param>
        /// <returns>An enumerable containing events where no two adjacent items share the same EventParam.</returns>
        public static IEnumerable<IndianaEvent> KeepFirstSequentialParam(this IEnumerable<IndianaEvent> events)
        {
            var sortedEvents = events.OrderBy(o => o.Timestamp).ToList();
            return sortedEvents.Where((w, i) => i == 0 || w.EventParam != sortedEvents[i - 1].EventParam);
        }

        /// <summary>
        /// Filters a sequence of events to ensure that a specific EventParam value does not appear consecutively.
        /// Other EventParam values are preserved as they appear in the timeline.
        /// </summary>
        /// <param name="events">The sequence of IndianaEvents to process.</param>
        /// <param name="param">The specific parameter value to monitor for sequential duplicates.</param>
        /// <returns>An enumerable where the specified parameter only appears when it represents a state change.</returns>
        public static IEnumerable<IndianaEvent> KeepFirstSequentialParam(this IEnumerable<IndianaEvent> events, short param)
        {
            var sort = events.OrderBy(o => o.Timestamp).ToList();
            return sort.Where((w, i) => i == 0 || w.EventParam != (int)param || (w.EventParam == (int)param && w.EventParam != sort[i - 1].EventParam));
        }

        /// <summary>
        /// Groups a collection of <see cref="IndianaEvent"/> by their parameter type, as determined by the
        /// <see cref="IndianaEventLayerAttribute"/> on the corresponding <see cref="IndianaEnumerations"/> value.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to group.</param>
        /// <returns>
        /// An <see cref="ILookup{TKey, TElement}"/> where the key is the parameter type as a string,
        /// and the value is a collection of <see cref="IndianaEvent"/> with that parameter type.
        /// </returns>
        public static ILookup<string, IndianaEvent> GroupEventsByParamType(this IEnumerable<IndianaEvent> events)
        {
            var codeToParamType = Enum.GetValues(typeof(IndianaEnumerations))
                .Cast<IndianaEnumerations>()
                .ToDictionary(
                    e => (short)e,
                    e => e.GetAttributeOfType<IndianaEventLayerAttribute>()?.IndianaEventParamType.ToString() ?? IndianaEventParamType.None.ToString()
                );

            return events
                .Where(e => codeToParamType.ContainsKey(e.EventCode))
                .ToLookup(e => codeToParamType[e.EventCode]);
        }

        /// <summary>
        /// Parses raw vehicle detector transition events into continuous presence intervals.
        /// </summary>
        /// <param name="events">The collection of detector events.</param>
        /// <param name="start">The start time of the analysis window.</param>
        /// <param name="end">The end time of the analysis window.</param>
        /// <returns>A collection of continuous presence intervals as start and end timestamps.</returns>
        public static IEnumerable<Tuple<DateTime, DateTime>> GetPresenceIntervals(this IEnumerable<IndianaEvent> events, DateTime start, DateTime end)
        {
            var sorted = events
                .Where(e => e.EventCode == (short)IndianaEnumerations.VehicleDetectorOn || e.EventCode == (short)IndianaEnumerations.VehicleDetectorOff)
                .OrderBy(e => e.Timestamp)
                .ToList();

            if (!sorted.Any())
            {
                yield break;
            }

            DateTime? currentOnTime = null;

            if (sorted[0].EventCode == (short)IndianaEnumerations.VehicleDetectorOff)
            {
                currentOnTime = start;
            }

            foreach (var ev in sorted)
            {
                if (ev.EventCode == (short)IndianaEnumerations.VehicleDetectorOn)
                {
                    if (!currentOnTime.HasValue)
                    {
                        currentOnTime = ev.Timestamp;
                    }
                }
                else if (ev.EventCode == (short)IndianaEnumerations.VehicleDetectorOff && currentOnTime.HasValue)
                {
                    yield return Tuple.Create(currentOnTime.Value, ev.Timestamp);
                    currentOnTime = null;
                }
            }

            if (currentOnTime.HasValue)
            {
                yield return Tuple.Create(currentOnTime.Value, end);
            }
        }
    }
}
