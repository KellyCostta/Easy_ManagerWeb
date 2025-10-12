using Microsoft.AspNetCore.Mvc;

namespace Easy_ManagerWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Tenta recuperar da sess�o
            var usuario = HttpContext.Session.GetString("usuario_logado");

            // Se n�o houver, tenta restaurar do cookie
            if (string.IsNullOrEmpty(usuario))
            {
                usuario = Request.Cookies["usuario_logado"];
                if (!string.IsNullOrEmpty(usuario))
                {
                    HttpContext.Session.SetString("usuario_logado", usuario);
                    usuario = Request.Cookies["usuario_logado"];
                }
            }

            // Passa o usu�rio para a view
            ViewBag.Usuario = usuario;

            return View();
        }
    }
}
