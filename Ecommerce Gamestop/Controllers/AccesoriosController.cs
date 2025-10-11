using Ecommerce_Gamestop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ecommerce_Gamestop.Controllers
{
    public class AccesoriosController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccesoriosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            List<Accesorios> lista = new List<Accesorios>();

            string connectionString = _configuration.GetConnectionString("cn");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT * FROM Accesorios";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Accesorios
                    {
                        AccesorioID = (int)dr["AccesorioID"],
                        Nombre = dr["Nombre"].ToString(),
                        Descripcion = dr["Descripcion"].ToString(),
                        Compatibilidad = dr["Compatibilidad"].ToString(),
                        Precio = (decimal)dr["Precio"],
                        Marca = dr["Marca"].ToString(),
                        Modelo = dr["Modelo"].ToString(),
                        TipoProducto = dr["TipoProducto"].ToString(),
                        ImagenURL = dr["ImagenURL"].ToString(),
                        Estado = dr["Estado"].ToString()
                    });
                }
            }

            return View(lista);
        }

        // REGISTRAR GET
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Accesorios accesorio)
        {
            // 🔍 1️⃣ Siempre mostrar los errores detectados en ModelState
            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState)
                {
                    foreach (var error in item.Value.Errors)
                    {
                        Console.WriteLine($"Campo: {item.Key} → Error: {error.ErrorMessage}");
                    }
                }

                ViewBag.Error = "⚠️ Complete correctamente todos los campos obligatorios.";
                return View(accesorio);
            }

            // 🧠 2️⃣ Si es válido, intentamos guardar
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("cn")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("sp_RegistrarAccesorio", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", accesorio.Nombre);
                    cmd.Parameters.AddWithValue("@Descripcion", accesorio.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Compatibilidad", accesorio.Compatibilidad ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Precio", accesorio.Precio);
                    cmd.Parameters.AddWithValue("@Marca", accesorio.Marca ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Modelo", accesorio.Modelo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ImagenURL", accesorio.ImagenURL ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                TempData["Mensaje"] = "✅ Accesorio registrado correctamente.";
                return RedirectToAction("IndexAdmin");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"❌ Error al registrar el accesorio: {ex.Message}";
                return View(accesorio);
            }
        }




        [HttpGet]
        public IActionResult Editar(int id)
        {
            Accesorios acc = null;

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("cn")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("sp_ObtenerAccesorioPorID", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AccesorioID", id);

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    acc = new Accesorios
                    {
                        AccesorioID = Convert.ToInt32(dr["AccesorioID"]),
                        Nombre = dr["Nombre"].ToString(),
                        Descripcion = dr["Descripcion"].ToString(),
                        Compatibilidad = dr["Compatibilidad"].ToString(),
                        Precio = Convert.ToDecimal(dr["Precio"]),
                        Marca = dr["Marca"].ToString(),
                        Modelo = dr["Modelo"].ToString(),
                        TipoProducto = dr["TipoProducto"].ToString(),
                        ImagenURL = dr["ImagenURL"].ToString(),
                        Estado = dr["Estado"].ToString()
                    };
                }
            }

            return View(acc);
        }


        [HttpPost]
        public IActionResult Editar(Accesorios accesorio)
        {
            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("cn");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("sp_ActualizarAccesorio", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@AccesorioID", accesorio.AccesorioID);
                    cmd.Parameters.AddWithValue("@Nombre", accesorio.Nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", accesorio.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Compatibilidad", accesorio.Compatibilidad ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Precio", accesorio.Precio);
                    cmd.Parameters.AddWithValue("@Marca", accesorio.Marca ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Modelo", accesorio.Modelo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TipoProducto", accesorio.TipoProducto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ImagenURL", accesorio.ImagenURL ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", accesorio.Estado ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                TempData["Mensaje"] = "Accesorio actualizado correctamente.";
                return RedirectToAction("IndexAdmin");
            }

            return View(accesorio);
        }

        // ELIMINAR (POST)
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("cn")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("sp_EliminarAccesorio", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AccesorioID", id);
                cmd.ExecuteNonQuery();
            }

            TempData["Mensaje"] = "🗑️ Accesorio eliminado correctamente.";
            return RedirectToAction("IndexAdmin");
        }

        // PANEL ADMIN (LISTA CON BOTONES)
        public IActionResult IndexAdmin()
        {
            List<Accesorios> lista = new List<Accesorios>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("cn")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("sp_ListarAccesorios", con);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new Accesorios
                    {
                        AccesorioID = Convert.ToInt32(dr["AccesorioID"]),
                        Nombre = dr["Nombre"].ToString(),
                        Precio = Convert.ToDecimal(dr["Precio"]),
                        Marca = dr["Marca"].ToString(),
                        Modelo = dr["Modelo"].ToString(),
                        ImagenURL = dr["ImagenURL"].ToString(),
                        Estado = dr["Estado"].ToString()
                    });
                }
            }

            return View(lista);
        }
    }
}
