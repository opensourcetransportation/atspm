#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.Atspm.InfrastructureTests.DatabaseInstallerTests/TransferConfigCommandTests.cs
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Xunit;

namespace Utah.Udot.Atspm.InfrastructureTests.DatabaseInstallerTests;

public class TransferConfigCommandTests
{
    [Fact]
    public void RootCommand_IncludesTransferConfigCommand()
    {
        var assembly = LoadDatabaseInstallerAssembly();
        var rootCommand = Activator.CreateInstance(
            assembly.GetType("DatabaseInstaller.Commands.DatabaseInstallerCommands", throwOnError: true)!)!;

        var children = GetEnumerableProperty(rootCommand, "Children")
            .Select(child => (string)GetPropertyValue(child, "Name")!)
            .ToArray();

        Assert.Contains("transfer-config", children);
    }

    [Fact]
    public void TransferConfigCommand_ExposesExpectedOptions()
    {
        var assembly = LoadDatabaseInstallerAssembly();
        var command = Activator.CreateInstance(
            assembly.GetType("DatabaseInstaller.Commands.TransferConfigCommand", throwOnError: true)!)!;

        var optionNames = GetEnumerableProperty(command, "Options")
            .Select(option => GetEnumerableProperty(option, "Aliases").Cast<string>().First())
            .ToArray();

        Assert.Equal(
            new[]
            {
                "--api-base-url",
                "--api-key",
                "--delete",
                "--update-locations",
                "--update-speed"
            },
            optionNames);
    }

    [Fact]
    public void BindCommandOptions_RegistersBinderAndHostedService()
    {
        var assembly = LoadDatabaseInstallerAssembly();
        var commandType = assembly.GetType("DatabaseInstaller.Commands.TransferConfigCommand", throwOnError: true)!;
        var command = Activator.CreateInstance(commandType)!;
        var services = new ServiceCollection();
        var bindMethod = commandType.GetMethod("BindCommandOptions", BindingFlags.Instance | BindingFlags.Public)!;

        bindMethod.Invoke(command, new object[] { new HostBuilderContext(new Dictionary<object, object>()), services });

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType.FullName?.Contains("System.CommandLine.NamingConventionBinder.ModelBinder") == true);

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IHostedService) &&
                          descriptor.ImplementationType?.Name == "TransferConfigCommandHostedService");
    }

    [Fact]
    public void JsonOptions_AcceptsStringEnumValues()
    {
        var assembly = LoadDatabaseInstallerAssembly();
        var hostedServiceType = assembly.GetType("TransferConfigCommandHostedService", throwOnError: true)!;
        var jsonOptionsField = hostedServiceType.GetField("JsonOptions", BindingFlags.NonPublic | BindingFlags.Static)!;
        var jsonOptions = Assert.IsType<JsonSerializerOptions>(jsonOptionsField.GetValue(null));

        var json = """
        {
          "value": [
            {
              "id": 1,
              "description": "Test config",
              "protocol": "Ftp",
              "port": 1234
            }
          ]
        }
        """;

        var responseType = hostedServiceType.GetNestedType("ODataResponse`1", BindingFlags.NonPublic)!;
        var closedResponseType = responseType.MakeGenericType(typeof(DeviceConfiguration));

        var response = JsonSerializer.Deserialize(json, closedResponseType, jsonOptions);
        Assert.NotNull(response);

        var valueProperty = closedResponseType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)!;
        var value = Assert.IsAssignableFrom<IEnumerable<DeviceConfiguration>>(valueProperty.GetValue(response));
        var configuration = Assert.Single(value);

        Assert.Equal(TransportProtocols.Ftp, configuration.Protocol);
    }

    [Fact]
    public void BuildLocationBatchUrl_UsesExpectedODataSyntax()
    {
        var assembly = LoadDatabaseInstallerAssembly();
        var hostedServiceType = assembly.GetType("TransferConfigCommandHostedService", throwOnError: true)!;
        var method = hostedServiceType.GetMethod("BuildLocationBatchUrl", BindingFlags.NonPublic | BindingFlags.Static)!;

        var locationUrl = (string)method.Invoke(null, new object[] { new[] { "1001", "1002" } })!;

        Assert.Equal(
            "api/v1/Location?$filter=LocationIdentifier eq '1001' or LocationIdentifier eq '1002'&$orderby=LocationIdentifier asc,Start desc&$expand=Areas,Approaches($expand=Detectors($expand=DetectionTypes),DirectionType),Devices($expand=DeviceConfiguration($expand=Product))",
            locationUrl);
    }

    private static Assembly LoadDatabaseInstallerAssembly()
    {
        return typeof(DatabaseInstaller.Commands.TransferConfigCommand).Assembly;
    }

    private static IEnumerable<object> GetEnumerableProperty(object instance, string propertyName)
    {
        var value = GetPropertyValue(instance, propertyName);
        return Assert.IsAssignableFrom<IEnumerable<object>>(value);
    }

    private static object GetPropertyValue(object instance, string propertyName) =>
        instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!
            .GetValue(instance);
}
