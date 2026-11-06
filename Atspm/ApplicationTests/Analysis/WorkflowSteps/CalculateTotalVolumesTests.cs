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

using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.Common;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps;

public class CalculateTotalVolumesTests
{
    private static readonly DateTime Start = new(2023, 4, 17, 8, 0, 0);

    private static Approach Approach(DirectionTypes direction, string location = "7115") => new()
    {
        Location = new Location { LocationIdentifier = location },
        DirectionTypeId = direction,
        ProtectedPhaseNumber = direction == DirectionTypes.NB ? 2 : 6
    };

    internal static Volumes Volumes(Approach approach, DateTime start, params int[] counts)
    {
        var volumes = new Volumes(start, start.AddMinutes(15 * counts.Length), TimeSpan.FromMinutes(15))
        {
            LocationIdentifier = approach.Location.LocationIdentifier,
            PhaseNumber = approach.ProtectedPhaseNumber,
            Direction = approach.DirectionTypeId
        };
        foreach (var item in volumes.Segments.Select((segment, index) => (segment, index)))
        {
            item.segment.LocationIdentifier = volumes.LocationIdentifier;
            item.segment.PhaseNumber = volumes.PhaseNumber;
            item.segment.Direction = volumes.Direction;
            item.segment.DetectorEvents.AddRange(Enumerable.Range(0, counts[item.index]).Select(_ => new CorrectedDetectorEvent
            {
                LocationIdentifier = volumes.LocationIdentifier,
                PhaseNumber = volumes.PhaseNumber,
                Direction = volumes.Direction,
                Timestamp = item.segment.Start.AddMinutes(1),
                DetectorChannel = 1
            }));
        }
        return volumes;
    }

    private static Task<Tuple<Approach, TotalVolumes>> Calculate(Approach primary, Volumes p, Approach opposing, Volumes o) =>
        new CalculateTotalVolumes().ExecuteAsync(Tuple.Create(Tuple.Create(primary, p), Tuple.Create(opposing, o)));

    [Fact]
    public async Task CalculateTotalVolumesLocationFilterTest()
    {
        var primary = Approach(DirectionTypes.NB);
        var opposing = Approach(DirectionTypes.SB, "other");
        var result = await Calculate(primary, Volumes(primary, Start, 1), opposing, Volumes(opposing, Start, 2));
        Assert.Null(result.Item1);
        Assert.Null(result.Item2);
    }

    [Fact]
    public async Task CalculateTotalVolumesRejectsNonOpposingDirections()
    {
        var primary = Approach(DirectionTypes.NB);
        var other = Approach(DirectionTypes.EB);
        var result = await Calculate(primary, Volumes(primary, Start, 1), other, Volumes(other, Start, 2));
        Assert.Null(result.Item1);
        Assert.Null(result.Item2);
    }

    [Fact]
    public async Task CalculateTotalVolumesDataCheckTest()
    {
        var primary = Approach(DirectionTypes.NB);
        var opposing = Approach(DirectionTypes.SB);
        var p = Volumes(primary, Start, 3);
        var o = Volumes(opposing, Start, 2);
        var result = await Calculate(primary, p, opposing, o);
        Assert.Same(primary, result.Item1);
        Assert.Equal("7115", result.Item2.LocationIdentifier);
        Assert.Equal(5, result.Item2.DetectorCount);
        var segment = Assert.Single(result.Item2.Segments);
        Assert.Same(p.Segments.Single(), segment.Primary);
        Assert.Same(o.Segments.Single(), segment.Opposing);
    }

    [Fact]
    public async Task CalculatePhaseTimeSegmentCheckTest()
    {
        var primary = Approach(DirectionTypes.NB);
        var opposing = Approach(DirectionTypes.SB);
        var result = await Calculate(primary, Volumes(primary, Start, 2, 1, 0, 3), opposing, Volumes(opposing, Start, 1, 1, 0, 2));
        Assert.Equal(new[] { 3, 2, 0, 5 }, result.Item2.Segments.Select(s => s.DetectorCount));
        Assert.All(result.Item2.Segments, s => Assert.Equal(TimeSpan.FromMinutes(15), s.End - s.Start));
        Assert.Equal(10, result.Item2.DetectorCount);
    }

    [Fact]
    public async Task CalculatePhaseStartEndCheckTest()
    {
        var primary = Approach(DirectionTypes.NB);
        var opposing = Approach(DirectionTypes.SB);
        var result = await Calculate(primary, Volumes(primary, Start, 2, 2), opposing, Volumes(opposing, Start.AddMinutes(15), 3, 3));
        Assert.Equal(Start, result.Item2.Start);
        Assert.Equal(Start.AddMinutes(45), result.Item2.End);
        Assert.Equal(new[] { 2, 5, 3 }, result.Item2.Segments.Select(s => s.DetectorCount));
    }

    [Fact]
    public async Task CalculateTotalVolumesNullInputTest()
    {
        var result = await new CalculateTotalVolumes().ExecuteAsync(Tuple.Create<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>>(null, null));
        Assert.Null(result.Item1);
        Assert.Null(result.Item2);
    }

    [Fact]
    public async Task MissingPhaseVolumesPreserveTheApproachWithoutAResult()
    {
        var primary = Approach(DirectionTypes.NB);
        var opposing = Approach(DirectionTypes.SB);
        var result = await Calculate(primary, null, opposing, Volumes(opposing, Start, 1));
        Assert.Same(primary, result.Item1);
        Assert.Null(result.Item2);
    }

    [Fact]
    public async Task CalculateTotalVolumesNoDataTest()
    {
        var primary = Approach(DirectionTypes.NB);
        var opposing = Approach(DirectionTypes.SB);
        var result = await Calculate(primary, Volumes(primary, Start, 0, 0), opposing, Volumes(opposing, Start, 0, 0));
        Assert.Equal(2, result.Item2.Segments.Count);
        Assert.Equal(0, result.Item2.DetectorCount);
        Assert.All(result.Item2.Segments, s => Assert.Equal(0, s.DetectorCount));
    }

    [Theory]
    [InlineData("CalculateTotalVolumesTestData1.json")]
    public async Task CalculateTotalVolumesFromFileTest(string file)
    {
        var fixture = JsonConvert.DeserializeObject<CalculateTotalVolumeTestData>(TestDataFiles.ReadAllText(file));
        var result = await Calculate(fixture.Configuration[0], fixture.Input[0], fixture.Configuration[1], fixture.Input[1]);
        Assert.Equivalent(fixture.Output, result.Item2);
    }
}
