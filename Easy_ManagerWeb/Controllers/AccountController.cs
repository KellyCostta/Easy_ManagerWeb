using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace Easy_ManagerWeb.Controllers
{
    public class AccountController : Controller
    {
        private static List<DadosUsuario> listaUsuarios = new List<DadosUsuario>()
        {
            new DadosUsuario { Usuario = "jose", Senha = "123" },
            new DadosUsuario { Usuario = "maria", Senha = "321" }
        };

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(DadosUsuario dados, bool continuarConectado)
        {
            if (listaUsuarios.Any(u => u.Usuario == dados.Usuario && u.Senha == dados.Senha))
            {
                // Salva usuário na sessão
                HttpContext.Session.SetString("usuario_logado", dados.Usuario);

                if (continuarConectado)
                {
                    var option = new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddDays(7),
                        HttpOnly = true,
                        Secure = false,          // localhost não precisa de HTTPS
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax // garante que navegador aceite o cookie
                    };
                    Response.Cookies.Append("usuario_logado", dados.Usuario, option);
                }


                return RedirectToAction("Protegida");
            }

            ViewBag.Erro = "Usuário ou senha inválidos.";
            return View();
        }


        public IActionResult Protegida()
        {
            var usuario = HttpContext.Session.GetString("usuario_logado");

            // Se sessão acabou, tenta pegar do cookie
            if (usuario == null)
            {
                usuario = Request.Cookies["usuario_logado"];
                if (!string.IsNullOrEmpty(usuario))
                {
                    HttpContext.Session.SetString("usuario_logado", usuario);
                }
            }

            if (usuario == null)
                return RedirectToAction("Login");

            ViewBag.Usuario = usuario;
            ViewBag.UsuarioSession = usuario;
            return View();
        }





        public IActionResult Logout()
        {
            // 🔹 Limpa sessão e cookie
            HttpContext.Session.Clear();

            if (Request.Cookies["usuario_logado"] != null)
            {
                Response.Cookies.Delete("usuario_logado");
            }

            return RedirectToAction("Login");
        }
        

        // Opcional: método para restaurar sessão via AJAX
        [HttpPost]
        public IActionResult RestoreSession()
        {
            var usuario = Request.Cookies["usuario_logado"];
            if (!string.IsNullOrEmpty(usuario))
            {
                HttpContext.Session.SetString("usuario_logado", usuario);
            }
            return Ok();
        }
    }
}
