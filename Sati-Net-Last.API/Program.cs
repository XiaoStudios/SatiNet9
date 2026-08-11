using Microsoft.EntityFrameworkCore;
using MTsocketAPI.MT5;
using OfficeOpenXml;
using Sati_Net_Last.API;
using Sati_Net_Last.API.Hubs;
using Sati_Net_Last.API.MTRepositories;
using Sati_Net_Last.API.MTRepositories.Interfaces;
using Sati_Net_Last.API.Repositories.Implementations;
using Sati_Net_Last.API.Repositories.Interfaces;
using Sati_Net_Last.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Set EPPlus license context
// Set EPPlus license context for EPPlus 8+
// ExcelPackage.License = new EPPlusLicenseContext(LicenseContext.NonCommercial);
ExcelPackage.License.SetNonCommercialPersonal("Jesus Ivan Vazquez");

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var serverVersion = new MySqlServerVersion(new Version(8, 0, 46));
// builder.Services.AddDbContext<SatiDevContext>(options => options.UseMySql("server=localhost;database=mydb;user=myuser;password=mypassword", serverVersion));
builder.Services.AddDbContext<SatiDevContext>(options => options.UseMySql(builder.Configuration.GetConnectionString("sati_dev_db"), serverVersion));
// builder.Services.AddDbContext<SatiDevContext>(options => options.UseMySql(builder.Configuration.GetConnectionString("sati_dev_db"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("sati_dev_db"))));

builder.Services.AddSingleton<Terminal>();
builder.Services.AddSingleton<TerminalRepo>();
builder.Services.AddScoped<IExcelRepository, ExcelRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IMTRepo, MTRepo>();

builder.Services.AddSingleton<OperativeAlgorithmSvc>();

// Add SignalR services
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin => true); // For dev only!
    }); 
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

// Map SignalR hubs
app.UseCors();
app.MapHub<MetaTraderHub>("/metatraderhub").RequireCors();

app.Run();
