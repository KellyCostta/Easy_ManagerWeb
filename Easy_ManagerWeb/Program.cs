using Easy_ManagerWeb.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Conexão MySQL via Pomelo
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Controllers e Views
builder.Services.AddControllersWithViews();

// Configura sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // tempo de expiração da sessão
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // true em produção com HTTPS
    options.Cookie.Name = ".EasyManager.Session";
});

var app = builder.Build();

// Middlewares
app.UseStaticFiles();
app.UseRouting();

// ⚠️ Session deve vir antes do middleware que acessa HttpContext.Session
app.UseSession();

// Middleware para recriar sessão a partir do cookie persistente
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;

    if (path != null && !path.StartsWith("/Account/Login") && !path.StartsWith("/Account/Logout"))
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

    await next();
});

// Rota padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Protegida}/{id?}"
);

app.Run();
