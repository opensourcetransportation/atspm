#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.ATSPM.ConfigApi.DTO/GitHubReleaseDto.cs
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

namespace Utah.Udot.ATSPM.ConfigApi.DTO
{
    /// <summary>
    /// Represents a simplified, API‑friendly view of a GitHub release,
    /// suitable for returning to clients without exposing internal GitHub URLs.
    /// </summary>
    public class GitHubReleaseDto
    {
        /// <summary>
        /// The unique numeric identifier for the release.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The tag name associated with the release (e.g., <c>v5.2.0-rc2</c>).
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// The display name of the release.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The release notes or description text.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The public HTML URL for viewing the release on GitHub.
        /// </summary>
        public string HtmlUrl { get; set; }

        /// <summary>
        /// Public GitHub repository configured as the source for release checks.
        /// </summary>
        public string SourceRepositoryUrl { get; set; }

        /// <summary>
        /// Indicates whether the release is a draft.
        /// </summary>
        public bool Draft { get; set; }

        /// <summary>
        /// Indicates whether the release is marked as a prerelease.
        /// </summary>
        public bool Prerelease { get; set; }

        /// <summary>
        /// Indicates whether this release is the newest release
        /// according to the server's versioning logic.
        /// </summary>
        public bool IsLatest { get; set; }

        /// <summary>
        /// The timestamp when the release was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The timestamp when the release was published.
        /// </summary>
        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// Information about the GitHub user who authored the release.
        /// </summary>
        public GitHubAuthorDto Author { get; set; }

        /// <summary>
        /// A collection of assets attached to the release.
        /// </summary>
        public List<GitHubAssetDto> Assets { get; set; }
    }
}
