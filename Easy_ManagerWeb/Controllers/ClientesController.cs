using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace Easy_ManagerWeb.Controllers
{
    public class ClientesController : Controller
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Clientes/Gerenciamento_clientes
        public IActionResult Gerenciamento_clientes()
        {
            var clientes = _context.Clientes.ToList(); // pega do banco
            return View(clientes);
        }

        // GET: /Clientes/Novo_Cliente
        public IActionResult Novo_Cliente()
        {
            return View();
        }

        // POST: /Clientes/Novo_Cliente
        [HttpPost]
        public IActionResult Novo_Cliente(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                cliente.DataCadastro = DateTime.Now;
                _context.Clientes.Add(cliente); // adiciona no banco
                _context.SaveChanges(); // salva no MySQL
                return RedirectToAction("Gerenciamento_clientes");
            }
            return View(cliente);
        }
    }
}
