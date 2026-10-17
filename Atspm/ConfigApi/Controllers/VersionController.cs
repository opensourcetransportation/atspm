#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/VersionController.cs
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

using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Options;
using System.Reflection;
using Utah.Udot.ATSPM.ConfigApi.DTO;
using Utah.Udot.NetStandardToolkit.Exceptions;
using Utah.Udot.NetStandardToolkit.Configuration;
using Utah.Udot.NetStandardToolkit.Services.GitHubReleaseService;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// Provides a specialized OData-enabled interface for interacting with application versioning and release metadata.
    /// </summary>
    /// <remarks>
    /// This controller serves as a bridge between the running application's assembly metadata and the project's external GitHub repository. 
    /// It leverages the <see cref="IGitHubReleaseService"/> to fetch real-time release information, assets, and changelogs.
    /// 
    /// Because this controller inherits from <see cref="ODataController"/>, it supports standard OData query syntax ($filter, $select, $top, etc.) 
    /// on collection-based endpoints, allowing clients to precisely query the project's release history.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="VersionController"/> class using dependency injection.
    /// </remarks>
    /// <param name="github">The service responsible for communicating with the GitHub API to fetch release data.</param>
    /// <param name="mapper">The AutoMapper instance used to transform domain models into API-friendly Data Transfer Objects.</param>
    /// <param name="configuration">The repository configured as the source of release checks.</param>
    [ApiVersion(1.0)]
    [Produces("application/json")]
    public class VersionController(IGitHubReleaseService github, IMapper mapper, IOptions<GitHubReleaseConfiguration> configuration) : ODataController
    {
        private readonly IGitHubReleaseService _github = github;
        private readonly IMapper _mapper = mapper;

        private GitHubReleaseDto MapRelease(GitHubRelease release)
        {
            var dto = _mapper.Map<GitHubReleaseDto>(release);
            var source = configuration.Value;
            dto.SourceRepositoryUrl = $"https://github.com/{source.RepositoryOwner}/{source.RepositoryName}";
            return dto;
        }

        /// <summary>
        /// Retrieves the specific release details for the version currently running in this environment.
        /// </summary>
        /// <remarks>
        /// This method identifies the current version by inspecting the assembly's Informational Version attribute. 
        /// It then queries GitHub for the corresponding tag. If the assembly version cannot be determined or 
        /// the tag does not exist on GitHub, an error is returned.
        /// </remarks>
        /// <returns>A <see cref="GitHubReleaseDto"/> containing metadata, release notes, and assets for the current version.</returns>
        /// <response code="200">Returns the release details matching the current assembly version.</response>
        /// <response code="400">Returned if the assembly version metadata is missing or malformed.</response>
        /// <response code="404">Returned if no matching release tag was found in the GitHub repository.</response>
        /// <response code="502">Returned if there was a communication failure or rate-limit issue with the GitHub API.</response>
        [HttpGet("api/v{version:apiVersion}/GetCurrentVersion")]
        [ProducesResponseType(typeof(GitHubReleaseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetCurrentVersion()
        {
            try
            {
                var assembly = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Utah.Udot.Atspm");

                var info = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                var tag = info?.InformationalVersion?.Split('+')[0];

                if (string.IsNullOrEmpty(tag))
                    return BadRequest("Could not determine current assembly version.");

                if (!tag.StartsWith("v"))
                    tag = "v" + tag;

                var release = await _github.GetReleaseByTag(tag);

                return Ok(MapRelease(release));
            }
            catch (GitHubReleaseNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (GitHubApiException ex)
            {
                return StatusCode(502, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the most recent release published to the GitHub repository.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to check for updates. By default, this only returns stable releases. 
        /// Set the <paramref name="PreRelease"/> parameter to true if you wish to include beta or release candidate versions.
        /// </remarks>
        /// <param name="PreRelease">A boolean flag indicating whether to include versions marked as 'Pre-release' on GitHub. Defaults to false.</param>
        /// <returns>The latest <see cref="GitHubReleaseDto"/> based on the publication date.</returns>
        /// <response code="200">Returns the latest release matching the specified criteria.</response>
        /// <response code="502">Returned if the upstream GitHub API is unavailable or returns an error.</response>
        [HttpGet("api/v{version:apiVersion}/GetLatestVersion(PreRelease={PreRelease})")]
        [ProducesResponseType(typeof(GitHubReleaseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetLatestVersion(bool PreRelease = false)
        {
            try
            {
                var latest = await _github.GetLatestRelease(PreRelease);
                return Ok(MapRelease(latest));
            }
            catch (GitHubApiException ex)
            {
                return StatusCode(502, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a complete historical list of all releases associated with this project.
        /// </summary>
        /// <remarks>
        /// This endpoint supports standard OData query options such as $filter, $orderby, and $top. 
        /// It provides a chronological history of all versions, including their respective release notes and assets.
        /// </remarks>
        /// <param name="PreRelease">A boolean flag; if true, the history will include pre-release versions (RC, Beta, etc.). Defaults to false.</param>
        /// <returns>A collection of <see cref="GitHubReleaseDto"/> objects.</returns>
        /// <response code="200">Returns the requested list of releases.</response>
        /// <response code="502">Returned if the project's release history cannot be retrieved from GitHub.</response>
        [EnableQuery]
        [HttpGet("api/v{version:apiVersion}/GetVersionHistory(PreRelease={PreRelease})")]
        [ProducesResponseType(typeof(IEnumerable<GitHubReleaseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetVersionHistory(bool PreRelease = false)
        {
            try
            {
                var history = await _github.GetReleaseHistory(PreRelease);
                var dtos = history.Select(MapRelease).ToList();
                return Ok(dtos.AsQueryable());
            }
            catch (GitHubApiException ex)
            {
                return StatusCode(502, ex.Message);
            }
        }
    }
}

