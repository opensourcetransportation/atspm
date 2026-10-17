#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/AggregatePreemptStepTests.cs
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
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Extensions;
using Xunit;
using Xunit.Abstractions;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    /// <summary>
    /// Unit tests for the AggregatePreemptStep workflow step.
    /// </summary>
    public class AggregatePreemptStepTests : WorkflowStepTestBase<AggregatePreemptStep, AggregatePreemptTestData, Location, IEnumerable<IndianaEvent>, IEnumerable<PreemptionAggregation>>
    {
        /// <summary>
        /// Initializes a new instance of the AggregatePreemptStepTests class.
        /// </summary>
        /// <param name="output">The xUnit test output helper.</param>
        /// <param name="testLocationFixture">The test location class fixture.</param>
        public AggregatePreemptStepTests(ITestOutputHelper output, TestLocationFixture testLocationFixture) : base(output, testLocationFixture) { }

        /// <inheritdoc/>
        protected override Location DefaultTestConfig => TestLocation;

        /// <inheritdoc/>
        protected override IEnumerable<IndianaEvent> DefaultTestInput => new List<IndianaEvent>();

        /// <inheritdoc/>
        protected override AggregatePreemptStep CreateStep(Location config, IEnumerable<IndianaEvent> input, IEnumerable<PreemptionAggregation> expected)
        {
            var start = input != null && input.Any() ? input.Min(e => e.Timestamp).Date : new DateTime(2025, 12, 25);
            var timeline = new Timeline<StartEndRange>(start, start.AddDays(1), TimeSpan.FromMinutes(15));
            return new AggregatePreemptStep(timeline);
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<PreemptionAggregation>> ExecuteStepAsync(AggregatePreemptStep step, Location config, IEnumerable<IndianaEvent> input, CancellationToken cancelToken = default)
        {
            return step.ExecuteAsync(Tuple.Create(config, input), cancelToken);
        }

        /// <summary>
        /// Verifies that an empty event stream does not crash the step and produces zero-filled binned rows.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePreemptStep), "EmptyEvents")]
        public async Task Process_EmptyEvents_ReturnsZeroFilledBins()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregatePreemptStep(timeline);

            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)new List<IndianaEvent>()));

            var list = result.ToList();
            Assert.Empty(list);
        }

        /// <summary>
        /// Verifies that preempt call input on events (EC 102) are correctly counted in the segment.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePreemptStep), "PreemptRequests")]
        public async Task Process_PreemptCallOn_CountsCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregatePreemptStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(5), EventCode = 102, EventParam = 1 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(10), EventCode = 102, EventParam = 1 }
            };

            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events));

            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var bin1 = list.FirstOrDefault(a => a.Start == start && a.PreemptNumber == 1);
            Assert.NotNull(bin1);
            Assert.Equal(2, bin1.PreemptRequests);
            Assert.Equal(0, bin1.PreemptServices);
        }

        /// <summary>
        /// Verifies that preempt entry started events (EC 105) are correctly counted in the segment.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePreemptStep), "PreemptServices")]
        public async Task Process_PreemptEntryStarted_CountsCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregatePreemptStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(5), EventCode = 105, EventParam = 1 }
            };

            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events));

            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var bin1 = list.FirstOrDefault(a => a.Start == start && a.PreemptNumber == 1);
            Assert.NotNull(bin1);
            Assert.Equal(0, bin1.PreemptRequests);
            Assert.Equal(1, bin1.PreemptServices);
        }

        /// <summary>
        /// Verifies that preempt numbers are isolated and grouped separately.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePreemptStep), "PreemptIsolation")]
        public async Task Process_PreemptIsolation_IgnoresOtherPreemptNumbers()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(15), TimeSpan.FromMinutes(15));
            var sut = new AggregatePreemptStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(5), EventCode = 102, EventParam = 1 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(6), EventCode = 102, EventParam = 2 }
            };

            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events));

            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var p1 = list.FirstOrDefault(a => a.PreemptNumber == 1);
            var p2 = list.FirstOrDefault(a => a.PreemptNumber == 2);

            Assert.NotNull(p1);
            Assert.Equal(1, p1.PreemptRequests);

            Assert.NotNull(p2);
            Assert.Equal(1, p2.PreemptRequests);
        }

        /// <summary>
        /// Verifies that preempt events falling outside the timeline boundaries are excluded.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePreemptStep), "BoundaryExclusion")]
        public async Task Process_BoundaryEvents_ExcludesCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(15), TimeSpan.FromMinutes(15));
            var sut = new AggregatePreemptStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(-5), EventCode = 102, EventParam = 1 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(20), EventCode = 102, EventParam = 1 }
            };

            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events));

            var list = result.ToList();
            Assert.Single(list);
            Assert.All(list, a => Assert.Equal(0, a.PreemptRequests));
            Assert.All(list, a => Assert.Equal(0, a.PreemptServices));
        }

        private Location CreateLocalMockLocation()
        {
            return new Location { LocationIdentifier = "MOCK_7573" };
        }
    }
}
