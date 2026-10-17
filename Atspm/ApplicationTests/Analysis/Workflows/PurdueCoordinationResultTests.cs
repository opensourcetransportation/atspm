#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows/PurdueCoordinationResultTests.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.Common;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows;

public class PurdueCoordinationResultTests
{
    [Fact]
    public async Task ResultsKeepEachLocationAndPhaseSeparate()
    {
        var cycles = new[] { new SignalMetricTestCycle(), new SignalMetricTestCycle(phase: 6), new SignalMetricTestCycle("other") };
        var arrivals = cycles.Select(c => new CycleArrivals(c) { Vehicles = new[] { c.VehicleAt(10), c.VehicleAt(40) } }).ToList();
        var results = (await new GeneratePurdueCoordinationResult().ExecuteAsync((IReadOnlyList<CycleArrivals>)arrivals)).ToList();
        Assert.Equal(3, results.Count);
        var result = results.Single(r => r.LocationIdentifier == "7115" && r.PhaseNumber == 2);
        Assert.Equal(cycles[0].Start, result.Start);
        Assert.Equal(cycles[0].End, result.End);
        Assert.Single(result.Plans);
        Assert.Single(result.ArrivalCycles);
        Assert.Equal(2, result.TotalVolume);
        Assert.Equal(1, result.TotalArrivalOnRed);
        Assert.Equal(1, result.TotalArrivalOnGreen);
        Assert.Equal(50, result.PercentArrivalOnRed);
    }
}
