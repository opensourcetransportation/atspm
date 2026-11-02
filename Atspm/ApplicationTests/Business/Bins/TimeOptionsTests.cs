#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Business.Bins/TimeOptionsTests.cs
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
using Utah.Udot.Atspm.Business.Bins;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Business.Bins
{
    public class TimeOptionsTests
    {
        // Thursday October 1 up to Thursday October 15, weekdays, 06:00 to 09:00.
        private static TimeOptions WeekdayMornings() => new()
        {
            Start = new DateTime(2026, 10, 1),
            End = new DateTime(2026, 10, 15),
            TimeOfDayStartHour = 6,
            TimeOfDayStartMinute = 0,
            TimeOfDayEndHour = 9,
            TimeOfDayEndMinute = 0,
            DaysOfWeek = new List<DayOfWeek>
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
            },
            TimeOption = TimeOptions.TimePeriodOptions.TimePeriod,
            SelectedBinSize = TimeOptions.BinSize.Hour
        };

        [Fact]
        public void GetValidationError_UsableOptions_ReturnsNull()
        {
            Assert.Null(WeekdayMornings().GetValidationError());
        }

        [Fact]
        public void GetValidationError_EndAtMidnight_IsEndOfDay()
        {
            var options = WeekdayMornings();
            options.TimeOfDayEndHour = 0;

            Assert.Null(options.GetValidationError());
        }

        [Fact]
        public void GetValidationError_EndNotAfterStart()
        {
            var options = WeekdayMornings();
            options.End = options.Start;

            Assert.Equal("End must be later than Start.", options.GetValidationError());
        }

        [Fact]
        public void GetValidationError_MissingTimeOfDay()
        {
            var options = WeekdayMornings();
            options.TimeOfDayEndMinute = null;

            Assert.Equal("A time period needs a start and end time of day.", options.GetValidationError());
        }

        [Fact]
        public void GetValidationError_TimeOfDayOutOfRange()
        {
            var options = WeekdayMornings();
            options.TimeOfDayEndHour = 24;

            Assert.Equal("Times of day must be between 00:00 and 23:59.", options.GetValidationError());
        }

        [Theory]
        [InlineData(9, 6)]
        [InlineData(6, 6)]
        public void GetValidationError_EndTimeOfDayNotAfterStart(int startHour, int endHour)
        {
            var options = WeekdayMornings();
            options.TimeOfDayStartHour = startHour;
            options.TimeOfDayEndHour = endHour;

            Assert.Equal("The end time of day must be later than the start time of day.", options.GetValidationError());
        }

        [Fact]
        public void GetValidationError_NoChosenDayInRange()
        {
            // Thursday October 1 to Saturday October 3, with only Mondays chosen.
            var options = WeekdayMornings();
            options.End = new DateTime(2026, 10, 3);
            options.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

            Assert.Equal("None of the chosen days fall in the date range.", options.GetValidationError());
        }

        [Fact]
        public void GetValidationError_TimesOfDayOutsideStartAndEnd()
        {
            // Thursday October 1 from 10:00 to noon, with a 06:00 to 09:00 time period.
            var options = WeekdayMornings();
            options.Start = new DateTime(2026, 10, 1, 10, 0, 0);
            options.End = new DateTime(2026, 10, 1, 12, 0, 0);

            Assert.Equal("The chosen times of day do not fall between Start and End.", options.GetValidationError());
        }

        [Fact]
        public void GetValidationError_NoChosenDayInRangeOverTheWholeRange()
        {
            // Thursday October 1 to Saturday October 3 by day, with only Mondays chosen.
            var options = WeekdayMornings();
            options.TimeOption = TimeOptions.TimePeriodOptions.StartToEnd;
            options.SelectedBinSize = TimeOptions.BinSize.Day;
            options.End = new DateTime(2026, 10, 3);
            options.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

            Assert.Equal("None of the chosen days fall in the date range.", options.GetValidationError());
        }

        [Fact]
        public void GetValidationError_StartToEndIgnoresTimeOfDay()
        {
            var options = WeekdayMornings();
            options.TimeOption = TimeOptions.TimePeriodOptions.StartToEnd;
            options.TimeOfDayStartHour = null;
            options.DaysOfWeek = null;
            options.SelectedBinSize = TimeOptions.BinSize.Month;

            Assert.Null(options.GetValidationError());
        }
    }
}
