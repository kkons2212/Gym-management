using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using GymManagement.Server.Core.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace GymManagement.Server.ApiTests;

public sealed class ApiPipelineTests : IClassFixture<ApiApplicationFactory>
{
    private readonly ApiApplicationFactory _factory;

    public ApiPipelineTests(ApiApplicationFactory factory)
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

    [Theory]
    [InlineData("/test-api/errors/validation", System.Net.HttpStatusCode.BadRequest, "VALIDATION_ERROR", "Invalid request.")]
    [InlineData("/test-api/errors/not-found", System.Net.HttpStatusCode.NotFound, "NOT_FOUND", "Record not found.")]
    [InlineData("/test-api/errors/business-rule", System.Net.HttpStatusCode.Conflict, "BUSINESS_RULE_VIOLATION", "Operation conflicts with current state.")]
    public async Task KnownException_ShouldReturnMappedApiError(
        string path,
        System.Net.HttpStatusCode expectedStatus,
        string expectedErrorCode,
        string expectedMessage)
    {
        using var client = _factory.CreateAuthenticatedClient();

        using var response = await client.GetAsync(path);
        using var body = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        Assert.Equal(expectedStatus, response.StatusCode);
        AssertErrorResponse(body.RootElement, expectedErrorCode);
        Assert.Equal(expectedMessage, body.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task UnexpectedException_ShouldNotExposeInternalDetails()
    {
        using var client = _factory.CreateAuthenticatedClient();

        using var response = await client.GetAsync("/test-api/errors/unexpected");
        using var body = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        AssertErrorResponse(body.RootElement, "INTERNAL_SERVER_ERROR");
        Assert.DoesNotContain("private diagnostic detail", body.RootElement.GetRawText(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvalidModelState_ShouldReturnStandardValidationResponse()
    {
        using var client = _factory.CreateAuthenticatedClient();

        using var response = await client.PostAsJsonAsync("/test-api/validation", new { });
        using var body = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        AssertErrorResponse(body.RootElement, "VALIDATION_ERROR");
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("name", out _));
    }

    private static void AssertErrorResponse(System.Text.Json.JsonElement body, string expectedErrorCode)
    {
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal(expectedErrorCode, body.GetProperty("errorCode").GetString());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, body.GetProperty("data").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("message").GetString()));
    }
}

public sealed class ApiApplicationFactory : WebApplicationFactory<Program>
{
    private const string Issuer = "GymManagement.ApiPipelineTests";
    private const string Audience = "GymManagement.ApiPipelineTests.Client";
    private const string SigningKey = "test-signing-key-with-at-least-32-bytes";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Issuer", Issuer);
        builder.UseSetting("Jwt:Audience", Audience);
        builder.UseSetting("Jwt:SigningKey", SigningKey);
        builder.ConfigureServices(services =>
        {
            services
                .AddControllers()
                .AddApplicationPart(typeof(TestApiController).Assembly);
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: new[] { new Claim(ClaimTypes.Role, "Admin") },
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: signingCredentials);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            new JwtSecurityTokenHandler().WriteToken(token));
        return client;
    }
}

[ApiController]
[Route("test-api")]
public sealed class TestApiController : ControllerBase
{
    [HttpGet("anonymous")]
    public IActionResult GetAnonymous() => Ok();

    [HttpGet("errors/validation")]
    public IActionResult ThrowValidationError() => throw new ValidationException("Invalid request.");

    [HttpGet("errors/not-found")]
    public IActionResult ThrowNotFoundError() => throw new NotFoundException("Record not found.");

    [HttpGet("errors/business-rule")]
    public IActionResult ThrowBusinessRuleError() =>
        throw new BusinessRuleException("Operation conflicts with current state.");

    [HttpGet("errors/unexpected")]
    public IActionResult ThrowUnexpectedError() =>
        throw new InvalidOperationException("private diagnostic detail");

    [HttpPost("validation")]
    public IActionResult ValidateRequest([FromBody] TestRequest request) => Ok(request);

    public sealed class TestRequest
    {
        [Required]
        public string? Name { get; init; }
    }
}
