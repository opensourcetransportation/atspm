#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityTests - Utah.Udot.Atspm.IdentityTests.Controllers/AccountControllerTests.cs
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

using Identity.Business.Accounts;
using Identity.Controllers;
using Identity.Models.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using System.Net.Mail;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.NetStandardToolkit.Services;
using Xunit;

namespace Utah.Udot.Atspm.IdentityTests.Controllers
{
    public class AccountControllerTests
    {
        private readonly AccountController _accountController;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;

        public AccountControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(_userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);

            _accountServiceMock = new Mock<IAccountService>();
            _emailServiceMock = new Mock<IEmailService>();
            var configurationMock = new Mock<IConfiguration>();

            // Setup the configuration to return specific values for keys
            configurationMock.Setup(x => x["Jwt:Secret"]).Returns("3936A97D8CD9E34B2E5E565F8226F");
            // Add more configuration values if needed
            // configurationMock.Setup(x => x["IdentityServer:Endpoint"]).Returns("...");

            _accountController = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _accountServiceMock.Object,
                _emailServiceMock.Object,
                configurationMock.Object);
        }


        [Fact]
        public async Task Register_ValidModel_ReturnsOk()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                Agency = "Avenue"
            };
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Agency = "Avenue" };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user); // Return the same user as GetUserAsync

            _accountServiceMock.Setup(service => service.CreateUser(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(new AccountResult(StatusCodes.Status200OK, "token", new List<string>(), null));

            // Act
            var result = await _accountController.Register(model);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _accountServiceMock.Verify(service => service.CreateUser(
                It.Is<ApplicationUser>(created => created.Email == model.Email && created.Agency == model.Agency),
                model.Password), Times.Once);
        }

        [Fact]
        public async Task Register_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new RegisterViewModel(); // Invalid model without required properties

            // Act
            var result = await _accountController.Register(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidModel_ReturnsOk()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                RememberMe = false
            };

            // Set up the mock SignInResult for a successful login
            _signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _accountServiceMock.Setup(service => service.Login(model.Email, model.Password, model.RememberMe))
                .ReturnsAsync(new AccountResult(StatusCodes.Status200OK, "token", new List<string>(), null));

            // Set up the mock UserManager to return a valid user when GetUserAsync is called
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Agency = "Avenue" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Set up the mock UserManager to return a valid user when FindByEmailAsync is called
            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user); // Return the same user as GetUserAsync

            // Set up the HttpContext with a valid user principal
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.Name, model.Email) // Use any relevant claims here
            }));

            _accountController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _accountController.Login(model);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _accountServiceMock.Verify(service => service.Login(model.Email, model.Password, model.RememberMe), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new LoginViewModel(); // Invalid model without required properties
            _accountController.ModelState.AddModelError(nameof(model.Email), "Email is required");

            // Act
            var result = await _accountController.Login(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Logout_ReturnsOk()
        {
            _signInManagerMock.Setup(manager => manager.SignOutAsync()).Returns(Task.CompletedTask);
            // Act
            var result = await _accountController.Logout();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _signInManagerMock.Verify(manager => manager.SignOutAsync(), Times.Once);
        }

        //[Fact]
        //public async Task ChangePassword_ValidModel_ReturnsOk()
        //{
        //    // Arrange
        //    var model = new ChangePasswordViewModel
        //    {
        //        CurrentPassword = "OldPassword123!",
        //        NewPassword = "NewPassword123!",
        //        ConfirmPassword = "NewPassword123!"
        //    };

        //    var user = new ApplicationUser();

        //    _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        //        .ReturnsAsync(user);

        //    _userManagerMock.Setup(um => um.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword))
        //        .ReturnsAsync(IdentityResult.Success);

        //    // Act
        //    var result = await _accountController.ChangePassword(model);

        //    // Assert
        //    Assert.IsType<OkResult>(result);
        //}

        [Fact]
        public async Task ChangePassword_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new ChangePasswordViewModel(); // Invalid model without required properties
            _accountController.ModelState.AddModelError(nameof(model.NewPassword), "Password is required");

            // Act
            var result = await _accountController.ChangePassword(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ForgotPassword_ValidModel_ReturnsOk()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                Email = "test@example.com"
            };

            var user = new ApplicationUser { UserName = model.Email };

            _userManagerMock.Setup(um => um.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.IsEmailConfirmedAsync(user))
                .ReturnsAsync(true);

            _userManagerMock.Setup(um => um.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            _emailServiceMock.Setup(service => service.SendEmailAsync(It.IsAny<MailMessage>()))
                .ReturnsAsync(true);

            // Act
            var result = await _accountController.ForgotPassword(Options.Create(new IdentityConfiguration
            {
                Website = "https://example.com",
                DefaultEmailAddress = "noreply@example.com"
            }), model);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ForgotPassword_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                Email = null
            }; // Invalid model without required properties

            // Act
            var result = await _accountController.ForgotPassword(Mock.Of<IOptions<IdentityConfiguration>>(), model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
