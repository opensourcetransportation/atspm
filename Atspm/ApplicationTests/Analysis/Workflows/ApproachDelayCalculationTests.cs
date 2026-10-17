#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows/ApproachDelayCalculationTests.cs
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

using Utah.Udot.Atspm.Analysis.ApproachDelay;
using Utah.Udot.Atspm.Analysis.Plans;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows;

public class ApproachDelayCalculationTests
{
    [Fact]
    public void RedArrivalDelayPropagatesToApproachPlanAndResult()
    {
        var cycle = new SignalMetricTestCycle();
        var red = cycle.VehicleAt(10);
        var green = cycle.VehicleAt(40);
        var plan = new ApproachDelayPlan { Vehicles = new[] { red, green } };
        var result = new ApproachDelayResult { Plans = new[] { plan } };
        Assert.Equal(20, red.Delay);
        Assert.Equal(0, green.Delay);
        Assert.Equal(10, result.AverageDelay);
        Assert.Equal(20.0 / 3600, result.TotalDelay);
        Assert.Equal(2, result.Vehicles.Count);
    }
}
