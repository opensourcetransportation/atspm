using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Utah.Udot.Atspm.ConfigApi.Controllers;
using Utah.Udot.ATSPM.ConfigApi.DTO;
using Utah.Udot.NetStandardToolkit.Services.GitHubReleaseService;
using Utah.Udot.NetStandardToolkit.Configuration;

namespace ConfigApiTests;

[TestClass]
public class VersionControllerTests
{
    [DataTestMethod]
    [DataRow(false, "utahudot", "udot-atspm")]
    [DataRow(true, "example", "atspm")]
    public async Task LatestVersionIncludesConfiguredSourceWithoutReleaseUrl(bool prerelease, string owner, string repository)
    {
        var service = new Mock<IGitHubReleaseService>();
        var mapper = new Mock<IMapper>();
        var release = new GitHubRelease { Name = "v5.3.1" };
        service.Setup(s => s.GetLatestRelease(prerelease)).ReturnsAsync(release);
        mapper.Setup(m => m.Map<GitHubReleaseDto>(release)).Returns(new GitHubReleaseDto { Name = release.Name });
        var configuration = Options.Create(new GitHubReleaseConfiguration
        {
            RepositoryOwner = owner,
            RepositoryName = repository
        });
        var controller = new VersionController(service.Object, mapper.Object, configuration);

        var response = (OkObjectResult)await controller.GetLatestVersion(prerelease);
        var dto = (GitHubReleaseDto)response.Value!;

        Assert.AreEqual($"https://github.com/{owner}/{repository}", dto.SourceRepositoryUrl);
        Assert.AreEqual("v5.3.1", dto.Name);
        Assert.IsNull(dto.HtmlUrl);
        service.Verify(s => s.GetLatestRelease(prerelease), Times.Once);
    }
}
