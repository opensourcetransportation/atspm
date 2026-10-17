#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows/PhaseTerminationAggregationTests.cs
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
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Common;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows;

public class PhaseTerminationAggregationTests
{
    [Fact]
    public async Task TerminationsRespectLocationPhaseAndBinBoundaries()
    {
        var start = new DateTime(2025, 12, 25, 8, 0, 0);
        var location = new Location { LocationIdentifier = "7115" };
        location.Approaches.Add(new Approach { Location = location, ProtectedPhaseNumber = 2 });
        IndianaEvent Event(short code, int minute, short phase = 2, string id = "7115") => new()
        {
            LocationIdentifier = id, EventCode = code, EventParam = phase, Timestamp = start.AddMinutes(minute)
        };
        // The aggregation contract retains the last event of a three-event termination run.
        var events = new[]
        {
            Event(4, 1), Event(4, 2), Event(4, 3),
            Event(5, 4), Event(5, 5), Event(5, 6),
            Event(6, 13), Event(6, 14), Event(6, 15),
            Event(4, 3, 6), Event(5, 7, id: "other"), Event(82, 8)
        };
        var step = new AggregatePhaseTerminationStep(new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15)));
        var results = (await step.ExecuteAsync(Tuple.Create(location, (IEnumerable<IndianaEvent>)events))).OrderBy(r => r.Start).ToList();
        Assert.Equal(2, results.Count);
        Assert.Equal(1, results[0].GapOuts);
        Assert.Equal(1, results[0].MaxOuts);
        Assert.Equal(0, results[0].ForceOffs);
        Assert.Equal(0, results[1].GapOuts);
        Assert.Equal(0, results[1].MaxOuts);
        Assert.Equal(1, results[1].ForceOffs);
        Assert.All(results, r => { Assert.Equal("7115", r.LocationIdentifier); Assert.Equal(2, r.PhaseNumber); });
    }
}
