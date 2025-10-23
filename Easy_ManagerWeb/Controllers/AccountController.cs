using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace Easy_ManagerWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            // Redireciona se já estiver logado via sessão
            var usuario = HttpContext.Session.GetString("usuario_logado");
            if (!string.IsNullOrEmpty(usuario))
                return RedirectToAction("Protegida");

            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(DadosUsuario dados, bool continuarConectado)
        {
            // Verifica usuário no banco
            var usuario = _context.usuarios
                .FirstOrDefault(u => u.Usuario == dados.Usuario && u.Senha == dados.Senha);

            if (usuario != null)
            {
                // Salva na sessão
                HttpContext.Session.SetString("usuario_logado", usuario.Usuario);

                // Cria cookie persistente se "Continuar conectado" estiver marcado
                if (continuarConectado)
                {
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddDays(7),
                        Path = "/",
                        HttpOnly = true,
                        SameSite = SameSiteMode.Lax,
                        Secure = false // true em produção
                    };
                    Response.Cookies.Append("usuario_logado", usuario.Usuario, cookieOptions);
                }

                return RedirectToAction("Protegida");
            }

            ViewBag.Erro = "Usuário ou senha inválidos.";
            return View();
        }

        // GET: Protegida
        public IActionResult Protegida()
        {
            // Primeiro verifica sessão
            var usuario = HttpContext.Session.GetString("usuario_logado");

            // Se não houver sessão, tenta recuperar do cookie
            if (string.IsNullOrEmpty(usuario))
            {
                usuario = Request.Cookies["usuario_logado"];
                if (!string.IsNullOrEmpty(usuario))
                {
                    HttpContext.Session.SetString("usuario_logado", usuario);
                }
            }

            if (string.IsNullOrEmpty(usuario))
                return RedirectToAction("Login");

            ViewBag.Usuario = usuario;
            return View();
        }

        // GET: Logout
        public IActionResult Logout()
        {
            // Limpa sessão
            HttpContext.Session.Clear();

            // Limpa cookie persistente
            if (Request.Cookies.ContainsKey("usuario_logado"))
            {
                Response.Cookies.Append("usuario_logado", "",
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddDays(-1),
                        Path = "/",
                        HttpOnly = true,
                        SameSite = SameSiteMode.Lax
                    });
            }

            return RedirectToAction("Login");
        }
    }
}
