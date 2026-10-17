#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.ATSPM.ApplicationTests.Analysis.WorkflowFilterTests/FilterPhaseIntervalChangeDateProcessStepTests.cs
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

using Utah.Udot.Atspm.Analysis.WorkflowFilters;
using Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowFilterTests;
using Utah.Udot.Atspm.Data.Enums;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.ApplicationTests.Analysis.WorkflowFilterTests
{
    public class FilterPhaseIntervalChangeDateProcessStepTests : WorkflowFilterTestsBase
    {
        public FilterPhaseIntervalChangeDateProcessStepTests(ITestOutputHelper output) : base(output)
        {
            filteredCodes =
            [
                (short)IndianaEnumerations.PhaseOn,
                (short)IndianaEnumerations.PhaseBeginGreen,
                (short)IndianaEnumerations.PhaseBeginYellowChange,
                (short)IndianaEnumerations.PhaseEndYellowChange,
                (short)IndianaEnumerations.PhaseEndRedClearance,
            ];

            sut = new FilterPhaseIntervalChangeDateProcessStep();
        }
    }
}
