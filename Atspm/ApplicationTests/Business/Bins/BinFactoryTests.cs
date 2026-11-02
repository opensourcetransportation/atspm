#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Business.Bins/BinFactoryTests.cs
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
using Utah.Udot.Atspm.Business.Bins;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Business.Bins
{
    public class BinFactoryTests
    {
        private static TimeOptions MorningPeakHourBins(DateTime start, DateTime end, List<DayOfWeek> days) => new()
        {
            Start = start,
            End = end,
            TimeOfDayStartHour = 6,
            TimeOfDayStartMinute = 0,
            TimeOfDayEndHour = 9,
            TimeOfDayEndMinute = 0,
            DaysOfWeek = days,
            TimeOption = TimeOptions.TimePeriodOptions.TimePeriod,
            SelectedBinSize = TimeOptions.BinSize.Hour
        };

        [Fact]
        public void GetBins_TimePeriodAcrossNewYear_BinsEveryDay()
        {
            var allDays = Enum.GetValues<DayOfWeek>().ToList();
            var options = MorningPeakHourBins(new DateTime(2025, 12, 30), new DateTime(2026, 1, 2), allDays);

            var bins = BinFactory.GetBins(options).Single().Bins;

            Assert.Equal(9, bins.Count);
            Assert.Equal(new DateTime(2025, 12, 30, 6, 0, 0), bins.First().Start);
            Assert.Equal(new DateTime(2026, 1, 1, 9, 0, 0), bins.Last().End);
        }

        [Fact]
        public void GetBins_TimePeriod_OnlySelectedDaysBeforeEndDate()
        {
            var weekdays = new List<DayOfWeek>
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
            };
            // Thursday October 1 up to, but not including, Thursday October 15.
            var options = MorningPeakHourBins(new DateTime(2026, 10, 1), new DateTime(2026, 10, 15), weekdays);

            var bins = BinFactory.GetBins(options).Single().Bins;

            Assert.Equal(30, bins.Count);
            Assert.All(bins, bin => Assert.Contains(bin.Start.DayOfWeek, weekdays));
            Assert.Equal(new DateTime(2026, 10, 14, 9, 0, 0), bins.Last().End);
        }

        [Fact]
        public void GetBins_TimePeriod_EndLaterInTheDayKeepsThatDay()
        {
            var allDays = Enum.GetValues<DayOfWeek>().ToList();
            var options = MorningPeakHourBins(new DateTime(2026, 10, 15), new DateTime(2026, 10, 15, 23, 59, 0), allDays);

            var bins = BinFactory.GetBins(options).Single().Bins;

            Assert.Equal(3, bins.Count);
            Assert.Equal(new DateTime(2026, 10, 15, 9, 0, 0), bins.Last().End);
        }

        [Fact]
        public void GetBins_TimePeriod_NoDaysChosenBinsEveryDay()
        {
            var options = MorningPeakHourBins(new DateTime(2026, 10, 1), new DateTime(2026, 10, 3), null);

            var bins = BinFactory.GetBins(options).Single().Bins;

            Assert.Equal(6, bins.Count);
        }

        [Fact]
        public void GetBins_TimePeriod_StartLaterInTheDayKeepsOnlyBinsFromStart()
        {
            var allDays = Enum.GetValues<DayOfWeek>().ToList();
            var options = MorningPeakHourBins(new DateTime(2026, 10, 1, 8, 0, 0), new DateTime(2026, 10, 2), allDays);

            var bin = BinFactory.GetBins(options).Single().Bins.Single();

            Assert.Equal(new DateTime(2026, 10, 1, 8, 0, 0), bin.Start);
        }

        [Fact]
        public void GetBins_TimePeriod_EndInsideTheWindowKeepsOnlyBinsBeforeEnd()
        {
            // A bin that would run past End is left out rather than cut short.
            var allDays = Enum.GetValues<DayOfWeek>().ToList();
            var options = MorningPeakHourBins(new DateTime(2026, 10, 15), new DateTime(2026, 10, 15, 7, 30, 0), allDays);

            var bin = BinFactory.GetBins(options).Single().Bins.Single();

            Assert.Equal(new DateTime(2026, 10, 15, 7, 0, 0), bin.End);
        }

        [Fact]
        public void GetBins_DayBins_StartAndEndInsideTheWindowCutTheFirstAndLastDays()
        {
            var allDays = Enum.GetValues<DayOfWeek>().ToList();
            var options = MorningPeakHourBins(new DateTime(2026, 10, 1, 8, 0, 0), new DateTime(2026, 10, 2, 7, 0, 0), allDays);
            options.SelectedBinSize = TimeOptions.BinSize.Day;

            var bins = BinFactory.GetBins(options).Single().Bins;

            Assert.Equal(new[] { new DateTime(2026, 10, 1, 8, 0, 0), new DateTime(2026, 10, 2, 6, 0, 0) },
                bins.Select(bin => bin.Start));
            Assert.Equal(new[] { new DateTime(2026, 10, 1, 9, 0, 0), new DateTime(2026, 10, 2, 7, 0, 0) },
                bins.Select(bin => bin.End));
        }

        [Fact]
        public void GetBins_DayBins_EndLaterInTheDayKeepsThatDay()
        {
            var allDays = Enum.GetValues<DayOfWeek>().ToList();
            var options = MorningPeakHourBins(new DateTime(2026, 10, 15), new DateTime(2026, 10, 15, 12, 0, 0), allDays);
            options.SelectedBinSize = TimeOptions.BinSize.Day;

            var bin = BinFactory.GetBins(options).Single().Bins.Single();

            Assert.Equal(new DateTime(2026, 10, 15, 6, 0, 0), bin.Start);
            Assert.Equal(new DateTime(2026, 10, 15, 9, 0, 0), bin.End);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(12, 2)]
        public void GetBins_MonthBins_IncludeEachMonthThatStartsBeforeEnd(int endHour, int expectedMonths)
        {
            var options = new TimeOptions
            {
                Start = new DateTime(2026, 10, 1),
                End = new DateTime(2026, 11, 1, endHour, 0, 0),
                TimeOption = TimeOptions.TimePeriodOptions.StartToEnd,
                SelectedBinSize = TimeOptions.BinSize.Month
            };

            var bins = BinFactory.GetBins(options).Single().Bins;

            Assert.Equal(expectedMonths, bins.Count);
            Assert.Equal(new DateTime(2026, 10, 1), bins.First().Start);
        }

        [Theory]
        [InlineData(2025, 1, 1, 2025, 12, 31, 1)]
        [InlineData(2024, 6, 1, 2025, 6, 1, 2)]
        [InlineData(2025, 1, 1, 2026, 1, 1, 1)]
        public void GetBins_YearBins_IncludeEachYearThatStartsBeforeEnd(int startYear, int startMonth, int startDay,
            int endYear, int endMonth, int endDay, int expectedYears)
        {
            var options = new TimeOptions
            {
                Start = new DateTime(startYear, startMonth, startDay),
                End = new DateTime(endYear, endMonth, endDay),
                TimeOption = TimeOptions.TimePeriodOptions.StartToEnd,
                SelectedBinSize = TimeOptions.BinSize.Year
            };

            var bins = BinFactory.GetBins(options).Single().Bins;

            Assert.Equal(expectedYears, bins.Count);
            Assert.Equal(new DateTime(startYear, 1, 1), bins.First().Start);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(12, 2)]
        public void GetBins_YearTimePeriod_IncludesEachYearThatStartsBeforeEnd(int endHour, int expectedYears)
        {
            var options = MorningPeakHourBins(new DateTime(2025, 6, 1), new DateTime(2026, 1, 1, endHour, 0, 0), null);
            options.SelectedBinSize = TimeOptions.BinSize.Year;

            Assert.Equal(expectedYears, BinFactory.GetBins(options).Count);
        }

        [Fact]
        public void GetBins_MonthTimePeriod_KeepsOnlyDaysBetweenStartAndEnd()
        {
            var options = MorningPeakHourBins(new DateTime(2026, 10, 15), new DateTime(2026, 11, 15), null);
            options.SelectedBinSize = TimeOptions.BinSize.Month;

            var containers = BinFactory.GetBins(options);

            Assert.Equal(new DateTime(2026, 10, 15, 6, 0, 0), containers[0].Bins.First().Start);
            Assert.Equal(17, containers[0].Bins.Count);
            Assert.Equal(new DateTime(2026, 11, 14, 9, 0, 0), containers[1].Bins.Last().End);
            Assert.Equal(14, containers[1].Bins.Count);
        }

        [Fact]
        public void GetBins_MonthTimePeriod_StartAndEndInsideTheWindowCutTheFirstAndLastDays()
        {
            var options = MorningPeakHourBins(new DateTime(2026, 10, 15, 8, 0, 0), new DateTime(2026, 11, 15, 7, 0, 0), null);
            options.SelectedBinSize = TimeOptions.BinSize.Month;

            var containers = BinFactory.GetBins(options);

            Assert.Equal(new DateTime(2026, 10, 15, 8, 0, 0), containers[0].Bins.First().Start);
            Assert.Equal(new DateTime(2026, 11, 15, 6, 0, 0), containers[1].Bins.Last().Start);
            Assert.Equal(new DateTime(2026, 11, 15, 7, 0, 0), containers[1].Bins.Last().End);
        }

        [Fact]
        public void GetBins_YearTimePeriod_KeepsOnlyDaysBetweenStartAndEnd()
        {
            var options = MorningPeakHourBins(new DateTime(2025, 6, 1), new DateTime(2026, 1, 1, 12, 0, 0), null);
            options.SelectedBinSize = TimeOptions.BinSize.Year;

            var containers = BinFactory.GetBins(options);

            Assert.Equal(new DateTime(2025, 6, 1, 6, 0, 0), containers[0].Bins.First().Start);
            Assert.Equal(new DateTime(2025, 12, 31, 9, 0, 0), containers[0].Bins.Last().End);
            Assert.Equal(new DateTime(2026, 1, 1, 6, 0, 0), containers[1].Bins.Single().Start);
        }

        [Fact]
        public void GetBins_TimePeriodEndingAtMidnight_RunsToEndOfDay()
        {
            var allDays = Enum.GetValues<DayOfWeek>().ToList();
            var options = MorningPeakHourBins(new DateTime(2026, 10, 1), new DateTime(2026, 10, 3), allDays);
            options.TimeOfDayStartHour = 22;
            options.TimeOfDayEndHour = 0;

            var bins = BinFactory.GetBins(options).Single().Bins;

            Assert.Equal(4, bins.Count);
            Assert.Equal(new DateTime(2026, 10, 3), bins.Last().End);
        }

        [Fact]
        public void GetBins_DayBinsEndingAtMidnight_RunToEndOfDay()
        {
            var allDays = Enum.GetValues<DayOfWeek>().ToList();
            var options = MorningPeakHourBins(new DateTime(2026, 10, 1), new DateTime(2026, 10, 2), allDays);
            options.TimeOfDayEndHour = 0;
            options.SelectedBinSize = TimeOptions.BinSize.Day;

            var bin = BinFactory.GetBins(options).Single().Bins.Single();

            Assert.Equal(new DateTime(2026, 10, 1, 6, 0, 0), bin.Start);
            Assert.Equal(new DateTime(2026, 10, 2), bin.End);
        }

        [Fact]
        public void GetBins_MonthTimePeriod_NoDaysChosenBinsEveryDay()
        {
            var options = MorningPeakHourBins(new DateTime(2026, 10, 1), new DateTime(2026, 11, 1), null);
            options.SelectedBinSize = TimeOptions.BinSize.Month;

            var bins = BinFactory.GetBins(options).Single().Bins;

            Assert.Equal(31, bins.Count);
        }

        [Fact]
        public void GetBins_NoSelectedDayInRange_AveragesToZero()
        {
            // Thursday October 1 to Saturday October 3, with only Mondays chosen.
            var options = MorningPeakHourBins(new DateTime(2026, 10, 1), new DateTime(2026, 10, 3),
                new List<DayOfWeek> { DayOfWeek.Monday });

            var container = BinFactory.GetBins(options).Single();

            Assert.Empty(container.Bins);
            Assert.Equal(0, container.AverageValue);
        }
    }
}
