using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using Ecommerce_Gamestop.Models;

namespace Ecommerce_Gamestop.Controllers
{
    public class MailController : Controller
    {
        private readonly IConfiguration _configuration;

        public MailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Renderiza el formulario (GET)
        [HttpGet]
        public IActionResult Correo()
        {
            return View();
        }

        // Procesa el formulario (POST)
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public IActionResult Correo(Email modelo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "Por favor complete todos los campos.";
                return View(modelo);
            }

            try
            {
                string from = _configuration["EmailSettings:From"];
                string password = _configuration["EmailSettings:Password"];

                MailMessage mail = new MailMessage(from, modelo.EmailDestino)
                {
                    Subject = "GameStop Perú - Confirmación de Mensaje",
                    Body = $"Hola {modelo.Nombre},\n\n" +
                           $"Hemos recibido tu mensaje desde nuestra página web con el siguiente asunto: \"{modelo.Asunto}\".\n\n" +
                           $"Mensaje:\n{modelo.Mensaje}\n\n" +
                           "Está a la espera de ser atendido(a) por un miembro del personal de atención al cliente.\n\n" +
                           "📌 Las consultas suelen ser respondidas por lo general entre 24 y 48 horas tras haberse realizado el envío del mensaje. ¡Gracias por escribirnos y mantente conectado!\n\n" +
                           "Atentamente,\nGameStop Perú",
                    IsBodyHtml = false
                };

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(from, password);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }

                // Redirige a la vista de resultado
                return RedirectToAction("Resultado", new { mensaje = "Mensaje enviado exitosamente, por favor revise su correo personal." });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Resultado", new { mensaje = "Error al enviar el Mensaje: " + ex.Message });
            }
        }

        // Vista de confirmación
        [HttpGet]
        public IActionResult Resultado(string mensaje)
        {
            ViewBag.Mensaje = mensaje;
            return View();
        }
    }
}