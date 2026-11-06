#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Fixtures/TestSignalFixture.cs
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
using System.IO;
using Utah.Udot.Atspm.Data.Models;

namespace Utah.Udot.Atspm.ApplicationTests.Fixtures
{
    public class TestLocationFixture : IDisposable
    {
        public TestLocationFixture()
        {
            var json = TestDataFiles.ReadAllText("Location7115TestData.json");
            TestLocation = JsonConvert.DeserializeObject<Location>(json);
        }

        public Location TestLocation { get; set; }

        public void Dispose()
        {
        }
    }
}
