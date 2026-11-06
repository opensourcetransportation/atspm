#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Attributes/AnalysisTestDataAttribute.cs
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Interfaces;
using Xunit.Sdk;

namespace Utah.Udot.Atspm.ApplicationTests.Attributes
{
    /// <summary>
    /// Custom generic xUnit data attribute to dynamically load test inputs and expected outputs
    /// from JSON data files matching the specific test type name.
    /// </summary>
    /// <typeparam name="T">The test data type parameter.</typeparam>
    public class AnalysisTestDataAttribute<T> : DataAttribute where T : AnalysisTestDataBase
    {
        /// <inheritdoc/>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var dir = new DirectoryInfo(TestDataFiles.DirectoryPath);
            var hasFiles = false;

            if (dir.Exists)
            {
                foreach (var f in dir.GetFiles("*.json").Where(f => f.Name.Contains(typeof(T).Name)))
                {
                    hasFiles = true;
                    var json = File.ReadAllText(f.FullName);
                    var fixture = JObject.Parse(json);
                    fixture.Remove("$type"); // The declared test type is known; old fixture assembly names are not.
                    var testFile = fixture.ToObject<T>(JsonSerializer.Create(new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All,
                    }));

                    if (testFile.Configuration is ILocationLayer config)
                    {
                        if (testFile.Input is IEnumerable<ILocationLayer> input)
                        {
                            foreach (var i in input)
                            {
                                i.LocationIdentifier = config.LocationIdentifier;
                            }
                        }

                        if (testFile.Output is IEnumerable<ILocationLayer> output)
                        {
                            foreach (var o in output)
                            {
                                o.LocationIdentifier = config.LocationIdentifier;
                            }
                        }
                    }

                    yield return new object[] { testFile.Configuration, testFile.Input, testFile.Output };
                }
            }

            if (!hasFiles)
            {
                yield return new object[] { default, default, default };
            }
        }
    }

    /// <summary>
    /// Non-generic custom xUnit data attribute that dynamically infers the test data type
    /// from the generic base class parameter of the declaring class.
    /// </summary>
    public class AnalysisTestDataAttribute : DataAttribute
    {
        /// <inheritdoc/>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var declaringType = testMethod.DeclaringType;
            Type testDataType = null;

            while (declaringType != null)
            {
                if (declaringType.IsGenericType)
                {
                    var genericTypeDefinition = declaringType.GetGenericTypeDefinition();
                    var genericArgs = declaringType.GetGenericArguments();

                    if (genericTypeDefinition.Name.StartsWith("WorkflowStepTestBase") && genericArgs.Length == 5)
                    {
                        testDataType = genericArgs[1];
                        break;
                    }
                }
                declaringType = declaringType.BaseType;
            }

            if (testDataType == null)
            {
                yield break;
            }

            var dir = new DirectoryInfo(TestDataFiles.DirectoryPath);
            var hasFiles = false;

            if (dir.Exists)
            {
                foreach (var f in dir.GetFiles("*.json").Where(f => f.Name.Contains(testDataType.Name)))
                {
                    hasFiles = true;
                    var json = File.ReadAllText(f.FullName);
                    var fixture = JObject.Parse(json);
                    fixture.Remove("$type");
                    var testFile = (AnalysisTestDataBase)fixture.ToObject(testDataType, JsonSerializer.Create(new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All,
                    }));

                    if (testFile.Configuration is ILocationLayer config)
                    {
                        if (testFile.Input is IEnumerable<ILocationLayer> input)
                        {
                            foreach (var i in input)
                            {
                                i.LocationIdentifier = config.LocationIdentifier;
                            }
                        }

                        if (testFile.Output is IEnumerable<ILocationLayer> output)
                        {
                            foreach (var o in output)
                            {
                                o.LocationIdentifier = config.LocationIdentifier;
                            }
                        }
                    }

                    yield return new object[] { testFile.Configuration, testFile.Input, testFile.Output };
                }
            }

            if (!hasFiles)
            {
                yield return new object[] { default, default, default };
            }
        }
    }
}
