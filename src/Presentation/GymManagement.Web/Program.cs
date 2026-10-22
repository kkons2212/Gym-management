using GymManagement.Core.Contracts;
using GymManagement.Core.Services;
using GymManagement.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Đăng ký dịch vụ tầng 2 (Mock hiện tại, cắm EF Core sau này)
var mockGymService = new MockGymService();
builder.Services.AddSingleton<IAuthService>(mockGymService);
builder.Services.AddSingleton<IMemberService>(mockGymService);
builder.Services.AddSingleton<ICheckInService>(mockGymService);
builder.Services.AddSingleton<IPtService>(mockGymService);
builder.Services.AddSingleton<IBillingService>(mockGymService);
builder.Services.AddSingleton<IReportService>(mockGymService);
builder.Services.AddSingleton<ISystemSettingService>(mockGymService);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
