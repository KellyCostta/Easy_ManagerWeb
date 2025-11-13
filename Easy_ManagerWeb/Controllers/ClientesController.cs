using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Easy_ManagerWeb.Controllers
{
    public class ClientesController : Controller
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Clientes/Gerenciamento_clientes (Listagem de Clientes)
        public IActionResult Gerenciamento_clientes()
        {
            var clientes = _context.Clientes.ToList();
            return View(clientes);
        }

        // GET: /Clientes/Novo_Cliente (Exibir formulário de cadastro)
        public IActionResult Novo_Cliente()
        {
            return View();
        }

        // POST: /Clientes/Novo_Cliente (Processar formulário de cadastro)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Novo_Cliente(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                cliente.DataCadastro = DateTime.Now;
                _context.Clientes.Add(cliente);
                _context.SaveChanges();
                return RedirectToAction("Gerenciamento_clientes");
            }
            return View(cliente);
        }

        // GET: /Clientes/Editar_Cliente/5 (Exibir formulário de edição)
        public IActionResult Editar_Cliente(int id)
        {
            var cliente = _context.Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: /Clientes/Editar_Cliente/5 (Processar edição sem a correção de data)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar_Cliente(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                // A data será redefinida na próxima vez que for lida.
                _context.Clientes.Update(cliente);

                _context.SaveChanges();

                return RedirectToAction("Gerenciamento_clientes");
            }

            return View(cliente);
        }

        // GET: /Clientes/Excluir_Cliente/5 (Exibir tela de confirmação de exclusão)
        public IActionResult Excluir_Cliente(int id)
        {
            var cliente = _context.Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: /Clientes/Excluir_Cliente/5 (Executar a exclusão)
        [HttpPost, ActionName("Excluir_Cliente")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarExclusao(int id)
        {
            var cliente = _context.Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                _context.SaveChanges();
            }

            return RedirectToAction("Gerenciamento_clientes");
        }

        [HttpPost]
        public IActionResult Novo_ClienteAjax([FromBody] Cliente cliente)
        {
            if (cliente == null)
                return BadRequest("Dados inválidos.");

            // ⚠️ Aqui garantimos que mesmo vindo de nomes diferentes no JSON, preenchemos as propriedades corretas
            cliente.DataCadastro = DateTime.Now;
            _context.Clientes.Add(cliente);
            _context.SaveChanges();

            return Json(new { id = cliente.Id, nome = cliente.Nome });
        }


    }
}