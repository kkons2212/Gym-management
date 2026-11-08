using GymManagement.Server.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký 3 Cổng Adapters ngoại vi (VietQR, Notification, CameraScanner)
builder.Services.AddServerDataAdapters(builder.Configuration);

// Đăng ký Database API (GymDbContext, GenericRepository, UnitOfWork)
builder.Services.AddServerDataRepositories(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));
app.MapControllers();

app.Run();
