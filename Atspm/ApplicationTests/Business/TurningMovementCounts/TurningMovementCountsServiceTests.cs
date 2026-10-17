#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Business.TurningMovementCounts/TurningMovementCountsServiceTests.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Business.TurningMovementCounts
{
    public class TurningMovementCountsServiceTests
    {
        private static readonly DateTime Start = new DateTime(2026, 9, 8, 8, 0, 0);

        private static Detector Detector(int channel, int? lane, LaneTypes laneType = LaneTypes.V)
        {
            return new Detector { Id = channel, DetectorChannel = channel, LaneNumber = lane, LaneType = laneType, MovementType = MovementTypes.T };
        }

        private static IndianaEvent Event(int channel, DateTime timestamp, short code = 82)
        {
            return new IndianaEvent { EventParam = (short)channel, Timestamp = timestamp, EventCode = code };
        }

        private static Task<TurningMovementCountsLanesResult> Calculate(List<Detector> detectors, List<IndianaEvent> events, int binSize = 15)
        {
            return new TurningMovementCountsService().GetChartData(detectors, LaneTypes.V, "Thru", DirectionTypes.NB,
                new TurningMovementCountsOptions { Start = Start, End = Start.AddHours(1), BinSize = binSize },
                events, new List<Plan>(), "1", "Test location");
        }

        [Theory]
        [InlineData(5)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(60)]
        public async Task DetectorCountsAreIndependentOfBinSizeAndUnknownLanesArePreserved(int binSize)
        {
            var detectors = new List<Detector> { Detector(1, null), Detector(2, 1), Detector(3, null), Detector(4, 2) };
            var events = new List<IndianaEvent>
            {
                Event(1, Start.AddMinutes(1)), Event(1, Start.AddMinutes(2)),
                Event(2, Start.AddMinutes(3)), Event(4, Start.AddMinutes(31))
            };
            var result = await Calculate(detectors, events, binSize);

            Assert.Equal(4, result.DetectorCount);
            Assert.Equal(4, result.TotalVolume);
            Assert.Equal(3, result.Lanes.Count);
            var unknownLane = result.Lanes.Single(l => l.LaneNumber == null);
            var laneOne = result.Lanes.Single(l => l.LaneNumber == 1);
            var laneTwo = result.Lanes.Single(l => l.LaneNumber == 2);
            Assert.Equal(2, unknownLane.DetectorCount);
            Assert.Equal(1, laneOne.DetectorCount);
            Assert.Equal(1, laneTwo.DetectorCount);
            Assert.Equal(2 * (60 / binSize), unknownLane.Volume.Sum(v => v.Value));
            Assert.Equal(60 / binSize, laneOne.Volume.Sum(v => v.Value));
            Assert.Equal(60 / binSize, laneTwo.Volume.Sum(v => v.Value));
            Assert.Equal(60 / binSize, result.TotalVolumes.Count);
            Assert.Null(result.LaneUtilizationFactor);
            for (var i = 0; i < result.TotalVolumes.Count; i++)
                Assert.Equal(result.TotalVolumes[i].Value * (60 / binSize), result.Lanes.Sum(l => l.Volume[i].Value));
        }

        [Fact]
        public async Task KnownLanesUseLaneVolumesForUtilizationAndCountEachChannelOnce()
        {
            var result = await Calculate(new List<Detector> { Detector(1, 1), Detector(2, 1), Detector(3, 2) },
                new List<IndianaEvent> { Event(1, Start), Event(1, Start.AddMinutes(1)), Event(2, Start), Event(3, Start) });
            Assert.Equal(3, result.DetectorCount);
            Assert.Equal(2, result.Lanes.Single(l => l.LaneNumber == 1).DetectorCount);
            Assert.Equal(1, result.Lanes.Single(l => l.LaneNumber == 2).DetectorCount);
            Assert.Equal(4.0 / (2 * 3), result.LaneUtilizationFactor);
        }

        [Fact]
        public async Task DuplicateChannelAssignmentsDoNotDuplicateCountsOrInventALane()
        {
            var result = await Calculate(new List<Detector> { Detector(1, 1), Detector(1, 2) },
                new List<IndianaEvent> { Event(1, Start) });
            Assert.Equal(1, result.DetectorCount);
            Assert.Equal(1, result.TotalVolume);
            var lane = Assert.Single(result.Lanes);
            Assert.Null(lane.LaneNumber);
            Assert.Equal(1, lane.DetectorCount);
            Assert.Null(result.LaneUtilizationFactor);
        }

        [Fact]
        public async Task EventsAtBinBoundariesAreCountedOnceAndUnrelatedEventsAreExcluded()
        {
            var events = new List<IndianaEvent>
            {
                Event(1, Start.AddTicks(-1)), Event(1, Start), Event(2, Start.AddMinutes(15).AddTicks(-1)),
                Event(1, Start.AddMinutes(15)), Event(2, Start.AddMinutes(30)), Event(1, Start.AddHours(1).AddTicks(-1)),
                Event(2, Start.AddHours(1)), Event(99, Start.AddMinutes(1)), Event(1, Start.AddMinutes(1), 81)
            };
            var result = await Calculate(new List<Detector> { Detector(1, 1), Detector(2, 1) }, events);

            Assert.Equal(2, result.DetectorCount);
            Assert.Equal(5, result.TotalVolume);
            Assert.Equal(new[] { 2, 1, 1, 1 }, result.TotalVolumes.Select(v => v.Value));
            Assert.Equal(new[] { 8, 4, 4, 4 }, Assert.Single(result.Lanes).Volume.Select(v => v.Value));
            Assert.Equal(2, result.Lanes[0].DetectorCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(7)]
        public async Task ExplicitAndUnknownLaneNumbersArePreserved(int explicitLane)
        {
            var detectors = new List<Detector> { Detector(1, null), Detector(2, explicitLane) };
            var result = await Calculate(detectors, new List<IndianaEvent> { Event(1, Start), Event(2, Start) });
            Assert.Equal(new int?[] { null, explicitLane }, result.Lanes.Select(l => l.LaneNumber).OrderBy(n => n));
            Assert.All(result.Lanes, l => Assert.Equal(1, l.DetectorCount));
            Assert.Null(detectors[0].LaneNumber);
            Assert.Equal(explicitLane, detectors[1].LaneNumber);
        }

        [Fact]
        public async Task NoMatchingActivationsKeepsDetectorMetadataAndZeroVolumes()
        {
            var result = await Calculate(new List<Detector> { Detector(1, null), Detector(2, null) },
                new List<IndianaEvent> { Event(99, Start) });
            Assert.Equal(2, result.DetectorCount);
            Assert.Equal(2, Assert.Single(result.Lanes).DetectorCount);
            Assert.Null(result.Lanes[0].LaneNumber);
            Assert.Equal(0, result.TotalVolume);
            Assert.All(result.TotalVolumes, v => Assert.Equal(0, v.Value));
            Assert.Null(result.LaneUtilizationFactor);
            Assert.Null(result.PeakHourFactor);
        }

        [Fact]
        public async Task EmptyEventListReturnsZeroFilledVolumesAndDetectorMetadata()
        {
            var result = await Calculate(new List<Detector> { Detector(1, null) }, new List<IndianaEvent>());
            Assert.Equal(1, result.DetectorCount);
            Assert.Equal(1, Assert.Single(result.Lanes).DetectorCount);
            Assert.Equal(0, result.TotalVolume);
            Assert.Equal(4, result.TotalVolumes.Count);
            Assert.All(result.TotalVolumes, point => Assert.Equal(0, point.Value));
            Assert.Null(result.LaneUtilizationFactor);
            Assert.Null(result.PeakHourFactor);
        }

        [Fact]
        public async Task NoEligibleLaneTypePreservesExistingNoResultBehavior()
        {
            Assert.Null(await Calculate(new List<Detector> { Detector(1, null, LaneTypes.Bike) }, new List<IndianaEvent> { Event(1, Start) }));
            Assert.Null(await Calculate(new List<Detector>(), new List<IndianaEvent> { Event(1, Start) }));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task MissingLaneNumbersStayUnknownAndCountsReflectEachChannelGroup(int? secondLane)
        {
            var start = new DateTime(2026, 9, 8, 8, 0, 0);
            var detectors = new List<Detector>
            {
                new Detector { Id = 1, DetectorChannel = 1, LaneNumber = null, LaneType = LaneTypes.V, MovementType = MovementTypes.T },
                new Detector { Id = 2, DetectorChannel = 2, LaneNumber = secondLane, LaneType = LaneTypes.V, MovementType = MovementTypes.TR },
                new Detector { Id = 3, DetectorChannel = 3, LaneNumber = 1, LaneType = LaneTypes.V, MovementType = MovementTypes.T },
                new Detector { Id = 4, DetectorChannel = 4, LaneNumber = null, LaneType = LaneTypes.Bike, MovementType = MovementTypes.T }
            };
            var events = new List<IndianaEvent>
            {
                new IndianaEvent { Timestamp = start.AddMinutes(1), EventCode = 82, EventParam = 1 },
                new IndianaEvent { Timestamp = start.AddMinutes(2), EventCode = 82, EventParam = 2 },
                new IndianaEvent { Timestamp = start.AddMinutes(3), EventCode = 82, EventParam = 3 },
                new IndianaEvent { Timestamp = start.AddMinutes(4), EventCode = 82, EventParam = 4 }
            };

            var result = await new TurningMovementCountsService().GetChartData(
                detectors, LaneTypes.V, "Thru + Thru-Right", DirectionTypes.NB,
                new TurningMovementCountsOptions { Start = start, End = start.AddHours(1), BinSize = 15, CombineThruRight = true },
                events, new List<Plan>(), "1", "Test location");

            Assert.NotNull(result);
            Assert.Equal(3, result.DetectorCount);
            Assert.Equal(3, result.TotalVolume);
            Assert.Equal(secondLane == 2 ? 3 : 2, result.Lanes.Count);
            Assert.Contains(result.Lanes, lane => lane.LaneNumber == 1);
            Assert.Contains(result.Lanes, lane => lane.LaneNumber == null);
            Assert.All(result.Lanes, lane => Assert.Equal(4 * lane.DetectorCount, lane.Volume.Sum(point => point.Value)));
            Assert.Null(result.LaneUtilizationFactor);
            Assert.Equal(secondLane == 1 ? 2 : 1, result.Lanes.Single(lane => lane.LaneNumber == 1).DetectorCount);
            Assert.Equal(secondLane == null ? 2 : 1, result.Lanes.Single(lane => lane.LaneNumber == null).DetectorCount);
            Assert.Null(detectors[0].LaneNumber);
            if (secondLane == 2)
                Assert.Equal(1, result.Lanes.Single(lane => lane.LaneNumber == 2).DetectorCount);
        }
    }
}
