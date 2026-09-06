using Microsoft.AspNetCore.Authentication.Cookies;
using Sati_Net_Last.Admin.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// Registrar repositorio (implementación consumirá la API vía HttpClient)
builder.Services.AddScoped<Sati_Net_Last.Admin.Repositories.IAdminRepository, Sati_Net_Last.Admin.Repositories.AdminRepository>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<TokenHandler>();

// HttpClient para backend
builder.Services.AddHttpClient("BackendAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BackendAPI:BaseUrl"] ?? "http://localhost:5289/");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<TokenHandler>();

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // en producción usar Always
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// IMPORTANTE: autenticación antes de autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MtapiSettings}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public class TokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Busca el JWT guardado en la sesión activa
        var token = _httpContextAccessor.HttpContext?.User.FindFirst("SessionToken")?.Value;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}