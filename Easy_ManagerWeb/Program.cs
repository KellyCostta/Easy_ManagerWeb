using Easy_ManagerWeb.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Conexão MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))); builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,           // tenta novamente até 5 vezes
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        })
);


// Controllers e Views
builder.Services.AddControllersWithViews();

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

// Static files e rotas
app.UseStaticFiles();
app.UseRouting();

// Sessão precisa ser inicializada antes do uso
app.UseSession();

// Adiciona autenticação/autorização (mesmo que não use ainda)
app.UseAuthentication();
app.UseAuthorization();

// Middleware customizado (depois de Session!)
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";

    // Evita erro de rota nula
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

    await next.Invoke(); // importante usar Invoke()
});



// Rota padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Protegida}/{id?}"
);

app.Run();
