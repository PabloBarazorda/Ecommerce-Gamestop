using Ecommerce_Gamestop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Ecommerce_Gamestop.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IConfiguration _configuration;

        public UsuarioController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(UsuarioLogin model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string connectionString = _configuration.GetConnectionString("cn"); // <-- usa la key "cn"

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open(); // aquí debería funcionar
                SqlCommand cmd = new SqlCommand("SELECT UsuarioID, Nombre, TipoUsuario, ImagenURL FROM Usuarios WHERE Correo=@Correo AND ContraseniaHash=@Hash",conn);
                cmd.Parameters.AddWithValue("@Correo", model.Correo);
                cmd.Parameters.AddWithValue("@Hash", model.Contrasenia);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    HttpContext.Session.SetInt32("UsuarioID", reader.GetInt32(0));
                    HttpContext.Session.SetString("NombreUsuario", reader.GetString(1));
                    HttpContext.Session.SetString("TipoUsuario", reader.GetString(2));
                    HttpContext.Session.SetString("ImagenUsuario", reader["ImagenURL"] != DBNull.Value ? reader["ImagenURL"].ToString() : "");


                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                    return View(model);
                }
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Limpiar sesión                           
            TempData["Mensaje"] = "Se ha cerrado sesión exitosamente.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Perfil()
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login");

            // Aquí puedes obtener datos del usuario desde la BD y enviarlos a la vista Perfil
            return View();
        }



        // GET: Registrar
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(UsuarioRegistro model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string connectionString = _configuration.GetConnectionString("cn");
            string fileName = null;

            if (model.Imagen != null && model.Imagen.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/usuarios");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Imagen.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.Imagen.CopyTo(stream);
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_InsertarUsuario", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", model.Apellido);
                cmd.Parameters.AddWithValue("@Correo", model.Correo);
                cmd.Parameters.AddWithValue("@ContraseniaHash", model.Contrasenia);
                cmd.Parameters.AddWithValue("@Telefono", model.Telefono);
                cmd.Parameters.AddWithValue("@Direccion", model.Direccion);
                cmd.Parameters.AddWithValue("@TipoUsuario", model.TipoUsuario);
                cmd.Parameters.AddWithValue("@ImagenURL", (object)fileName ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Login");
        }



        [HttpPost]
        public async Task<IActionResult> Editar(UsuarioEditar model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string fileName = null;

            if (model.Imagen != null && model.Imagen.Length > 0)
            {
                // Guardar imagen en wwwroot/img/usuarios
                string uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/usuarios");
                fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.Imagen.FileName)}";
                string filePath = Path.Combine(uploads, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Imagen.CopyToAsync(fileStream);
                }
            }

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_ActualizarUsuario", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UsuarioID", model.UsuarioID);
                cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", model.Apellido);
                cmd.Parameters.AddWithValue("@Telefono", model.Telefono);
                cmd.Parameters.AddWithValue("@Direccion", model.Direccion);

                if (!string.IsNullOrEmpty(fileName))
                    cmd.Parameters.AddWithValue("@ImagenURL", fileName);
                else
                    cmd.Parameters.AddWithValue("@ImagenURL", DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            // Actualizar sesión si se cambió el nombre o la imagen
            HttpContext.Session.SetString("NombreUsuario", model.Nombre);
            if (!string.IsNullOrEmpty(fileName))
                HttpContext.Session.SetString("ImagenUsuario", fileName);

            TempData["Success"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Perfil");
        }



    }
}
