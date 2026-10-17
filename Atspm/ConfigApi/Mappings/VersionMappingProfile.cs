#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.ATSPM.ConfigApi.Mappings/VersionMappingProfile.cs
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

using AutoMapper;
using Utah.Udot.ATSPM.ConfigApi.DTO;
using Utah.Udot.NetStandardToolkit.Services.GitHubReleaseService;

namespace Utah.Udot.ATSPM.ConfigApi.Mappings
{
    /// <summary>
    /// Defines the AutoMapper configurations for GitHub release entities.
    /// </summary>
    public class VersionMappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionMappingProfile"/> class.
        /// </summary>
        public VersionMappingProfile()
        {
            CreateMap<GitHubAuthor, GitHubAuthorDto>();

            CreateMap<GitHubAsset, GitHubAssetDto>();

            CreateMap<GitHubRelease, GitHubReleaseDto>()
                .ForMember(dest => dest.SourceRepositoryUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Assets, opt => opt.MapFrom(src => src.Assets));
        }
    }
}
