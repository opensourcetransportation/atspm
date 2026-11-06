#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows/ApproachVolumePipelineTests.cs
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
using System.Linq;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.Workflows;

public class ApproachVolumePipelineTests
{
    [Fact]
    public async Task OpposingVolumesProduceCombinedHourlyReport()
    {
        var location = new Location { LocationIdentifier = "7115" };
        var primary = new Approach { Location = location, DirectionTypeId = DirectionTypes.NB, ProtectedPhaseNumber = 2 };
        var opposing = new Approach { Location = location, DirectionTypeId = DirectionTypes.SB, ProtectedPhaseNumber = 6 };
        var start = new DateTime(2025, 12, 25, 8, 0, 0);
        var p = CalculateTotalVolumesTests.Volumes(primary, start, 2, 2, 2, 2);
        var o = CalculateTotalVolumesTests.Volumes(opposing, start, 1, 1, 1, 1);
        var combined = await new CalculateTotalVolumes().ExecuteAsync(Tuple.Create(Tuple.Create(primary, p), Tuple.Create(opposing, o)));
        var result = await new GenerateApproachVolumeResults().ExecuteAsync(combined.Item2);
        Assert.Equal(start, result.Start);
        Assert.Equal(start.AddHours(1), result.End);
        Assert.Equal(8, result.PrimaryTotalVolume);
        Assert.Equal(4, result.OpposingTotalVolume);
        Assert.Equal(12, result.TotalVolume);
        Assert.Equal(12, result.TotalPeakVolume);
        Assert.Equal(1, result.TotalPHF);
    }
}
