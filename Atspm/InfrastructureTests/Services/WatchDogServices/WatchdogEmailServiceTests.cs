#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests/WatchdogEmailServiceTests.cs
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

using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.NetStandardToolkit.Services;
using Xunit;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests
{
    public class WatchdogEmailServiceTests
    {
        private readonly Mock<ILogger<WatchdogEmailService>> _loggerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly WatchdogEmailService _watchdogEmailService;
        private readonly TimeProvider _weekdayClock = new FixedTimeProvider(new DateTimeOffset(2026, 4, 24, 12, 0, 0, TimeSpan.Zero));

        public WatchdogEmailServiceTests()
        {
            _loggerMock = new Mock<ILogger<WatchdogEmailService>>();
            _emailServiceMock = new Mock<IEmailService>();
            _watchdogEmailService = new WatchdogEmailService(_loggerMock.Object, _emailServiceMock.Object, _weekdayClock);
        }

        [Fact]
        public void GetEventsByIssueType_ShouldCategorizeEventsCorrectly()
        {
            // Arrange
            var eventsContainer = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Record count issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc2", DateTime.UtcNow, WatchDogComponentTypes.Location, 101, WatchDogIssueTypes.ForceOffThreshold, "Force off threshold issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc3", DateTime.UtcNow, WatchDogComponentTypes.Location, 102, WatchDogIssueTypes.MaxOutThreshold, "Max out threshold issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc4", DateTime.UtcNow, WatchDogComponentTypes.Location, 103, WatchDogIssueTypes.LowDetectorHits, "Low detector hits issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc5", DateTime.UtcNow, WatchDogComponentTypes.Location, 104, WatchDogIssueTypes.StuckPed, "Stuck ped issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc6", DateTime.UtcNow, WatchDogComponentTypes.Location, 105, WatchDogIssueTypes.UnconfiguredApproach, "Unconfigured approach issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc7", DateTime.UtcNow, WatchDogComponentTypes.Location, 106, WatchDogIssueTypes.UnconfiguredDetector, "Unconfigured detector issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc8", DateTime.UtcNow, WatchDogComponentTypes.Location, 107, WatchDogIssueTypes.LowRampDetectorHits, "Ramp Detectors Threshold issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc9", DateTime.UtcNow, WatchDogComponentTypes.Location, 108, WatchDogIssueTypes.RampMissedDetectorHits, "Ramp Missed Detectors issue","1", 1)
            };

            // Act
            WatchdogEmailService.GetEventsByIssueType(
                eventsContainer, out var missingErrorsLogs, out var forceErrorsLogs, out var maxErrorsLogs,
                out var countErrorsLogs, out var stuckPedErrorsLogs, out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs, out var rampDetectorThresholdErrorsLogs, out var rampMainlineErrorsLogs);

            // Assert
            Assert.Single(missingErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.RecordCount, missingErrorsLogs.First().IssueType);

            Assert.Single(forceErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.ForceOffThreshold, forceErrorsLogs.First().IssueType);

            Assert.Single(maxErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.MaxOutThreshold, maxErrorsLogs.First().IssueType);

            Assert.Single(countErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.LowDetectorHits, countErrorsLogs.First().IssueType);

            Assert.Single(stuckPedErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.StuckPed, stuckPedErrorsLogs.First().IssueType);

            Assert.Single(configurationErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.UnconfiguredApproach, configurationErrorsLogs.First().IssueType);

            Assert.Single(unconfiguredDetectorErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.UnconfiguredDetector, unconfiguredDetectorErrorsLogs.First().IssueType);

            Assert.Single(rampDetectorThresholdErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.LowRampDetectorHits, rampDetectorThresholdErrorsLogs.First().IssueType);

            Assert.Single(rampMainlineErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.RampMissedDetectorHits, rampMainlineErrorsLogs.First().IssueType);
        }

        [Fact]
        public void GetEventsByIssueType_ShouldHandleEmptyInput()
        {
            // Arrange
            var eventsContainer = new List<WatchDogLogEventWithCountAndDate>();

            // Act
            WatchdogEmailService.GetEventsByIssueType(
                eventsContainer,
                out var missingErrorsLogs, out var forceErrorsLogs, out var maxErrorsLogs,
                out var countErrorsLogs, out var stuckPedErrorsLogs, out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs, out var rampDetectorThresholdErrorsLogs, out var rampMainlineErrorsLogs);

            // Assert
            Assert.Empty(missingErrorsLogs);
            Assert.Empty(forceErrorsLogs);
            Assert.Empty(maxErrorsLogs);
            Assert.Empty(countErrorsLogs);
            Assert.Empty(stuckPedErrorsLogs);
            Assert.Empty(configurationErrorsLogs);
            Assert.Empty(unconfiguredDetectorErrorsLogs);
            Assert.Empty(rampDetectorThresholdErrorsLogs);
            Assert.Empty(rampMainlineErrorsLogs);
        }

        [Fact]
        public void GetEventsByIssueType_ShouldCategorizeMultipleEventsCorrectly()
        {
            // Arrange
            var eventsContainer = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Record count issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc2", DateTime.UtcNow, WatchDogComponentTypes.Location, 101, WatchDogIssueTypes.RecordCount, "Another record count issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc3", DateTime.UtcNow, WatchDogComponentTypes.Location, 102, WatchDogIssueTypes.ForceOffThreshold, "Force off threshold issue","1", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc4", DateTime.UtcNow, WatchDogComponentTypes.Location, 103, WatchDogIssueTypes.ForceOffThreshold, "Another force off threshold issue","1", 1)
            };

            // Act
            WatchdogEmailService.GetEventsByIssueType(
                eventsContainer,
                out var missingErrorsLogs, out var forceErrorsLogs, out var maxErrorsLogs,
                out var countErrorsLogs, out var stuckPedErrorsLogs, out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs, out var rampDetectorThresholdErrorsLogs, out var rampMainlineErrorsLogs);

            // Assert
            Assert.Equal(2, missingErrorsLogs.Count);
            Assert.All(missingErrorsLogs, e => Assert.Equal(WatchDogIssueTypes.RecordCount, e.IssueType));

            Assert.Equal(2, forceErrorsLogs.Count);
            Assert.All(forceErrorsLogs, e => Assert.Equal(WatchDogIssueTypes.ForceOffThreshold, e.IssueType));

            Assert.Empty(maxErrorsLogs);
            Assert.Empty(countErrorsLogs);
            Assert.Empty(stuckPedErrorsLogs);
            Assert.Empty(configurationErrorsLogs);
            Assert.Empty(unconfiguredDetectorErrorsLogs);
            Assert.Empty(rampDetectorThresholdErrorsLogs);
            Assert.Empty(rampMainlineErrorsLogs);
        }

        [Fact]
        public void GetEventsByIssueType_ShouldHandleNullContainer()
        {
            // Arrange
            List<WatchDogLogEventWithCountAndDate> eventsContainer = null;

            // Act
            WatchdogEmailService.GetEventsByIssueType(
                eventsContainer,
                out var missingErrorsLogs, out var forceErrorsLogs, out var maxErrorsLogs,
                out var countErrorsLogs, out var stuckPedErrorsLogs, out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs, out var rampDetectorThresholdErrorsLogs, out var rampMainlineErrorsLogs);

            // Assert
            Assert.Empty(missingErrorsLogs);
            Assert.Empty(forceErrorsLogs);
            Assert.Empty(maxErrorsLogs);
            Assert.Empty(countErrorsLogs);
            Assert.Empty(stuckPedErrorsLogs);
            Assert.Empty(configurationErrorsLogs);
            Assert.Empty(unconfiguredDetectorErrorsLogs);
            Assert.Empty(rampDetectorThresholdErrorsLogs);
            Assert.Empty(rampMainlineErrorsLogs);
        }

        [Fact]
        public void GetEventsByIssueType_ShouldIgnoreNullEntriesInContainer()
        {
            // Arrange
            var eventsContainer = new List<WatchDogLogEventWithCountAndDate>
    {
        null,
        new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Record count issue","1", 1),
        null
    };

            // Act
            WatchdogEmailService.GetEventsByIssueType(
                eventsContainer,
                out var missingErrorsLogs, out var forceErrorsLogs, out var maxErrorsLogs,
                out var countErrorsLogs, out var stuckPedErrorsLogs, out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs, out var rampDetectorThresholdErrorsLogs, out var rampMainlineErrorsLogs);

            // Assert
            Assert.Single(missingErrorsLogs);
            Assert.Empty(forceErrorsLogs);
            Assert.Empty(maxErrorsLogs);
            Assert.Empty(countErrorsLogs);
            Assert.Empty(stuckPedErrorsLogs);
            Assert.Empty(configurationErrorsLogs);
            Assert.Empty(unconfiguredDetectorErrorsLogs);
            Assert.Empty(rampDetectorThresholdErrorsLogs);
            Assert.Empty(rampMainlineErrorsLogs);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldReturnHeadersForMissingRecordsErrors()
        {
            // Arrange
            var sectionTitle = "Missing Records Errors";
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string> { "Location", "Location Description", "Issue Details", "Date of First Occurrence" };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldIncludeErrorCountsAndConsecutiveHeaders()
        {
            // Arrange
            var sectionTitle = "Missing Records Errors";
            var includeErrorCounts = true;
            var includeConsecutive = true;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string>
            {
                "Location", "Location Description", "Issue Details", "Error Count", "Consecutive Occurrence Count", "Date of First Occurrence"
            };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldReturnHeadersForForceOffErrors()
        {
            // Arrange
            var sectionTitle = "Force Off Errors";
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string> { "Location", "Location Description", "Phase", "Issue Details", "Date of First Occurrence" };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldReturnHeadersForLowDetectionCountErrors()
        {
            // Arrange
            var sectionTitle = "Low Detection Count Errors";
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string> { "Location", "Location Description", "Detector Config Id", "Issue Details", "Date of First Occurrence" };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldReturnHeadersForRampErrors()
        {
            // Arrange
            var sectionTitle = "Ramp Mainline Errors";
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string> { "Location", "Location Description", "Detector Config Id", "Issue Details", "Date of First Occurrence" };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldReturnDefaultHeadersForUnknownSection()
        {
            // Arrange
            var sectionTitle = "Unknown Errors";
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string> { "Location", "Location Description", "Issue Details", "Date of First Occurrence" };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldIncludeOnlyErrorCounts()
        {
            // Arrange
            var sectionTitle = "Missing Records Errors";
            var includeErrorCounts = true;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string>
            {
                "Location", "Location Description", "Issue Details", "Error Count", "Date of First Occurrence"
            };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldIncludeOnlyConsecutiveOccurrenceCount()
        {
            // Arrange
            var sectionTitle = "Missing Records Errors";
            var includeErrorCounts = false;
            var includeConsecutive = true;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string>
            {
                "Location", "Location Description", "Issue Details", "Consecutive Occurrence Count", "Date of First Occurrence"
            };
            Assert.Equal(expectedHeaders, headers);
        }

        // The following tests are for the BuildErrorSection method
        [Fact]
        public void BuildErrorSection_ShouldReturnTableWithHeadersAndRows_WhenErrorLogsArePresent()
        {
            // Arrange
            var sectionTitle = "Missing Records Errors";
            var sectionTimeDescription = "Errors detected in the last 24 hours.";
            var errorLogs = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Issue details 1","1", null)
                {
                    EventCount = 5,
                    ConsecutiveOccurenceCount = 3,
                    DateOfFirstInstance = DateTime.UtcNow.AddDays(-10)
                },
                new WatchDogLogEventWithCountAndDate(2, "Loc2", DateTime.UtcNow, WatchDogComponentTypes.Location, 101, WatchDogIssueTypes.RecordCount, "Issue details 2","1", null)
                {
                    EventCount = 10,
                    ConsecutiveOccurenceCount = 5,
                    DateOfFirstInstance = DateTime.UtcNow.AddDays(-15)
                }
            };
            var locations = new Dictionary<int, Location>
            {
                { 1, new Location { Id = 1, PrimaryName = "Main St", SecondaryName = "1st Ave", LocationIdentifier = "Loc1", Latitude = 40.1234, Longitude = -111.5678 } },
                { 2, new Location { Id = 2, PrimaryName = "Broadway", SecondaryName = "2nd Ave", LocationIdentifier = "Loc2", Latitude = 41.9876, Longitude = -112.3456 } }
            };
            var options = new WatchdogEmailOptions();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = true;
            var includeConsecutive = true;

            // Act
            var result = _watchdogEmailService.BuildErrorSection(
                sectionTitle,
                sectionTimeDescription,
                errorLogs,
                locations,
                options.EmailAllErrors,
                logsFromPreviousDay,
                includeErrorCounts,
                includeConsecutive);

            // Assert
            Assert.Contains("<h3>Missing Records Errors</h3>", result);
            Assert.Contains("<p>Errors detected in the last 24 hours.</p>", result);
            Assert.Contains("<table class='atspm-table'>", result);
            Assert.Contains("<th>Location</th>", result);
            Assert.Contains("<th>Error Count</th>", result);
            Assert.Contains("<th>Consecutive Occurrence Count</th>", result);
            Assert.Contains("<td>Loc1</td>", result);
            Assert.Contains("<td>Main St & 1st Ave</td>", result);
            Assert.Contains("<td>Loc2</td>", result);
            Assert.Contains("<td>Broadway & 2nd Ave</td>", result);
        }

        [Fact]
        public void BuildErrorSection_ShouldReturnNoErrorsMessage_WhenErrorLogsAreEmpty()
        {
            // Arrange
            var sectionTitle = "Force Off Errors";
            var sectionTimeDescription = "No errors detected in the last 24 hours.";
            var errorLogs = new List<WatchDogLogEventWithCountAndDate>();
            var locations = new Dictionary<int, Location>();
            var options = new WatchdogEmailOptions();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.BuildErrorSection(
                sectionTitle,
                sectionTimeDescription,
                errorLogs,
                locations,
                options.EmailAllErrors,
                logsFromPreviousDay,
                includeErrorCounts,
                includeConsecutive);

            // Assert
            Assert.Contains("<h3>Force Off Errors</h3>", result);
            Assert.Contains("<p>No errors found for this category.</p>", result);
            Assert.DoesNotContain("<table", result);
        }

        [Fact]
        public void BuildErrorSection_ShouldHandleNullErrorLogs()
        {
            // Arrange
            var sectionTitle = "Max Out Errors";
            var sectionTimeDescription = "Errors detected during the past week.";
            List<WatchDogLogEventWithCountAndDate> errorLogs = null;
            var locations = new Dictionary<int, Location>();
            var options = new WatchdogEmailOptions();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.BuildErrorSection(
                sectionTitle,
                sectionTimeDescription,
                errorLogs,
                locations,
                options.EmailAllErrors,
                logsFromPreviousDay,
                includeErrorCounts,
                includeConsecutive);

            // Assert
            Assert.Contains("<h3>Max Out Errors</h3>", result);
            Assert.Contains("<p>No errors found for this category.</p>", result);
            Assert.DoesNotContain("<table", result);
        }

        [Fact]
        public void BuildErrorSection_ShouldIncludeHeadersBasedOnFlags()
        {
            // Arrange
            var sectionTitle = "Low Detection Count Errors";
            var sectionTimeDescription = "Errors detected in the past hour.";
            var errorLogs = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Detector, 100, WatchDogIssueTypes.LowDetectorHits, "Details","1", null)
                {
                    EventCount = 5,
                    ConsecutiveOccurenceCount = 3,
                    DateOfFirstInstance = DateTime.UtcNow.AddDays(-10)
                }
            };
            var locations = new Dictionary<int, Location>
            {
                { 1, new Location { Id = 1, PrimaryName = "Main St", SecondaryName = "1st Ave", LocationIdentifier = "Loc1", Latitude = 40.1234, Longitude = -111.5678 } }
            };
            var options = new WatchdogEmailOptions();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = true;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.BuildErrorSection(
                sectionTitle,
                sectionTimeDescription,
                errorLogs,
                locations,
                options.EmailAllErrors,
                logsFromPreviousDay,
                includeErrorCounts,
                includeConsecutive);

            // Assert
            Assert.Contains("<th>Error Count</th>", result);
            Assert.DoesNotContain("<th>Consecutive Occurrence Count</th>", result);
        }

        // The following tests are for the ProcessErrorList method
        [Fact]
        public void ProcessErrorList_ShouldReturnNoErrorsMessage_WhenErrorListIsEmpty()
        {
            // Arrange
            var errorTitle = "Test Errors";
            var errorSubHeader = "No errors detected in this category.";
            var errors = new List<WatchDogLogEventWithCountAndDate>();
            var options = new WatchdogEmailOptions
            {
                PmScanDate = DateTime.UtcNow
            };
            var locations = new List<Location>();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.ProcessErrorList(errorTitle, errorSubHeader, errors, options, locations, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains($"<h2>{errorTitle}</h2>", result);
            Assert.Contains($"<h4>{errorSubHeader}</h4>", result);
            Assert.Contains("<p>No errors found for this category.</p>", result);
        }

        [Fact]
        public void ProcessErrorList_ShouldHandleNullErrorList()
        {
            // Arrange
            var errorTitle = "Test Errors";
            var errorSubHeader = "No errors detected in this category.";
            List<WatchDogLogEventWithCountAndDate> errors = null;
            var options = new WatchdogEmailOptions
            {
                PmScanDate = DateTime.UtcNow
            };
            var locations = new List<Location>();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.ProcessErrorList(errorTitle, errorSubHeader, errors, options, locations, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains($"<h2>{errorTitle}</h2>", result);
            Assert.Contains($"<h4>{errorSubHeader}</h4>", result);
            Assert.Contains("<p>No errors found for this category.</p>", result);
        }

        [Fact]
        public void ProcessErrorList_ShouldIncludeCSSStyles()
        {
            // Arrange
            var errorTitle = "Test Errors";
            var errorSubHeader = "Some errors detected.";
            var errors = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Issue details","1", null)
            };
            var options = new WatchdogEmailOptions
            {
                PmScanDate = DateTime.UtcNow
            };
            var locations = new List<Location>
            {
                new Location { Id = 1, PrimaryName = "Main St", SecondaryName = "1st Ave", LocationIdentifier = "Loc1", Latitude = 40.1234, Longitude = -111.5678 }
            };
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.ProcessErrorList(errorTitle, errorSubHeader, errors, options, locations, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains("<style>", result);
            Assert.Contains(".shaded-header", result);
            Assert.Contains(".atspm-table", result);
        }

        [Fact]
        public void ProcessErrorList_ShouldBuildSectionsForEachErrorType()
        {
            // Arrange
            var errorTitle = "Test Errors";
            var errorSubHeader = "Some errors detected.";
            var errors = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Issue details","1", null),
                new WatchDogLogEventWithCountAndDate(2, "Loc2", DateTime.UtcNow, WatchDogComponentTypes.Detector, 101, WatchDogIssueTypes.LowDetectorHits, "Detector issue details","1", null)
            };
            var options = new WatchdogEmailOptions
            {
                PmScanDate = DateTime.UtcNow,
                AmStartHour = 8,
                AmEndHour = 16,
                EmailPmErrors = true,
                EmailAmErrors = true
            };
            var locations = new List<Location>
            {
                new Location { Id = 1, PrimaryName = "Main St", SecondaryName = "1st Ave", LocationIdentifier = "Loc1", Latitude = 40.1234, Longitude = -111.5678 },
                new Location { Id = 2, PrimaryName = "Broadway", SecondaryName = "2nd Ave", LocationIdentifier = "Loc2", Latitude = 41.9876, Longitude = -112.3456 }
            };
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = true;
            var includeConsecutive = true;

            // Act
            var result = _watchdogEmailService.ProcessErrorList(errorTitle, errorSubHeader, errors, options, locations, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains("<h2 class='shaded-header'>Test Errors (Some errors detected.)</h2>", result);
            Assert.Contains("<h3>Missing Records Errors</h3>", result);
            Assert.Contains("<h3>Low Detection Count Errors</h3>", result);
            Assert.Contains("<td>Loc1</td>", result);
            Assert.Contains("<td>Main St & 1st Ave</td>", result);
            Assert.Contains("<td>Loc2</td>", result);
            Assert.Contains("<td>Broadway & 2nd Ave</td>", result);
        }

        // The following tests are for the GetMessage method
        [Fact]
        public void GetMessage_ShouldReturnEmptyString_WhenParametersAreNull()
        {
            // Arrange
            Dictionary<int, Location> locationDictionary = null;
            List<WatchDogLogEventWithCountAndDate> issues = null;
            WatchdogEmailOptions options = null;
            bool emailAllErrors = false;
            List<WatchDogLogEvent> logsFromPreviousDay = null;
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.GetMessage(locationDictionary, issues, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Equal(string.Empty, result);
            //_loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetMessage_ShouldReturnEmptyString_WhenIssuesListIsEmpty()
        {
            // Arrange
            var locationDictionary = GetMockLocations();
            var issues = new List<WatchDogLogEventWithCountAndDate>();
            var options = new WatchdogEmailOptions { EmailAllErrors = true };
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.GetMessage(locationDictionary, issues, options.EmailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetMessage_ShouldGenerateTableRows_ForValidIssues()
        {
            // Arrange
            var locationDictionary = GetMockLocations();
            var issues = GetMockWatchDogLogEventsWithCountAndDate();
            var options = new WatchdogEmailOptions { EmailAllErrors = true };
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = true;
            var includeConsecutive = true;

            // Act
            var result = _watchdogEmailService.GetMessage(locationDictionary, issues, options.EmailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains("<td>Loc1</td>", result);
            Assert.Contains("<td>Main St & 1st Ave</td>", result);
            Assert.Contains("<td>5</td>", result);
            Assert.Contains("<td>3</td>", result);
            Assert.Contains("<td>1/1/2024 12:00:00 AM</td>", result);
            //Assert.Contains("<td>2024-01-01</td>", result);
        }

        [Fact]
        public void GetMessage_ShouldExcludeIssues_FromLogsFromPreviousDay_WhenEmailAllErrorsIsFalse()
        {
            // Arrange
            var locationDictionary = GetMockLocations();
            var issues = GetMockWatchDogLogEventsWithCountAndDate();
            var options = new WatchdogEmailOptions { EmailAllErrors = false };
            var logsFromPreviousDay = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", DateTime.Parse("2026-01-20T21:07:41Z"), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Details","1", null)
            };
            var includeErrorCounts = true;
            var includeConsecutive = true;

            // Act
            var result = _watchdogEmailService.GetMessage(locationDictionary, issues, options.EmailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.DoesNotContain("<td>Loc1</td>", result);
            Assert.Contains("<td>Loc2</td>", result);
        }

        [Fact]
        public void GetMessage_ShouldIncludePhaseAndComponentId_WhenApplicable()
        {
            // Arrange
            var locationDictionary = GetMockLocations();
            var issues = GetMockWatchDogLogEventsWithPhaseAndComponent();
            var options = new WatchdogEmailOptions { EmailAllErrors = true };
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = true;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.GetMessage(locationDictionary, issues, options.EmailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains("<td>1</td>", result); // Phase
            Assert.Contains("<td>101</td>", result); // ComponentId
        }

        private Dictionary<int, Location> GetMockLocations()
        {
            return new Dictionary<int, Location>
            {
                { 1, new Location { Id = 1, PrimaryName = "Main St", SecondaryName = "1st Ave", LocationIdentifier = "Loc1" } },
                { 2, new Location { Id = 2, PrimaryName = "Broadway", SecondaryName = "2nd Ave", LocationIdentifier = "Loc2" } }
            };
        }

        private List<WatchDogLogEventWithCountAndDate> GetMockWatchDogLogEventsWithCountAndDate()
        {
            return new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.Parse("2026-01-20T21:07:41Z"), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Details","1", null)
                {
                    EventCount = 5,
                    ConsecutiveOccurenceCount = 3,
                    DateOfFirstInstance = new DateTime(2024, 1, 1)
                },
                new WatchDogLogEventWithCountAndDate(2, "Loc2", DateTime.UtcNow, WatchDogComponentTypes.Location, 101, WatchDogIssueTypes.RecordCount, "Details","1", null)
                {
                    EventCount = 10,
                    ConsecutiveOccurenceCount = 5,
                    DateOfFirstInstance = new DateTime(2024, 1, 2)
                }
            };
        }

        private List<WatchDogLogEventWithCountAndDate> GetMockWatchDogLogEventsWithPhaseAndComponent()
        {
            return new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Detector, 101, WatchDogIssueTypes.LowDetectorHits, "Details","1", 1)
                {
                    EventCount = 5,
                    ConsecutiveOccurenceCount = 3,
                    DateOfFirstInstance = new DateTime(2024, 1, 1)
                }
            };
        }

        [Fact]
        public async Task SendAllEmails_ShouldSendAdminEmail_ForUsersWithoutAssignments()
        {
            // Arrange
            var options = new WatchdogEmailOptions
            {
                EmailPmErrors = true,
                DefaultEmailAddress = "from@test.com",
                PmScanDate = DateTime.Today
            };

            var recipients = new List<WatchdogEmailRecipient>
            {
                new WatchdogEmailRecipient { UserId = "1", Email = "admin@test.com", IsAdmin = true, IsWatchdogSubscriber = true }
            };

            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(m => m.SendEmailAsync(It.IsAny<MailMessage>()))
                            .ReturnsAsync(true);

            var service = new WatchdogEmailService(_loggerMock.Object, emailServiceMock.Object, _weekdayClock);

            // Act
            await service.SendAllEmails(
                options,
                new(), new(), new(),
                new(), recipients,
                new(),
                new(),
                new(),
                new());

            // Assert
            emailServiceMock.Verify(m =>
                m.SendEmailAsync(It.Is<MailMessage>(msg =>
                    msg.From.Address == "from@test.com" &&
                    msg.Subject.Contains("All Locations ATSPM Alerts") &&
                    msg.To.Cast<MailAddress>().Any(to => to.Address == "admin@test.com")
                )),
                Times.Once);
        }

        [Fact]
        public async Task SendRegionEmails_ShouldNotSend_WhenPmAndAmDisabled()
        {
            // Arrange
            var options = new WatchdogEmailOptions
            {
                EmailPmErrors = false,
                EmailAmErrors = false,
                EmailRampErrors = false,
                DefaultEmailAddress = "from@test.com"
            };

            // Act
            await _watchdogEmailService.SendAllEmails(
                options,
                new(), new(), new(),      // newErrors, dailyRecurringErrors, recurringErrors
                new(),                     // Locations
                new(),                     // Recipients
                new(),                     // Jurisdictions
                new(),                     // Areas
                new(),                     // Regions
                new()                      // Logs from previous day
            );

            // Assert
            _emailServiceMock.Verify(m =>
                m.SendEmailAsync(It.IsAny<MailMessage>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SendAllEmails_ShouldSendDelivery_WhenWeekdayOnlyAndWeekend()
        {
            var options = new WatchdogEmailOptions
            {
                WeekdayOnly = true,
                EmailPmErrors = true,
                DefaultEmailAddress = "from@test.com",
                PmScanDate = new DateTime(2026, 4, 25)
            };

            var recipients = new List<WatchdogEmailRecipient>
            {
                new WatchdogEmailRecipient
                {
                    UserId = "1",
                    Email = "admin@test.com",
                    IsAdmin = true,
                    IsWatchdogSubscriber = true
                }
            };

            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(m => m.SendEmailAsync(It.IsAny<MailMessage>()))
                .ReturnsAsync(true);

            var service = new WatchdogEmailService(_loggerMock.Object, emailServiceMock.Object, _weekdayClock);

            await service.SendAllEmails(
                options,
                new(),
                new(),
                new(),
                new List<Location> { new Location { Id = 1, LocationIdentifier = "Loc1", RegionId = 1, JurisdictionId = 1, Areas = new List<Area>() } },
                recipients,
                new List<Jurisdiction> { new Jurisdiction { Id = 1, Name = "Jurisdiction 1" } },
                new List<Area> { new Area { Id = 1, Name = "Area 1" } },
                new List<Region> { new Region { Id = 1, Description = "Region 1" } },
                new());

            emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<MailMessage>()), Times.Once);
        }

        [Fact]
        public async Task SendAllEmails_ShouldSendDelivery_WhenWeekdayOnlyAndWeekday()
        {
            var options = new WatchdogEmailOptions
            {
                WeekdayOnly = true,
                EmailPmErrors = true,
                DefaultEmailAddress = "from@test.com",
                PmScanDate = new DateTime(2026, 4, 24)
            };

            var recipients = new List<WatchdogEmailRecipient>
            {
                new WatchdogEmailRecipient
                {
                    UserId = "1",
                    Email = "admin@test.com",
                    IsAdmin = true,
                    IsWatchdogSubscriber = true
                }
            };

            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(m => m.SendEmailAsync(It.IsAny<MailMessage>()))
                .ReturnsAsync(true);

            var service = new WatchdogEmailService(_loggerMock.Object, emailServiceMock.Object, _weekdayClock);

            await service.SendAllEmails(
                options,
                new(),
                new(),
                new(),
                new List<Location> { new Location { Id = 1, LocationIdentifier = "Loc1", RegionId = 1, JurisdictionId = 1, Areas = new List<Area>() } },
                recipients,
                new List<Jurisdiction> { new Jurisdiction { Id = 1, Name = "Jurisdiction 1" } },
                new List<Area> { new Area { Id = 1, Name = "Area 1" } },
                new List<Region> { new Region { Id = 1, Description = "Region 1" } },
                new());

            emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<MailMessage>()), Times.Once);
        }

        [Fact]
        public async Task SendAllEmails_ShouldSendRampEmail_WhenWeekdayOnlyAndWeekday()
        {
            var options = new WatchdogEmailOptions
            {
                WeekdayOnly = true,
                EmailRampErrors = true,
                DefaultEmailAddress = "from@test.com",
                RampMissedDetectorHitsStartScanDate = new DateTime(2026, 4, 24)
            };

            var recipient = new WatchdogEmailRecipient
            {
                UserId = "1",
                Email = "ramp@test.com",
                JurisdictionIds = new List<int> { 1 }
            };

            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(m => m.SendEmailAsync(It.IsAny<MailMessage>()))
                .ReturnsAsync(true);

            var service = new WatchdogEmailService(_loggerMock.Object, emailServiceMock.Object, _weekdayClock);

            await service.SendAllEmails(
                options,
                new(),
                new(),
                new(),
                new List<Location> { new Location { JurisdictionId = 1 } },
                new List<WatchdogEmailRecipient> { recipient },
                new List<Jurisdiction> { new Jurisdiction { Id = 1, Name = "I-15 Ramp" } },
                new(),
                new(),
                new());

            emailServiceMock.Verify(m =>
                m.SendEmailAsync(It.Is<MailMessage>(msg =>
                    msg.From.Address == "from@test.com" &&
                    msg.Subject.Contains("Ramp") &&
                    msg.To.Cast<MailAddress>().Any(to => to.Address == "ramp@test.com")
                )),
                Times.Once);
        }

        [Fact]
        public async Task SendAllEmails_ShouldSendRampDelivery_WhenWeekdayOnlyAndWeekend()
        {
            var service = new WatchdogEmailService(_loggerMock.Object, _emailServiceMock.Object, _weekdayClock);
            var options = new WatchdogEmailOptions
            {
                WeekdayOnly = true,
                EmailRampErrors = true,
                DefaultEmailAddress = "from@test.com",
                RampMissedDetectorHitsStartScanDate = new DateTime(2026, 4, 25)
            };

            var recipient = new WatchdogEmailRecipient
            {
                UserId = "1",
                Email = "ramp@test.com",
                JurisdictionIds = new List<int> { 1 }
            };

            await service.SendAllEmails(
                options,
                new(),
                new(),
                new(),
                new List<Location> { new Location { JurisdictionId = 1 } },
                new List<WatchdogEmailRecipient> { recipient },
                new List<Jurisdiction> { new Jurisdiction { Id = 1, Name = "I-15 Ramp" } },
                new(),
                new(),
                new());

            _emailServiceMock.Verify(m =>
                m.SendEmailAsync(It.Is<MailMessage>(msg =>
                    msg.Subject.Contains("Ramp") &&
                    msg.To.Cast<MailAddress>().Any(to => to.Address == "ramp@test.com")
                )), Times.Once);
        }


        [Fact]
        public async Task SendJurisdictionEmails_ShouldSendRampEmail_WhenJurisdictionContainsRamp()
        {
            // Arrange
            var options = new WatchdogEmailOptions
            {
                EmailRampErrors = true,
                RampMissedDetectorHitsStartScanDate = DateTime.Today,
                DefaultEmailAddress = "from@test.com"
            };

            var jurisdiction = new Jurisdiction { Id = 1, Name = "I-15 Ramp" };
            var recipient = new WatchdogEmailRecipient { UserId = "1", Email = "ramp@test.com", JurisdictionIds = new List<int> { 1 } };

            // Act
            await _watchdogEmailService.SendAllEmails(
                options,
                new(), new(), new(), // newErrors, dailyRecurringErrors, recurringErrors
                new List<Location> { new Location { JurisdictionId = 1 } },
                new List<WatchdogEmailRecipient> { recipient },
                new List<Jurisdiction> { jurisdiction },
                new(),
                new(),
                new()         // Logs from previous day
            );

            // Assert
            _emailServiceMock.Verify(m =>
                m.SendEmailAsync(It.Is<MailMessage>(msg =>
                    msg.From.Address == "from@test.com" &&
                    msg.Subject.Contains("Ramp") &&
                    msg.To.Cast<MailAddress>().Any(to => to.Address == "ramp@test.com")
                )),
                Times.Once
            );
        }


        [Fact]
        public async Task CreateEmailBody_ShouldIncludeAllThreeSections()
        {
            var options = new WatchdogEmailOptions
            {
                EmailPmErrors = true,
                PmScanDate = DateTime.Today
            };

            var body = await _watchdogEmailService.CreateEmailBody(
                options,
                new(), new(), new(),
                new(),
                new());

            Assert.Contains("New Errors", body);
            Assert.Contains("Daily Recurring Errors", body);
            Assert.Contains("Recurring Errors", body);
        }

        [Fact]
        public async Task CreateEmailBody_ShouldUseRampScanDate_WhenRampEmailTrue()
        {
            var options = new WatchdogEmailOptions
            {
                EmailRampErrors = true,
                RampMissedDetectorHitsStartScanDate = new DateTime(2024, 1, 15)
            };

            var body = await _watchdogEmailService.CreateEmailBody(
                options,
                new(), new(), new(),
                new(),
                new(),
                rampEmail: true);

            Assert.Contains("Ramp 1/15/2024", body);
        }

        [Fact]
        public void ProcessErrorList_ShouldOnlyRenderPmSections_WhenAmDisabled()
        {
            var options = new WatchdogEmailOptions
            {
                EmailPmErrors = true,
                EmailAmErrors = false,
                PmScanDate = DateTime.Today
            };

            var result = _watchdogEmailService.ProcessErrorList(
                "Errors",
                "Sub",
                new List<WatchDogLogEventWithCountAndDate>
                {
            new WatchDogLogEventWithCountAndDate(1, "Loc", DateTime.UtcNow,
                WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.RecordCount, "Details","1", null)
                },
                options,
                new List<Location> { new Location { Id = 1 } },
                new(),
                false,
                false);

            Assert.Contains("Missing Records Errors", result);
            Assert.DoesNotContain("Force Off Errors", result);
        }

        [Fact]
        public void ProcessErrorList_ShouldOnlyRenderRampMainline_WhenRampEmail()
        {
            var options = new WatchdogEmailOptions
            {
                EmailRampErrors = true,
                RampMissedDetectorHitsStartScanDate = DateTime.Today
            };

            var result = _watchdogEmailService.ProcessErrorList(
                "Errors",
                "Sub",
                new List<WatchDogLogEventWithCountAndDate>
                {
            new WatchDogLogEventWithCountAndDate(1, "Loc", DateTime.UtcNow,
                WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.RampMissedDetectorHits, "Details","1", null)
                },
                options,
                new List<Location> { new Location { Id = 1 } },
                new(),
                false,
                false,
                rampEmail: true);

            Assert.Contains("Ramp Mainline Errors", result);
            Assert.DoesNotContain("Missing Records Errors", result);
        }

        [Fact]
        public void GetMessage_ShouldSkipIssue_WhenLocationNotFound()
        {
            var result = _watchdogEmailService.GetMessage(
                new Dictionary<int, Location>(),
                new List<WatchDogLogEventWithCountAndDate>
                {
            new WatchDogLogEventWithCountAndDate(99, "LocX", DateTime.UtcNow,
                WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.RecordCount, "Details","1", null)
                },
                true,
                new(),
                false,
                false);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetMessage_ShouldNotIncludePhase_WhenPhaseIsZero()
        {
            var locations = GetMockLocations();
            var issues = new List<WatchDogLogEventWithCountAndDate>
    {
        new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow,
            WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.RecordCount, "Details","1", 0)
    };

            var result = _watchdogEmailService.GetMessage(
                locations,
                issues,
                true,
                new(),
                false,
                false);

            Assert.DoesNotContain("<td>0</td>", result);
        }

        [Fact]
        public async Task SendAdminEmail_ShouldIncludePmAndAmDatesInSubject()
        {
            // Arrange
            var options = new WatchdogEmailOptions
            {
                EmailPmErrors = true,
                EmailAmErrors = true,
                PmScanDate = new DateTime(2024, 1, 1),
                AmScanDate = new DateTime(2024, 1, 2),
                DefaultEmailAddress = "from@test.com"
            };

            var recipients = new List<WatchdogEmailRecipient>
    {
        new WatchdogEmailRecipient { UserId = "1", Email = "admin@test.com", IsAdmin = true, IsWatchdogSubscriber = true }
    };

            // Act
            await _watchdogEmailService.SendAllEmails(
                options,
                new(), new(), new(), // newErrors, dailyRecurringErrors, recurringErrors
                new(),                // Locations
                recipients,           // Recipients
                new(),                // Jurisdictions
                new(),                // Areas
                new(),                // Regions
                new()                 // Logs from previous day
            );

            // Assert
            _emailServiceMock.Verify(m =>
                m.SendEmailAsync(It.Is<MailMessage>(msg =>
                    msg.From.Address == "from@test.com" &&
                    msg.Subject.Contains("All Locations ATSPM Alerts") &&
                    msg.Subject.Contains("1/1/2024") &&  // PM date
                    msg.Subject.Contains("1/2/2024") &&  // AM date
                    msg.To.Cast<MailAddress>().Any(to => to.Address == "admin@test.com")
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task SendAdminEmail_ShouldSendTwoEmails_WhenPmAmAndRampEnabled()
        {
            // Arrange
            var options = new WatchdogEmailOptions
            {
                EmailPmErrors = true,
                EmailAmErrors = true,
                EmailRampErrors = true,
                PmScanDate = new DateTime(2024, 1, 1),
                AmScanDate = new DateTime(2024, 1, 2),
                RampMissedDetectorHitsStartScanDate = new DateTime(2024, 1, 3),
                DefaultEmailAddress = "from@test.com"
            };

            var recipients = new List<WatchdogEmailRecipient>
            {
                new WatchdogEmailRecipient { UserId = "1", Email = "admin@test.com", IsAdmin = true, IsWatchdogSubscriber = true }
            };

            // Act
            await _watchdogEmailService.SendAllEmails(
                options,
                new(), new(), new(), // newErrors, dailyRecurringErrors, recurringErrors
                new(),                // Locations
                recipients,           // Recipients
                new(),                // Jurisdictions
                new(),                // Areas
                new(),                // Regions
                new()                 // Logs from previous day
            );

            // Assert
            // Verify email for PM/AM errors
            _emailServiceMock.Verify(m =>
                m.SendEmailAsync(It.Is<MailMessage>(msg =>
                    msg.From.Address == "from@test.com" &&
                    msg.Subject.Contains("All Locations ATSPM Alerts") &&
                    msg.Subject.Contains("1/1/2024") &&  // PM date
                    msg.Subject.Contains("1/2/2024") &&  // AM date
                    msg.To.Cast<MailAddress>().Any(to => to.Address == "admin@test.com")
                )),
                Times.Once
            );

            // Verify separate email for Ramp errors
            _emailServiceMock.Verify(m =>
                m.SendEmailAsync(It.Is<MailMessage>(msg =>
                    msg.From.Address == "from@test.com" &&
                    msg.Subject.Contains("All Ramp ATSPM Alerts") &&
                    msg.Subject.Contains("1/3/2024") &&  // Ramp date
                    msg.To.Cast<MailAddress>().Any(to => to.Address == "admin@test.com")
                )),
                Times.Once
            );

            // Verify exactly 2 emails were sent total
            _emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<MailMessage>()), Times.Exactly(2));
        }

        [Fact]
        public void ProcessErrorList_ShouldIncludeOnlyRampErrors_WhenRampEmailOnly()
        {
            // Arrange
            var options = new WatchdogEmailOptions
            {
                EmailRampErrors = true,
                EmailPmErrors = false,
                EmailAmErrors = false,
                RampMissedDetectorHitsStartScanDate = DateTime.Today
            };

            var locations = new List<Location>
    {
        new Location { Id = 1, PrimaryName = "Loc1", SecondaryName = "Sec1" }
    };

            var rampError = new WatchDogLogEventWithCountAndDate(
                1, "Loc1", DateTime.Parse("2026-01-20T21:07:41Z"),
                WatchDogComponentTypes.Location, 100,
                WatchDogIssueTypes.RampMissedDetectorHits, "Ramp error details", "1", null)
            {
                EventCount = 5,
                ConsecutiveOccurenceCount = 3,
                DateOfFirstInstance = new DateTime(2024, 1, 1)
            };

            // Act
            var result = _watchdogEmailService.ProcessErrorList(
                "Ramp Errors",
                "Ramp error subheader",
                new List<WatchDogLogEventWithCountAndDate> { rampError },
                options,
                locations,
                new List<WatchDogLogEvent>(),
                includeErrorCounts: true,
                includeConsecutive: true,
                rampEmail: true);

            // Assert
            Assert.Contains("Ramp error details", result);
            Assert.DoesNotContain("Force Off", result); // PM/AM errors
            Assert.DoesNotContain("Unconfigured", result); // PM/AM errors
        }

        [Fact]
        public void ProcessErrorList_ShouldIncludeOnlyPmErrors_WhenPmEmailOnly()
        {
            // Arrange
            var options = new WatchdogEmailOptions
            {
                EmailRampErrors = false,
                EmailPmErrors = true,
                EmailAmErrors = false,
                PmScanDate = DateTime.Today
            };

            var locations = new List<Location>
    {
        new Location { Id = 1, PrimaryName = "Loc1", SecondaryName = "Sec1" }
    };

            var pmError = new WatchDogLogEventWithCountAndDate(
                1, "Loc1", DateTime.Parse("2026-01-20T21:07:41Z"),
                WatchDogComponentTypes.Location, 100,
                WatchDogIssueTypes.RecordCount, "PM error details", "1", null)
            {
                EventCount = 5,
                ConsecutiveOccurenceCount = 3,
                DateOfFirstInstance = new DateTime(2024, 1, 1)
            };

            // Act
            var result = _watchdogEmailService.ProcessErrorList(
                "PM Errors",
                "PM error subheader",
                new List<WatchDogLogEventWithCountAndDate> { pmError },
                options,
                locations,
                new List<WatchDogLogEvent>(),
                includeErrorCounts: true,
                includeConsecutive: true,
                rampEmail: false);

            // Assert
            Assert.Contains("PM error details", result);
            Assert.DoesNotContain("Mainline", result); // Ramp errors should not appear
        }

        [Fact]
        public void ProcessErrorList_ShouldIncludeOnlyAmErrors_WhenAmEmailOnly()
        {
            // Arrange
            var options = new WatchdogEmailOptions
            {
                EmailRampErrors = false,
                EmailPmErrors = false,
                EmailAmErrors = true,
                AmScanDate = DateTime.Today
            };

            var locations = new List<Location>
    {
        new Location { Id = 1, PrimaryName = "Loc1", SecondaryName = "Sec1" }
    };

            var amError = new WatchDogLogEventWithCountAndDate(
                1, "Loc1", DateTime.Parse("2026-01-20T21:07:41Z"),
                WatchDogComponentTypes.Location, 100,
                WatchDogIssueTypes.ForceOffThreshold, "AM error details", "1", null)
            {
                EventCount = 5,
                ConsecutiveOccurenceCount = 3,
                DateOfFirstInstance = new DateTime(2024, 1, 1)
            };

            // Act
            var result = _watchdogEmailService.ProcessErrorList(
                "AM Errors",
                "AM error subheader",
                new List<WatchDogLogEventWithCountAndDate> { amError },
                options,
                locations,
                new List<WatchDogLogEvent>(),
                includeErrorCounts: true,
                includeConsecutive: true,
                rampEmail: false);

            // Assert
            Assert.Contains("AM error details", result);
            Assert.DoesNotContain("Ramp", result);
            Assert.DoesNotContain("PM", result);
        }

        private sealed class FixedTimeProvider : TimeProvider
        {
            private readonly DateTimeOffset _utcNow;

            public FixedTimeProvider(DateTimeOffset utcNow)
            {
                _utcNow = utcNow;
            }

            public override DateTimeOffset GetUtcNow() => _utcNow;

            public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

            public override long GetTimestamp() => 0;

            public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
            {
                return new NoOpTimer();
            }

            private sealed class NoOpTimer : ITimer
            {
                public bool Change(TimeSpan dueTime, TimeSpan period) => true;

                public void Dispose()
                {
                }

                public ValueTask DisposeAsync()
                {
                    Dispose();
                    return ValueTask.CompletedTask;
                }
            }
        }

    }
}
