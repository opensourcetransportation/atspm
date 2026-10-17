#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/CalculateTotalVolumesTests.cs
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

using AutoFixture;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Utah.Udot.Atspm.Analysis.Common;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    public class CalculateTotalVolumesTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;

        public CalculateTotalVolumesTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
        }

        private Volumes GenerateVolumes(string locationIdentifier, int phaseNumber, int detectorChannel, DirectionTypes direction, DateTime start, DateTime end, int count)
        {
            var correctedEventFixture = new Fixture();
            correctedEventFixture.Customize<CorrectedDetectorEvent>(c =>
            {
                return c.With(w => w.LocationIdentifier, locationIdentifier)
                .With(w => w.PhaseNumber, phaseNumber)
                .With(w => w.DetectorChannel, detectorChannel)
                .With(w => w.Direction, direction);
            });
            correctedEventFixture.Customizations.Add(new RandomDateTimeSequenceGenerator(start, end));

            var events = correctedEventFixture.CreateMany<CorrectedDetectorEvent>(count);

            var result = new Volumes(events, TimeSpan.FromMinutes(15))
            {
                LocationIdentifier = locationIdentifier,
                PhaseNumber = phaseNumber,
                Direction = direction,
            };

            result.Segments.ToList().ForEach(f =>
            {
                f.LocationIdentifier = locationIdentifier;
                f.PhaseNumber = phaseNumber;
                f.Direction = direction;
                f.DetectorEvents.AddRange(events.Where(w => f.InRange(w)));
            });

            return result;
        }

        [Fact]
        [Trait(nameof(CalculateTotalVolumes), "Location Filter")]
        public async void CalculateTotalVolumesLocationFilterTest()
        {
            Assert.True(false);
        }

        [Fact]
        [Trait(nameof(CalculateTotalVolumes), "Detector Filter")]
        public async void CalculateTotalVolumesDetectorFilterTest()
        {
            Assert.True(false);
        }

        [Fact]
        [Trait(nameof(CalculateTotalVolumes), "Data Check")]
        public async void CalculateTotalVolumesDataCheckTest()
        {
            //var start = DateTime.Parse("4/17/2023 8:00:00");
            //var end = DateTime.Parse("4/17/2023 9:00:00");

            //var approachP = _testLocation.Approaches.FirstOrDefault(f => f.Id == 2880);
            //var approachO = _testLocation.Approaches.FirstOrDefault(f => f.Id == 2882);

            //var primaryVolumes = GenerateVolumes(_testLocation.LocationIdentifier, approachP.ProtectedPhaseNumber, 2, approachP.DirectionTypeId, start, end, 10);
            //var opposingVolumes = GenerateVolumes(_testLocation.LocationIdentifier, approachO.ProtectedPhaseNumber, 6, approachO.DirectionTypeId, start, end, 10);

            //var t1 = Tuple.Create(approachP, primaryVolumes);
            //var t2 = Tuple.Create(approachO, opposingVolumes);

            //var testData = Tuple.Create(t1, t2);

            //var sut = new CalculateTotalVolumes();

            //var result = await sut.ExecuteAsync(testData);

            //_output.WriteLine($"approach: {result.Item1}");
            //_output.WriteLine($"total volume: {result.Item2}");

            //foreach (var s in result.Item2.Segments)
            //{
            //    _output.WriteLine($"segments: {s}");
            //}

            Assert.True(false);
        }

        [Fact]
        [Trait(nameof(CalculateTotalVolumes), "Start/End Check")]
        public async void CalculatePhaseStartEndCheckTest()
        {
            //var detector = _testApproach.Detectors.First();

            //var testEvents = new List<CorrectedDetectorEvent>
            //{
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:14:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:15:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:16:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:20:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:45:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:50:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:55:00"), DetectorChannel = detector.DetectorChannel},

            //}.AsEnumerable();

            //var testData = Tuple.Create(_testApproach, testEvents);

            //var sut = new CalculateTotalVolumes();

            //var result = await sut.ExecuteAsync(testData);

            //_output.WriteLine($"approach: {result.Item1}");

            //foreach (var v in result.Item2)
            //{
            //    _output.WriteLine($"volume: {v}");
            //}

            //Assert.Equal(DateTime.Parse("4/17/2023 8:00:00"), result.Item2.Start);
            //Assert.Equal(DateTime.Parse("4/17/2023 9:00:00"), result.Item2.End);

            Assert.True(false);
        }

        [Fact]
        [Trait(nameof(CalculateTotalVolumes), "Time Segment Check")]
        public async void CalculatePhaseTimeSegmentCheckTest()
        {
            //var detector = _testApproach.Detectors.First();

            //var testEvents = new List<CorrectedDetectorEvent>
            //{
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:14:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:15:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:16:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:20:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:45:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:50:00"), DetectorChannel = detector.DetectorChannel},
            //    new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:55:00"), DetectorChannel = detector.DetectorChannel},

            //}.AsEnumerable();

            //var testData = Tuple.Create(_testApproach, testEvents);

            //var sut = new CalculateTotalVolumes();

            //var result = await sut.ExecuteAsync(testData);

            //_output.WriteLine($"approach: {result.Item1}");

            //foreach (var v in result.Item2)
            //{
            //    _output.WriteLine($"volume: {v}");
            //}

            //Assert.Collection(result.Item2,
            //    a => Assert.True(a.Count == 2),
            //    a => Assert.True(a.Count == 3),
            //    a => Assert.True(a.Count == 0),
            //    a => Assert.True(a.Count == 3));

            Assert.True(false);
        }

        [Fact]
        [Trait(nameof(CalculateTotalVolumes), "Null Input")]
        public async void CalculateTotalVolumesNullInputTest()
        {
            //var testData = Tuple.Create<Approach, IEnumerable<CorrectedDetectorEvent>>(null, null);

            //var sut = new CalculateTotalVolumes();

            //var result = await sut.ExecuteAsync(testData);

            //Assert.True(result != null);
            //Assert.True(result.Item1 == null);
            //Assert.True(result.Item2 == null);

            Assert.True(false);
        }

        [Fact]
        [Trait(nameof(CalculateTotalVolumes), "No Data")]
        public async void CalculateTotalVolumesNoDataTest()
        {
            //var testLogs = Enumerable.Range(1, 5).Select(s => new CorrectedDetectorEvent()
            //{
            //    LocationIdentifier = "1001",
            //    Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
            //    DetectorChannel = 5
            //});

            //var testData = Tuple.Create(_testApproach, testLogs);

            //var sut = new CalculateTotalVolumes();

            //var result = await sut.ExecuteAsync(testData);

            //Assert.True(result != null);
            //Assert.True(result.Item1 == _testApproach);
            //Assert.True(result.Item2 == null);

            Assert.True(false);
        }

        [Theory]
        [InlineData("CalculateTotalVolumesTestData1.json")]
        [Trait(nameof(CalculateTotalVolumes), "From File")]
        public async void CalculateTotalVolumesFromFileTest(string file)
        {
            var json = TestDataFiles.ReadAllText(file);
            var testFile = JsonConvert.DeserializeObject<CalculateTotalVolumeTestData>(json);

            _output.WriteLine($"Configuration: {testFile.Configuration}");
            _output.WriteLine($"Input: {testFile.Input.Count}");
            _output.WriteLine($"Output: {testFile.Output.Segments.Count}");

            var t1 = Tuple.Create(testFile.Configuration[0], testFile.Input[0]);
            var t2 = Tuple.Create(testFile.Configuration[1], testFile.Input[1]);

            var testData = Tuple.Create(t1, t2);

            var sut = new CalculateTotalVolumes();

            var result = await sut.ExecuteAsync(testData);

            var expected = testFile.Output;
            var actual = result.Item2;

            //_output.WriteLine($"expected: {expected.Count}");
            //_output.WriteLine($"actual: {actual.Count}");

            Assert.Equivalent(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
