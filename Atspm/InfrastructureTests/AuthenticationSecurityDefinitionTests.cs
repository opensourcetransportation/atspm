// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Xunit;

namespace Utah.Udot.Atspm.InfrastructureTests;

public class AuthenticationSecurityDefinitionTests
{
    [Fact]
    public void Swagger_describes_Jwt_and_api_key_as_alternatives()
    {
        var options = new SwaggerGenOptions();
        options.AddAtspmSecurityDefinitions();

        var requirements = options.SwaggerGeneratorOptions.SecurityRequirements;
        Assert.Equal(2, requirements.Count);
        Assert.All(requirements, requirement => Assert.Single(requirement));
        Assert.Contains(requirements, requirement => requirement.Keys.Single().Reference.Id == "Bearer");
        Assert.Contains(requirements, requirement => requirement.Keys.Single().Reference.Id == "ApiKey");
    }
}
