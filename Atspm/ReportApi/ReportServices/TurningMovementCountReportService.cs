#region license
// Copyright 2026 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/TurningMovementCountReportService.cs
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

using System.ComponentModel.DataAnnotations;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    /// <summary>
    /// Turning movement count report service
    /// </summary>
    public class TurningMovementCountReportService : ReportServiceBase<TurningMovementCountsOptions, TurningMovementCountsResult>
    {
        private const string CombinedThruRightMovementType = "Thru + Thru-Right";
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly TurningMovementCountsService turningMovementCountsService;
        private readonly ILocationRepository LocationRepository;
        private readonly PlanService planService;

        /// <inheritdoc/>
        public TurningMovementCountReportService(
            IIndianaEventLogRepository controllerEventLogRepository,
            TurningMovementCountsService turningMovementCountsService,
            ILocationRepository LocationRepository,
            PlanService planService
            )
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.turningMovementCountsService = turningMovementCountsService;
            this.LocationRepository = LocationRepository;
            this.planService = planService;
        }

        /// <inheritdoc/>
        public override async Task<TurningMovementCountsResult> ExecuteAsync(TurningMovementCountsOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            Validator.ValidateObject(parameter, new ValidationContext(parameter), true);
            cancelToken.ThrowIfCancellationRequested();
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);

            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<TurningMovementCountsResult>(new NullReferenceException("Location not found"));
            }

            // A detector's corrected time is raw time + offset - latency. Fetch the raw
            // boundary events that can move into the requested interval after correction.
            var shifts = Location.Approaches
                .SelectMany(approach => approach.GetDetectorsForMetricType(parameter.MetricTypeId))
                .Select(detector => detector.GetOffset() - detector.LatencyCorrection * 1000)
                .Append(0)
                .ToList();
            var queryStart = parameter.Start.AddMilliseconds(-Math.Max(0, shifts.Max()));
            var queryEnd = parameter.End.AddMilliseconds(-Math.Min(0, shifts.Min()));
            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(
                Location.LocationIdentifier, queryStart, queryEnd).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                return new TurningMovementCountsResult
                {
                    Charts = new List<TurningMovementCountsLanesResult>(),
                    Table = new List<TurningMovementCountData>()
                };
            }

            // Capture activity before plan processing can move a boundary event to the report start.
            var hasControllerActivityInRange = controllerEventLogs.Any(e =>
                e.Timestamp >= parameter.Start && e.Timestamp < parameter.End);

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var plans = planService.GetBasicPlans(parameter.Start, parameter.End, parameter.LocationIdentifier, planEvents);
            var tasks = new List<Task<IEnumerable<TurningMovementCountsLanesResult>>>();
            foreach (var laneType in Enum.GetValues(typeof(LaneTypes)))
            {
                cancelToken.ThrowIfCancellationRequested();
                tasks.Add(GetChartDataForLaneType(
                    Location, (LaneTypes)laneType, parameter, controllerEventLogs, plans.ToList()));
            }
            var results = await Task.WhenAll(tasks);

            var finalLaneResultcheck = results.Where(result => result != null).SelectMany(r => r).ToList();

            // Logs in the query padding alone do not establish zero traffic in the requested interval.
            // Corrected detector events may still supply counts even when their raw timestamps are outside it.
            if (!hasControllerActivityInRange && finalLaneResultcheck.All(chart => chart.TotalVolume == 0))
                finalLaneResultcheck.Clear();

            var finalResultcheck = new TurningMovementCountsResult
            {
                Charts = finalLaneResultcheck,
                Table = new List<TurningMovementCountData>()
            };

            //Get Lane results by direction and movement type and bin size anc create a list of TurningMovementCountData for each direction and movement type
            foreach (var direction in Location.Approaches.Select(a => a.DirectionTypeId).Distinct())
            {
                var distinctLaneTypesByDirection = finalLaneResultcheck.Where(r => r.Direction == direction.GetAttributeOfType<DisplayAttribute>().Name).Select(i => i.LaneType).Distinct().ToList();
                foreach (var laneTypeByDirection in distinctLaneTypesByDirection)
                {
                    var laneResultsByDirection = finalLaneResultcheck.Where(r => r.Direction == direction.GetAttributeOfType<DisplayAttribute>().Name && r.LaneType == laneTypeByDirection).ToList();
                    var movementTypes = laneResultsByDirection.Select(r => r.MovementType).Distinct().ToList();
                    foreach (var movementType in movementTypes)
                    {
                        var laneResultsByMovementType = laneResultsByDirection.Where(r => r.MovementType == movementType).ToList();
                        if (laneResultsByMovementType.IsNullOrEmpty())
                        {
                            continue;
                        }
                        var turningMovementCountData = new TurningMovementCountData
                        {
                            Direction = direction.GetAttributeOfType<DisplayAttribute>().Name,
                            LaneType = laneResultsByMovementType.FirstOrDefault().LaneType,
                            MovementType = movementType,
                            DetectorCount = laneResultsByMovementType.Sum(r => r.DetectorCount)
                        };

                        //sum the totalVolumes.value grouped by toalVolume.Start and add to turningMovementCountData.Volumes
                        turningMovementCountData.Volumes = laneResultsByMovementType
                            .SelectMany(r => r.TotalVolumes)
                            .GroupBy(v => v.Timestamp)
                            .Select(g => new DataPointForInt(g.Key, g.Sum(v => v.Value)))
                            .ToList();
                        finalResultcheck.Table.Add(turningMovementCountData);
                    }
                }
            }
            ComputePeakHourAndFactor(finalResultcheck, parameter.Start, parameter.End, parameter.BinSize);
            SetPeakHourVolume(finalResultcheck);
            return finalResultcheck;
        }

        private void SetPeakHourVolume(TurningMovementCountsResult result)
        {
            foreach (var row in result.Table)
            {
                if (!result.PeakHour.HasValue)
                {
                    row.PeakHourVolume = null;
                    continue;
                }

                var start = result.PeakHour.Value.Key;
                var total = result.Charts
                    .Where(c => c.Direction == row.Direction && c.LaneType == row.LaneType && c.MovementType == row.MovementType)
                    .SelectMany(c => c.MinuteVolumes)
                    .Where(v => v.Timestamp >= start && v.Timestamp < start.AddHours(1))
                    .Sum(v => v.Value);
                row.PeakHourVolume = new DataPointForInt(start, total);
            }
        }

        private void ComputePeakHourAndFactor(TurningMovementCountsResult result, DateTime start, DateTime end, int binSize)
        {
            var vehicleLaneType = LaneTypes.V.GetAttributeOfType<DisplayAttribute>().Name;
            var minutes = result.Charts.Where(c => c.LaneType == vehicleLaneType)
                .SelectMany(c => c.MinuteVolumes).ToList();
            var statistics = TurningMovementCountsStatistics.Calculate(minutes, start, end, binSize);
            result.PeakHour = statistics.PeakHour;
            result.PeakHourFactor = statistics.PeakHourFactor;
        }

        private static IReadOnlyList<(string DisplayName, MovementTypes[] MovementTypes)> GetMovementTypeGroups(bool combineThruRight)
        {
            var groups = Enum.GetValues<MovementTypes>()
                .Where(m => !combineThruRight || (m != MovementTypes.T && m != MovementTypes.TR))
                .Select(m => (m.GetAttributeOfType<DisplayAttribute>().Name, new[] { m }))
                .ToList();
            if (combineThruRight)
                groups.Add((CombinedThruRightMovementType, new[] { MovementTypes.T, MovementTypes.TR }));
            return groups;
        }

        private async Task<IEnumerable<TurningMovementCountsLanesResult>> GetChartDataForLaneType(
            Location Location,
            LaneTypes laneType,
            TurningMovementCountsOptions options,
            List<IndianaEvent> controllerEventLogs,
            List<Plan> plans)
        {
            if (!Location.Approaches.SelectMany(a => a.Detectors).Select(d => d.LaneType).Distinct().Contains(laneType))
            {
                return null;
            }
            var directions = Location.Approaches.Select(a => a.DirectionTypeId).Distinct().ToList();
            var tasks = new List<Task<TurningMovementCountsLanesResult>>();
            foreach (var direction in directions)
            {
                var detectorsForDirection = Location.Approaches.Where(a => a.DirectionTypeId == direction).SelectMany(a => a.GetDetectorsForMetricType(options.MetricTypeId)).ToList();

                foreach (var movementTypeGroup in GetMovementTypeGroups(options.CombineThruRight))
                {
                    var movementTypeDetectors = detectorsForDirection
                        .Where(d => movementTypeGroup.MovementTypes.Contains(d.MovementType))
                        .ToList();

                    if (!movementTypeDetectors.IsNullOrEmpty())
                    {
                        tasks.Add(GetChartDataByMovementType(
                            options,
                            plans,
                            controllerEventLogs,
                            movementTypeDetectors,
                            movementTypeGroup.DisplayName,
                            laneType,
                            Location.LocationIdentifier,
                            Location.LocationDescription(),
                            direction));
                    }
                }
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null).OrderBy(r => r.Direction).ThenBy(r => r.MovementType);
        }

        private async Task<TurningMovementCountsLanesResult> GetChartDataByMovementType(
            TurningMovementCountsOptions options,
            List<Plan> planEvents,
            List<IndianaEvent> controllerEventLogs,
            List<Detector> detectors,
            string movementTypeLabel,
            LaneTypes laneType,
            string locationIdentifier,
            string LocationDescription,
            DirectionTypes directionType)
        {
            detectors = detectors.Where(d => d.LaneType == laneType).ToList();
            var detectorEvents = new List<IndianaEvent>();
            // Correct each channel once within this movement and lane type, even if configured more than once.
            foreach (var detector in detectors.OrderBy(d => d.Id).DistinctBy(d => d.DetectorChannel))
            {
                detectorEvents.AddRange(controllerEventLogs.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
                    options.Start,
                    options.End,
                    new List<short>() { 82 },
                    detector.DetectorChannel,
                    detector.GetOffset(),
                    detector.LatencyCorrection).ToList());
            }
            var result = turningMovementCountsService.GetChartData(
                detectors,
                laneType,
                movementTypeLabel,
                directionType,
                options,
                detectorEvents,
                planEvents,
                locationIdentifier,
                LocationDescription);

            return await result;
        }
    }
}
