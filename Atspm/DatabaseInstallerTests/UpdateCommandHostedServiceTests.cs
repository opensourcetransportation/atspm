// SPDX-License-Identifier: Apache-2.0

using DatabaseInstaller.Commands;
using DatabaseInstaller.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DatabaseInstallerTests;

public class UpdateCommandHostedServiceTests
{
    [Fact]
    public async Task StartAsync_ReportsMigrationFailureToTheHost()
    {
        var scope = new Mock<IServiceScope>();
        scope.SetupGet(s => s.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        using var services = new ServiceCollection()
            .AddSingleton(scopeFactory.Object)
            .BuildServiceProvider();
        var lifetime = new Mock<IHostApplicationLifetime>();
        var installer = new UpdateCommandHostedService(
            services,
            Options.Create(new UpdateCommandConfiguration { SeedAdmin = false }),
            NullLogger<UpdateCommandHostedService>.Instance,
            lifetime.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => installer.StartAsync(CancellationToken.None));
        lifetime.Verify(l => l.StopApplication(), Times.Once);
    }
}
