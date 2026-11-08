using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GymManagement.Server.ApiTests;

public sealed class JwtAuthenticationTests : IClassFixture<JwtApplicationFactory>
{
    private readonly JwtApplicationFactory _factory;

    public JwtAuthenticationTests(JwtApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_WhenAnonymous_ShouldRemainAvailable()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WhenAnonymous_ShouldReturnUnauthorized()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/test-api/protected");
        using var body = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        AssertStandardAuthError(body.RootElement, "UNAUTHORIZED");
    }

    [Fact]
    public async Task ProtectedEndpoint_WhenTokenIsMalformed_ShouldReturnUnauthorized()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "not-a-jwt");

        using var response = await client.GetAsync("/test-api/protected");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WhenAuthenticatedAsMember_ShouldReturnForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient("Member");

        using var response = await client.GetAsync("/test-api/admin");
        using var body = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        AssertStandardAuthError(body.RootElement, "FORBIDDEN");
    }

    [Fact]
    public async Task AdminEndpoint_WhenAuthenticatedAsAdmin_ShouldSucceed()
    {
        using var client = _factory.CreateAuthenticatedClient("Admin");

        using var response = await client.GetAsync("/test-api/admin");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    private static void AssertStandardAuthError(System.Text.Json.JsonElement body, string expectedErrorCode)
    {
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal(expectedErrorCode, body.GetProperty("errorCode").GetString());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, body.GetProperty("data").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("message").GetString()));
    }
}

public sealed class JwtApplicationFactory : WebApplicationFactory<Program>
{
    public const string Issuer = "GymManagement.ApiTests";
    public const string Audience = "GymManagement.ApiTests.Client";

    public string SigningKey { get; } = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Issuer", Issuer);
        builder.UseSetting("Jwt:Audience", Audience);
        builder.UseSetting("Jwt:SigningKey", SigningKey);
        builder.ConfigureServices(services =>
        {
            services
                .AddControllers()
                .AddApplicationPart(typeof(JwtTestApiController).Assembly);
        });
    }

    public HttpClient CreateAuthenticatedClient(string role)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAuthorizationHeader(role);
        return client;
    }

    private System.Net.Http.Headers.AuthenticationHeaderValue CreateAuthorizationHeader(string role)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: new[] { new Claim(ClaimTypes.Role, role) },
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            new JwtSecurityTokenHandler().WriteToken(token));
    }
}

[ApiController]
[Route("test-api")]
public sealed class JwtTestApiController : ControllerBase
{
    [HttpGet("protected")]
    public IActionResult GetProtected() => Ok();

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin")]
    public IActionResult GetAdmin() => Ok();
}
