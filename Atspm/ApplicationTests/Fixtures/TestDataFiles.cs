// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;

namespace Utah.Udot.Atspm.ApplicationTests.Fixtures;

internal static class TestDataFiles
{
    public static string DirectoryPath => Path.Combine(AppContext.BaseDirectory, "Analysis", "TestData");

    public static string ReadAllText(string fileName) => File.ReadAllText(Path.Combine(DirectoryPath, fileName));
}
