#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows/PreemptionDetailsPipelineTests.cs
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
using Utah.Udot.Atspm.Analysis.PreemptionDetails;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows;

public class PreemptionDetailsPipelineTests
{
    [Fact]
    public async Task DwellEventsProduceAFilteredPreemptDetailResult()
    {
        var start = new DateTime(2025, 12, 25, 8, 0, 0);
        var location = new Location { LocationIdentifier = "7115" };
        IndianaEvent Event(short code, int seconds, short preempt = 1, string id = "7115") => new()
        {
            LocationIdentifier = id, EventCode = code, EventParam = preempt, Timestamp = start.AddSeconds(seconds)
        };
        var events = new[] { Event(107, 0), Event(111, 42), Event(107, 1, 2), Event(111, 4, 2), Event(107, 2, id: "other"), Event(111, 5, id: "other") };
        var dwell = await new CalculateDwellTime().ExecuteAsync(Tuple.Create(location, (IEnumerable<IndianaEvent>)events, 1));
        var result = Assert.Single(await new GeneratePreemptDetailResults().ExecuteAsync(dwell.Cast<PreempDetailValueBase>()));
        Assert.Equal("7115", result.LocationIdentifier);
        Assert.Equal(1, result.PreemptNumber);
        Assert.Equal(start, result.Start);
        Assert.Equal(start.AddSeconds(42), result.End);
        Assert.Equal(TimeSpan.FromSeconds(42), Assert.Single(result.DwellTimes).Seconds);
        Assert.Empty(result.Delay);
        Assert.Empty(result.TrackClearTimes);
    }
}
