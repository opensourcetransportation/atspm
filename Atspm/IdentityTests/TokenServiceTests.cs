// SPDX-License-Identifier: Apache-2.0

using Identity.Business.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Xunit;

namespace Utah.Udot.Atspm.IdentityTests;

public class TokenServiceTests
{
    [Fact]
    public async Task GenerateJwtTokenAsync_AllowsASeededUserWithoutOptionalNames()
    {
        var user = new ApplicationUser { Id = "admin-id", UserName = "admin@example.org", Email = "admin@example.org" };
        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["Admin"]);
        var signInManager = new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null);
        var roleManager = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = new string('k', 64),
            ["Jwt:Issuer"] = "ATSPM-test",
            ["Jwt:ExpireDays"] = "1"
        }).Build();

        var service = new TokenService(config, signInManager.Object, roleManager.Object);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(await service.GenerateJwtTokenAsync(user));

        Assert.Equal("admin@example.org", token.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.DoesNotContain(token.Claims, c => c.Type == JwtRegisteredClaimNames.GivenName);
        Assert.DoesNotContain(token.Claims, c => c.Type == JwtRegisteredClaimNames.FamilyName);
        Assert.Contains(token.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
    }
}
