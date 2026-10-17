#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityApi - %Namespace%/Program.cs
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
using Identity.Business.Agency;
using Identity.Business.Claims;
using Identity.Business.Tokens;
using Identity.Business.Users;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.Configuration;

//git 2

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ApplyVolumeConfiguration()
    .ConfigureLogging((h, l) => l.AddGoogle(h))
    .ConfigureServices((h, s) =>
    {
        s.AddControllers(o =>
        {
            o.ReturnHttpNotAcceptable = true;
            o.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
            o.Filters.Add(new ProducesAttribute("application/json"));
        });
        s.AddProblemDetails();
        s.AddConfiguredCompression(new[] { "application/json", "application/xml", "text/csv", "application/x-ndjson" });
        s.AddConfiguredSwagger(builder.Configuration, o =>
        {
            o.IncludeXmlComments(typeof(Program).Assembly);
            o.CustomOperationIds((controller, verb, action) => $"{verb}{controller}{action}");
            o.CustomSchemaIds(type => type.Name);
            o.EnableAnnotations();
            o.AddAtspmSecurityDefinitions();
        });
        s.AddConfiguredCors(builder.Configuration);
        s.AddAtspmHttpLogging();
        s.AddAtspmDbContext(h);
        s.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<IdentityContext>()
        .AddDefaultTokenProviders();
        s.AddEmailServices(h);
        s.AddScoped<IAgencyService, AgencyService>();
        s.AddScoped<IAccountService, AccountService>();
        s.AddScoped<ClaimsService, ClaimsService>();
        s.AddScoped<TokenService, TokenService>();
        s.AddScoped<RoleManager<IdentityRole>>();
        s.AddScoped<UserManager<ApplicationUser>>();
        s.AddScoped<UsersService>();
        s.AddPathBaseFilter(h);
        s.AddAtspmAuthentication(h);
        s.AddAtspmAuthorization();
        s.AddHealthChecks();

        s.AddDataProtection().SetApplicationName("TestResetFlow");

        s.Configure<IdentityConfiguration>(h.Configuration.GetSection(nameof(IdentityConfiguration)));
    });

var app = builder.Build();

await app.ApplyMigrations<IdentityContext>(async (services) =>
{
    await services.SeedIdentityData();
    await services.SeedAdminUser();
});

#region Middleware Pipeline

//Error handling
if (!app.Environment.IsProduction())
{
    app.Services.PrintHostInformation();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

//Security
app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

//Cross-cutting
app.UseResponseCompression();
app.UseHttpLogging();
//app.UseMiddleware<DownloadLoggingMiddleware>();

//Swagger
app.UseConfiguredSwaggerUI();

//Endpoints
app.UseStaticFiles();
app.UseCookiePolicy();
app.MapControllers();
app.MapJsonHealthChecks();

#endregion

app.Run();
