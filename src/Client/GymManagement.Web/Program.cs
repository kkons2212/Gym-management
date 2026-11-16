using GymManagement.Web.Components;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Authentication & Core Services
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());

// Cấu hình linh hoạt: Cho phép bật/tắt Mock API qua biến môi trường USE_MOCK_API (giống Desktop launchSettings.json)
string apiBaseUrl = builder.Configuration["GYM_API_BASE_URL"]
    ?? Environment.GetEnvironmentVariable("GYM_API_BASE_URL")
    ?? "http://localhost:5000";

string? useMockEnv = builder.Configuration["USE_MOCK_API"]
    ?? Environment.GetEnvironmentVariable("USE_MOCK_API");

bool useMockApi = string.Equals(useMockEnv, "true", StringComparison.OrdinalIgnoreCase);

if (useMockApi)
{
    builder.Services.AddScoped<IWebApiClient, MockWebApiClient>();
}
else
{
    builder.Services.AddHttpClient<IWebApiClient, WebApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(15);
    });
}
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
