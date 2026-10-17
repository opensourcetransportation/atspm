using Moq;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.ReportApi.ReportServices;
using Utah.Udot.Atspm.Repositories.AggregationRepositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.ATSPM.ReportApi.DataAggregation;

namespace ConfigApiTests.ReportServices
{
    [TestClass()]
    public class PedestrianAggregationServiceTests
    {
        private Mock<ILocationRepository> _mockLocationRepo;
        private Mock<IPhasePedAggregationRepository> _mockPedRepo;
        private Mock<IPhaseCycleAggregationRepository> _mockCycleRepo;
        private Mock<IIndianaEventLogRepository> _mockEventRepo;
        private Mock<PhaseService> _mockPhaseService;
        private PedestrianAggregationService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockLocationRepo = new Mock<ILocationRepository>();
            _mockPedRepo = new Mock<IPhasePedAggregationRepository>();
            _mockCycleRepo = new Mock<IPhaseCycleAggregationRepository>();

            _service = new PedestrianAggregationService(
                _mockLocationRepo.Object,
                _mockPedRepo.Object,
                _mockCycleRepo.Object
            );
        }

        [TestMethod]
        public async Task ExecuteAsync_ReturnsExpectedData()
        {
            // Arrange
            var locationId = "1";
            var startDate = new DateTime(2025, 9, 1);
            var endDate = new DateTime(2025, 9, 2);

            _mockLocationRepo.Setup(x => x.GetLatestVersionOfLocation(locationId, startDate))
                .Returns(new Location
                {
                    LocationIdentifier = locationId,
                    PrimaryName = "Main St",
                    SecondaryName = "1st Ave",
                    Latitude = 40.0,
                    Longitude = -111.0,
                    Areas = new List<Area> { new Area { Name = "Downtown" } }
                });

            _mockPedRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, startDate, endDate))
                .Returns(new List<PhasePedAggregation>
                {
                    new PhasePedAggregation
                    {
                        Start = startDate,
                        End = startDate.AddHours(1),
                        PhaseNumber = 1,
                        PedBeginWalkCount = 10,
                        ImputedPedCallsRegistered = 8,
                        UniquePedDetections = 9
                    }
                });

            _mockCycleRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, startDate, endDate))
                .Returns(new List<PhaseCycleAggregation>
                {
                    new PhaseCycleAggregation
                    {
                        Start = startDate,
                        End = startDate.AddMinutes(90),
                        PhaseNumber = 1,
                        PhaseBeginCount = 5
                    }
                });

            var query = new PedatLocationDataQuery
            {
                LocationIdentifiers = new List<string> { locationId },
                StartDate = startDate,
                EndDate = endDate,
                TimeUnit = PedestrianTimeUnit.Day
            };

            // Act
            var result = await _service.ExecuteAsync(query, null);

            // Assert
            Assert.IsNotNull(result);
            var data = result.First();
            Assert.AreEqual(locationId, data.LocationIdentifier);
            Assert.AreEqual("Main St & 1st Ave", data.Names);
            Assert.AreEqual("Downtown", data.Areas);
            Assert.IsTrue(data.TotalVolume > 0);
            Assert.IsTrue(data.RawData.Count > 0);
            Assert.IsNotNull(data.StatisticData);
        }

        [TestMethod]
        public void CalculateStatistics_ReturnsCorrectValues()
        {
            // Arrange
            var start = new DateTime(2025, 9, 1);
            var end = new DateTime(2025, 9, 1).AddHours(3);
            var rawData = new List<RawDataPoint>
            {
                new RawDataPoint { Timestamp = start, PedestrianCount = 10 },
                new RawDataPoint { Timestamp = start.AddHours(1), PedestrianCount = 20 },
                new RawDataPoint { Timestamp = start.AddHours(2), PedestrianCount = 30 },
                new RawDataPoint { Timestamp = start.AddHours(3), PedestrianCount = 40 }
            };

            // Act
            var stats = _service.CalculateStatistics(rawData, start, end, TimeSpan.FromHours(1));

            // Assert
            Assert.AreEqual(100, stats.Count);
            Assert.AreEqual(10, stats.Min);
            Assert.AreEqual(40, stats.Max);
            Assert.AreEqual(25, stats.Mean);
        }

        [TestMethod]
        public void EquationCalculation_Test()
        {
            var method = typeof(PedestrianAggregationService)
                .GetMethod("EquationCalculation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Test with recall true and activity > 350
            double result1 = (double)method.Invoke(_service, new object[] { true, 2, 400, 10, 15 });
            Assert.IsTrue(result1 > 0);

            // Test with recall false and cycleLength < 1.5
            double result2 = (double)method.Invoke(_service, new object[] { false, 1, 100, 10, 50 });
            Assert.IsTrue(result2 > 0);
        }

        // Optional helper to quickly mock repository data
        private void SetupMockData(string locationId, DateTime start, DateTime end)
        {
            _mockLocationRepo.Setup(x => x.GetLatestVersionOfLocation(locationId, start))
                .Returns(new Location
                {
                    LocationIdentifier = locationId,
                    PrimaryName = "Main St",
                    SecondaryName = "1st Ave",
                    Latitude = 40.0,
                    Longitude = -111.0,
                    Areas = new List<Area> { new Area { Name = "Downtown" } }
                });

            _mockPedRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhasePedAggregation>
                {
                    new PhasePedAggregation
                    {
                        Start = start,
                        End = start.AddHours(1),
                        PhaseNumber = 1,
                        PedBeginWalkCount = 10,
                        ImputedPedCallsRegistered = 8,
                        UniquePedDetections = 9
                    }
                });

            _mockCycleRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhaseCycleAggregation>
                {
                    new PhaseCycleAggregation
                    {
                        Start = start,
                        End = start.AddMinutes(90),
                        PhaseNumber = 1,
                        PhaseBeginCount = 5
                    }
                });
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task ExecuteAsync_EmptyLocationList_Throws()
        {
            var query = new PedatLocationDataQuery
            {
                LocationIdentifiers = new List<string> { "999" }, // non-existent
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now
            };

            _mockLocationRepo.Setup(x => x.GetLatestVersionOfLocation(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns((Location)null);

            await _service.ExecuteAsync(query, null);
        }

        [TestMethod]
        public async Task ExecuteAsync_NoPedestrianEvents_ReturnsZeroVolumes()
        {
            string locationId = "1";
            DateTime start = DateTime.Today;
            DateTime end = DateTime.Today.AddDays(1);

            _mockLocationRepo.Setup(x => x.GetLatestVersionOfLocation(locationId, start))
                .Returns(new Location
                {
                    LocationIdentifier = locationId,
                    PrimaryName = "A",
                    SecondaryName = "B"
                });

            _mockPedRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhasePedAggregation>());

            _mockCycleRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhaseCycleAggregation>());

            var query = new PedatLocationDataQuery
            {
                LocationIdentifiers = new List<string> { locationId },
                StartDate = start,
                EndDate = end
            };

            var result = await _service.ExecuteAsync(query, null);
            var data = result.First();

            Assert.AreEqual(0, data.TotalVolume);
            Assert.AreEqual(0, data.RawData.Sum(r => r.PedestrianCount ?? 0));
        }

        [TestMethod]
        public async Task ExecuteAsync_MultiplePhasesAndHours_AggregatesCorrectly()
        {
            string locationId = "2";
            DateTime start = new DateTime(2025, 9, 1, 6, 0, 0);
            DateTime end = start.AddHours(3);

            _mockLocationRepo.Setup(x => x.GetLatestVersionOfLocation(locationId, start))
                .Returns(new Location
                {
                    LocationIdentifier = locationId,
                    PrimaryName = "X",
                    SecondaryName = "Y"
                });

            _mockPedRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhasePedAggregation>
                {
                new PhasePedAggregation { Start = start, End = start.AddMinutes(30), PhaseNumber = 1, PedBeginWalkCount = 5, ImputedPedCallsRegistered = 4, UniquePedDetections = 5 },
                new PhasePedAggregation { Start = start.AddHours(1), End = start.AddHours(1).AddMinutes(30), PhaseNumber = 2, PedBeginWalkCount = 8, ImputedPedCallsRegistered = 8, UniquePedDetections = 7 }
                });

            _mockCycleRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhaseCycleAggregation>
                {
                new PhaseCycleAggregation { Start = start, End = start.AddMinutes(30), PhaseNumber = 1, PhaseBeginCount = 3 },
                new PhaseCycleAggregation { Start = start.AddHours(1), End = start.AddHours(1).AddMinutes(30), PhaseNumber = 2, PhaseBeginCount = 4 }
                });

            var query = new PedatLocationDataQuery
            {
                LocationIdentifiers = new List<string> { locationId },
                StartDate = start,
                EndDate = end,
                TimeUnit = PedestrianTimeUnit.Hour
            };

            var result = await _service.ExecuteAsync(query, null);
            var data = result.First();

            Assert.IsTrue(data.TotalVolume > 0);
            Assert.AreEqual(2, data.RawData.Count); // Two hourly groups
        }

        [TestMethod]
        public async Task ExecuteAsync_DifferentTimeUnits_RawDataSwapWorks()
        {
            string locationId = "3";
            DateTime start = DateTime.Today;
            DateTime end = DateTime.Today.AddDays(1);

            _mockLocationRepo.Setup(x => x.GetLatestVersionOfLocation(locationId, start))
                .Returns(new Location { LocationIdentifier = locationId });

            _mockPedRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhasePedAggregation>
                {
                new PhasePedAggregation { Start = start, End = end, PhaseNumber = 1, PedBeginWalkCount = 1, ImputedPedCallsRegistered = 1, UniquePedDetections = 1 }
                });

            _mockCycleRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhaseCycleAggregation>
                {
                new PhaseCycleAggregation { Start = start, End = end, PhaseNumber = 1, PhaseBeginCount = 1 }
                });

            foreach (PedestrianTimeUnit unit in Enum.GetValues(typeof(PedestrianTimeUnit)))
            {
                var query = new PedatLocationDataQuery
                {
                    LocationIdentifiers = new List<string> { locationId },
                    StartDate = start,
                    EndDate = end,
                    TimeUnit = unit
                };

                var result = await _service.ExecuteAsync(query, null);
                var data = result.First();
                Assert.IsTrue(data.RawData.Count > 0, $"Failed for TimeUnit={unit}");
            }
        }

        [TestMethod]
        public void CalculateStatistics_WithNullPedestrianCount_IgnoresNulls()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddHours(2);
            var rawData = new List<RawDataPoint>
        {
            new RawDataPoint { Timestamp = start, PedestrianCount = null },
            new RawDataPoint { Timestamp = start.AddHours(1), PedestrianCount = 10 },
            new RawDataPoint { Timestamp = start.AddHours(2), PedestrianCount = null }
        };

            var stats = _service.CalculateStatistics(rawData, start, end, TimeSpan.FromHours(1));
            Assert.AreEqual(10, stats.Count);
            Assert.AreEqual(10, stats.Min);
            Assert.AreEqual(10, stats.Max);
        }

        [TestMethod]
        public async Task ExecuteAsync_ReturnsExpectedValuesForControlledInput()
        {
            // Arrange
            string locationId = "LOC123";
            DateTime start = new DateTime(2025, 9, 1, 8, 0, 0);
            DateTime end = start.AddHours(1);

            _mockLocationRepo.Setup(x => x.GetLatestVersionOfLocation(locationId, start))
                .Returns(new Location
                {
                    LocationIdentifier = locationId,
                    PrimaryName = "Main St",
                    SecondaryName = "1st Ave",
                    Latitude = 40.123,
                    Longitude = -111.456,
                    Areas = new List<Area> { new Area { Name = "TestArea" } }
                });

            _mockPedRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhasePedAggregation>
                {
            new PhasePedAggregation
            {
                Start = start,
                End = end,
                PhaseNumber = 1,
                PedBeginWalkCount = 5,
                ImputedPedCallsRegistered = 5,
                UniquePedDetections = 3
            }
                });

            _mockCycleRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhaseCycleAggregation>
                {
            new PhaseCycleAggregation
            {
                Start = start,
                End = end,
                PhaseNumber = 1,
                PhaseBeginCount = 2
            }
                });

            var query = new PedatLocationDataQuery
            {
                LocationIdentifiers = new List<string> { locationId },
                StartDate = start,
                EndDate = end,
                TimeUnit = PedestrianTimeUnit.Hour
            };

            // Act
            var result = await _service.ExecuteAsync(query, null);
            var data = result.First();

            // Assert: location info
            Assert.AreEqual(locationId, data.LocationIdentifier);
            Assert.AreEqual("Main St & 1st Ave", data.Names);
            Assert.AreEqual("TestArea", data.Areas);
            Assert.AreEqual(40.123, data.Latitude);
            Assert.AreEqual(-111.456, data.Longitude);

            // Assert: volumes
            Assert.IsTrue(data.TotalVolume > 0, "TotalVolume should be greater than zero");
            Assert.IsTrue(data.AverageDailyVolume > 0, "AverageDailyVolume should be greater than zero");
            Assert.AreEqual(data.TotalVolume, data.RawData.Sum(r => r.PedestrianCount ?? 0), "TotalVolume should equal RawData sum");

            // Assert: averages
            Assert.IsTrue(data.AverageVolumeByHourOfDay.Count > 0, "Should have hourly averages");
            Assert.IsTrue(data.AverageVolumeByDayOfWeek.Count > 0, "Should have day-of-week averages");
            Assert.IsTrue(data.AverageVolumeByMonthOfYear.Count > 0, "Should have month averages");

            // Assert: raw data
            Assert.AreEqual(1, data.RawData.Count, "Only one hourly bucket expected");
            Assert.AreEqual(start, data.RawData.First().Timestamp);
            Assert.AreEqual(data.TotalVolume, data.RawData.First().PedestrianCount, "RawData pedestrian count should match total volume");

            // Assert: statistics
            var stats = data.StatisticData;
            Assert.IsTrue(stats.Count > 0, "StatisticData.Count should be positive");
            Assert.IsTrue(stats.Mean > 0, "StatisticData.Mean should be positive");
            Assert.IsTrue(stats.Min >= 0, "StatisticData.Min should not be negative");
            Assert.IsTrue(stats.Max >= stats.Min, "Max should be >= Min");
        }

        [TestMethod]
        public async Task ExecuteAsync_FiltersByPhaseNumber_ReturnsOnlySelectedPhase()
        {
            // Arrange
            string locationId = "LOC-PHASE";
            DateTime start = new DateTime(2025, 9, 1, 6, 0, 0);
            DateTime end = start.AddHours(2);

            _mockLocationRepo.Setup(x => x.GetLatestVersionOfLocation(locationId, start))
                .Returns(new Location
                {
                    LocationIdentifier = locationId,
                    PrimaryName = "PhaseTest",
                    SecondaryName = "Intersection",
                    Latitude = 40.1,
                    Longitude = -111.2
                });

            // Two pedestrian aggregations: one Phase 1, one Phase 2
            _mockPedRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhasePedAggregation>
                {
            new PhasePedAggregation
            {
                Start = start,
                End = start.AddMinutes(30),
                PhaseNumber = 1,
                PedBeginWalkCount = 5,
                ImputedPedCallsRegistered = 5,
                UniquePedDetections = 3
            },
            new PhasePedAggregation
            {
                Start = start.AddHours(1),
                End = start.AddHours(1).AddMinutes(30),
                PhaseNumber = 2,
                PedBeginWalkCount = 10,
                ImputedPedCallsRegistered = 10,
                UniquePedDetections = 8
            }
                });

            // Cycle data for both phases
            _mockCycleRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhaseCycleAggregation>
                {
            new PhaseCycleAggregation
            {
                Start = start,
                End = start.AddMinutes(30),
                PhaseNumber = 1,
                PhaseBeginCount = 2
            },
            new PhaseCycleAggregation
            {
                Start = start.AddHours(1),
                End = start.AddHours(1).AddMinutes(30),
                PhaseNumber = 2,
                PhaseBeginCount = 4
            }
                });

            var query = new PedatLocationDataQuery
            {
                LocationIdentifiers = new List<string> { locationId },
                StartDate = start,
                EndDate = end,
                TimeUnit = PedestrianTimeUnit.Hour,
                Phase = 1 // <-- Select only Phase 1
            };

            // Act
            var result = await _service.ExecuteAsync(query, null);
            var data = result.First();

            // Assert: Ensure only Phase 1 data is present
            Assert.IsNotNull(data);
            Assert.AreEqual(locationId, data.LocationIdentifier);
            Assert.IsTrue(data.TotalVolume > 0);

            // Check raw data only contains phase 1 results
            Assert.AreEqual(1, data.RawData.Count, "Only Phase 1 should be included in RawData");
            var raw = data.RawData.First();
            Assert.AreEqual(start, raw.Timestamp, "Phase 1 timestamp expected");

            // Make sure Phase 2 did not sneak in
            Assert.AreEqual(data.TotalVolume, raw.PedestrianCount, "TotalVolume should equal Phase 1 pedestrian count");
            Assert.IsTrue(data.TotalVolume < 15, "Phase 2 contribution must not be included");
        }

        [TestMethod]
        public async Task ExecuteAsync_SelectedPhaseNotInEvents_ReturnsEmptyData()
        {
            // Arrange
            string locationId = "LOC-MISSINGPHASE";
            DateTime start = new DateTime(2025, 9, 1, 6, 0, 0);
            DateTime end = start.AddHours(2);

            _mockLocationRepo.Setup(x => x.GetLatestVersionOfLocation(locationId, start))
                .Returns(new Location
                {
                    LocationIdentifier = locationId,
                    PrimaryName = "MissingPhase",
                    SecondaryName = "Intersection",
                    Latitude = 40.5,
                    Longitude = -111.5
                });

            // Repository only contains Phase 1 data
            _mockPedRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhasePedAggregation>
                {
            new PhasePedAggregation
            {
                Start = start,
                End = start.AddMinutes(30),
                PhaseNumber = 1,
                PedBeginWalkCount = 5,
                ImputedPedCallsRegistered = 5,
                UniquePedDetections = 3
            }
                });

            _mockCycleRepo.Setup(x => x.GetAggregationsBetweenDates(locationId, start, end))
                .Returns(new List<PhaseCycleAggregation>
                {
            new PhaseCycleAggregation
            {
                Start = start,
                End = start.AddMinutes(30),
                PhaseNumber = 1,
                PhaseBeginCount = 2
            }
                });

            var query = new PedatLocationDataQuery
            {
                LocationIdentifiers = new List<string> { locationId },
                StartDate = start,
                EndDate = end,
                TimeUnit = PedestrianTimeUnit.Hour,
                Phase = 99 // <-- Phase 99 does not exist
            };

            // Act
            var result = await _service.ExecuteAsync(query, null);
            var data = result.First();

            // Assert: No data should be included for Phase 99
            Assert.IsNotNull(data);
            Assert.AreEqual(locationId, data.LocationIdentifier);
            Assert.AreEqual(0, data.TotalVolume, "TotalVolume should be 0 because selected phase does not exist");
            Assert.AreEqual(0, data.RawData.Sum(r => r.PedestrianCount ?? 0), "RawData pedestrian count sum should be 0");
            Assert.IsTrue(data.RawData.All(r => r.PedestrianCount == 0 || r.PedestrianCount == null), "All RawData counts should be zero or null");

            // Statistics should handle empty input gracefully
            Assert.AreEqual(null, data.StatisticData.Count);
        }


    }
}
