#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows/ArrivalOnRedCalculationTests.cs
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

using Utah.Udot.Atspm.Enums;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows;

public class ArrivalOnRedCalculationTests
{
    [Theory]
    [InlineData(-1, ArrivalType.Unknown, 0)]
    [InlineData(0, ArrivalType.ArrivalOnRed, 30)]
    [InlineData(29, ArrivalType.ArrivalOnRed, 1)]
    [InlineData(30, ArrivalType.ArrivalOnGreen, 0)]
    [InlineData(49, ArrivalType.ArrivalOnGreen, 0)]
    [InlineData(50, ArrivalType.ArrivalOnYellow, 0)]
    [InlineData(59, ArrivalType.ArrivalOnYellow, 0)]
    public void ArrivalClassificationUsesSignalBoundaries(double seconds, ArrivalType expected, double delay)
    {
        var vehicle = new SignalMetricTestCycle().VehicleAt(seconds);
        Assert.Equal(expected, vehicle.ArrivalType);
        Assert.Equal(delay, vehicle.Delay);
    }
}
