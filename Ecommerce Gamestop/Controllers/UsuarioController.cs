using Ecommerce_Gamestop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
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


        // GET: Listado

        public IActionResult Listado()
        {
            List<UsuarioListado> usuarios = new List<UsuarioListado>();
            string connectionString = _configuration.GetConnectionString("cn");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_ListarUsuarios", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new UsuarioListado
                        {
                            UsuarioID = reader["UsuarioID"] != DBNull.Value ? Convert.ToInt32(reader["UsuarioID"]) : 0,
                            Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : string.Empty,
                            Apellido = reader["Apellido"] != DBNull.Value ? reader["Apellido"].ToString() : string.Empty,
                            Correo = reader["Correo"] != DBNull.Value ? reader["Correo"].ToString() : string.Empty,
                            Telefono = reader["Telefono"] != DBNull.Value ? reader["Telefono"].ToString() : string.Empty,
                            Direccion = reader["Direccion"] != DBNull.Value ? reader["Direccion"].ToString() : string.Empty,
                            TipoUsuario = reader["TipoUsuario"] != DBNull.Value ? reader["TipoUsuario"].ToString() : string.Empty,
                            ImagenURL = reader["ImagenURL"] != DBNull.Value ? reader["ImagenURL"].ToString() : null
                        });
                    }
                }
            }

            return View(usuarios);
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

            // 📸 Guardar imagen si existe
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

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Verificado para ver si correo existe o no
                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE Correo = @Correo", conn);
                    checkCmd.Parameters.AddWithValue("@Correo", model.Correo);

                    int existe = (int)checkCmd.ExecuteScalar();
                    if (existe > 0)
                    {
                        ModelState.AddModelError("Correo", "⚠️ El correo ingresado ya está registrado.");
                        return View(model);
                    }

                    // Registro de usuario
                    SqlCommand cmd = new SqlCommand("sp_InsertarUsuario", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", model.Apellido);
                    cmd.Parameters.AddWithValue("@Correo", model.Correo);
                    cmd.Parameters.AddWithValue("@ContraseniaHash", model.Contrasenia);
                    cmd.Parameters.AddWithValue("@Telefono", model.Telefono);
                    cmd.Parameters.AddWithValue("@Direccion", model.Direccion);
                    cmd.Parameters.AddWithValue("@ImagenURL", (object)fileName ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("Login");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) // Violación UNIQUE constraint
                {
                    ModelState.AddModelError("Correo", "El correo ya está registrado a una cuenta, intente con otro.");
                    return View(model);
                }

                ModelState.AddModelError("", "Ocurrió un error al registrar el usuario. Intente nuevamente.");
                return View(model);
            }
        }



        [HttpGet]
        public IActionResult Editar(int id)
        {
            UsuarioEditar usuario = null;

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT UsuarioID, Nombre, Apellido, Telefono, Direccion, ImagenURL FROM Usuarios WHERE UsuarioID=@UsuarioID", conn);
                cmd.Parameters.AddWithValue("@UsuarioID", id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    usuario = new UsuarioEditar
                    {
                        UsuarioID = (int)reader["UsuarioID"],
                        Nombre = reader["Nombre"].ToString(),
                        Apellido = reader["Apellido"].ToString(),
                        Telefono = reader["Telefono"].ToString(),
                        Direccion = reader["Direccion"].ToString(),
                        ImagenURL = reader["ImagenURL"] != DBNull.Value ? reader["ImagenURL"].ToString() : null
                    };
                }
                else
                {
                    return NotFound();
                }
            }

            return View(usuario);
        }


        [HttpPost]
        public async Task<IActionResult> Editar(UsuarioEditar usuario)
        {
            string fileName = null;

            if (usuario.Imagen != null && usuario.Imagen.Length > 0)
            {
                string uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/usuarios");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                fileName = $"{Guid.NewGuid()}{Path.GetExtension(usuario.Imagen.FileName)}";
                string filePath = Path.Combine(uploads, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await usuario.Imagen.CopyToAsync(fileStream);
                }
            }

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ActualizarUsuario", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UsuarioID", usuario.UsuarioID);
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono);
                cmd.Parameters.AddWithValue("@Direccion", usuario.Direccion);
                cmd.Parameters.AddWithValue("@ImagenURL", (object)fileName ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Editar", new { id = usuario.UsuarioID });
        }



        // 🔹 OBTENER DATOS POR ID (GET)
        [HttpGet]
        public IActionResult EditarAdmin(int id)
        {
            string connectionString = _configuration.GetConnectionString("cn");
            UsuarioEditarAdmin model = new UsuarioEditarAdmin();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ObtenerUsuarioPorID", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UsuarioID", id);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.UsuarioID = (int)reader["UsuarioID"];
                        model.Nombre = reader["Nombre"].ToString();
                        model.Apellido = reader["Apellido"].ToString();
                        model.Correo = reader["Correo"].ToString();
                        model.ContraseniaHash = reader["ContraseniaHash"].ToString();
                        model.Telefono = reader["Telefono"].ToString();
                        model.Direccion = reader["Direccion"].ToString();
                        model.TipoUsuario = reader["TipoUsuario"].ToString();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }

            return View(model);
        }


        // 🔹 ACTUALIZAR DATOS (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarAdmin(UsuarioEditarAdmin model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string connectionString = _configuration.GetConnectionString("cn");

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ActualizarUsuarioAdmin", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UsuarioID", model.UsuarioID);
                cmd.Parameters.AddWithValue("@Nombre", model.Nombre ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Apellido", model.Apellido ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Correo", model.Correo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefono", model.Telefono ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Direccion", model.Direccion ?? (object)DBNull.Value);

                // Solo se actualiza si se escribió algo
                if (string.IsNullOrWhiteSpace(model.ContraseniaHash))
                    cmd.Parameters.AddWithValue("@ContraseniaHash", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@ContraseniaHash", model.ContraseniaHash);

                // Validar tipo usuario
                string tipo = (model.TipoUsuario == "Admin" || model.TipoUsuario == "Cliente" || model.TipoUsuario == "Empleado")
                    ? model.TipoUsuario
                    : null;

                cmd.Parameters.AddWithValue("@TipoUsuario", (object)tipo ?? DBNull.Value);

                conn.Open();
                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                    TempData["Mensaje"] = "Usuario actualizado correctamente.";
                else
                    TempData["Mensaje"] = "No se pudo actualizar el usuario.";
            }

            return RedirectToAction("Listado", new { id = model.UsuarioID });
        }



// GET: /Usuario/Eliminar
public IActionResult EliminarAdmin(int id)
        {
            string connectionString = _configuration.GetConnectionString("cn");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_EliminarUsuario", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UsuarioID", id);
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Listado");
        }


    }
}
