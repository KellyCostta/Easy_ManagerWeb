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
        public IActionResult Login(DadosUsuario dados)
        {
            if (listaUsuarios.Any(u => u.Usuario == dados.Usuario && u.Senha == dados.Senha))
            {
                // salva o usuário logado em sessão
                HttpContext.Session.SetString("usuario_logado", dados.Usuario);
                return RedirectToAction("Protegida");
            }
            ViewBag.Erro = "Usuário ou senha inválidos.";
            return View();
        }

        public IActionResult Protegida()
        {
            var usuario = HttpContext.Session.GetString("usuario_logado");
            if (usuario == null)
                return RedirectToAction("Login");

            ViewBag.Usuario = usuario;
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
