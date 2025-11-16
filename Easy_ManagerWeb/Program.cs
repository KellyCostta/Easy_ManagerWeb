using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);

// --- Forçar cultura brasileira ---
var defaultCulture = new CultureInfo("pt-BR");
defaultCulture.NumberFormat.CurrencySymbol = "R$";

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};



// Conexão MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    )
);




builder.Services.AddControllersWithViews();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"./keys"))
    .SetApplicationName("Easy_ManagerWeb");


// Configura sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.Name = ".EasyManager.Session";
});

var app = builder.Build();
// Aplica cultura pt-BR no site inteiro
app.UseRequestLocalization(localizationOptions);

app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();



app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";


    if (!path.StartsWith("/Account/Login") && !path.StartsWith("/Account/Logout"))
    {
        var usuario = context.Session.GetString("usuario_logado");

        if (string.IsNullOrEmpty(usuario))
        {
            usuario = context.Request.Cookies["usuario_logado"];
            if (!string.IsNullOrEmpty(usuario))
            {
                context.Session.SetString("usuario_logado", usuario);
            }
        }
    }

    await next.Invoke();
});




app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Protegida}/{id?}"
);

app.Run();
