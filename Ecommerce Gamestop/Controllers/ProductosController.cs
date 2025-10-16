using Ecommerce_Gamestop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecommerce_Gamestop.Controllers
{
    public class ProductosController : Controller
    {
        private readonly IConfiguration _configuration;

        public ProductosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ---- VISTAS PARA USUARIO ----
        public ActionResult Index()
        {
            List<Productos> productos = new List<Productos>();

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ListarProductos", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Estado", "Activo");

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    productos.Add(new Productos
                    {
                        ProductoID = (int)reader["ProductoID"],
                        Nombre = reader["Nombre"].ToString(),
                        Descripcion = reader["Descripcion"].ToString(),
                        Precio = (decimal)reader["Precio"],
                        TipoProducto = reader["TipoProducto"].ToString(),
                        Plataforma = reader["Plataforma"].ToString(),
                        ImagenURL = reader["ImagenURL"].ToString(),
                        Estado = reader["Estado"].ToString()
                    });
                }
            }

            return View(productos); // Vista para mostrar catálogo a clientes
        }

        // ---- VISTAS PARA ADMIN ----
        public ActionResult Gestion()
        {
            return RedirectToAction("IndexAdmin");
        }

        public ActionResult IndexAdmin()
        {
            List<Productos> productos = new List<Productos>();

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ListarProductosAdmin", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    productos.Add(new Productos
                    {
                        ProductoID = (int)reader["ProductoID"],
                        Nombre = reader["Nombre"].ToString(),
                        Descripcion = reader["Descripcion"].ToString(),
                        Precio = (decimal)reader["Precio"],
                        TipoProducto = reader["TipoProducto"].ToString(),
                        Plataforma = reader["Plataforma"].ToString(),
                        ImagenURL = reader["ImagenURL"].ToString(),
                        Estado = reader["Estado"].ToString()
                    });
                }
            }

            return View(productos); // Vista especial para Admin
        }

        [HttpGet]
        public ActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Productos producto)
        {
            if (!ModelState.IsValid)
                return View(producto);

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_InsertarProducto", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
                cmd.Parameters.AddWithValue("@Precio", producto.Precio);
                cmd.Parameters.AddWithValue("@TipoProducto", producto.TipoProducto);
                cmd.Parameters.AddWithValue("@Plataforma", producto.Plataforma);
                cmd.Parameters.AddWithValue("@ImagenURL", producto.ImagenURL);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("IndexAdmin");
        }



        [HttpGet]
        public ActionResult Editar(int id)
        {
            Productos producto = null;

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerProductoPorID", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProductoID", id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    producto = new Productos
                    {
                        ProductoID = (int)reader["ProductoID"],
                        Nombre = reader["Nombre"].ToString(),
                        Descripcion = reader["Descripcion"].ToString(),
                        Precio = (decimal)reader["Precio"],
                        TipoProducto = reader["TipoProducto"].ToString(),
                        Plataforma = reader["Plataforma"].ToString(),
                        ImagenURL = reader["ImagenURL"].ToString(),
                        Estado = reader["Estado"].ToString()
                    };
                }
            }

            return View(producto);
        }

        [HttpPost]
        public ActionResult Editar(Productos producto)
        {
            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ActualizarProducto", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ProductoID", producto.ProductoID);
                cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
                cmd.Parameters.AddWithValue("@Precio", producto.Precio);
                cmd.Parameters.AddWithValue("@TipoProducto", producto.TipoProducto);
                cmd.Parameters.AddWithValue("@Plataforma", producto.Plataforma);
                cmd.Parameters.AddWithValue("@ImagenURL", producto.ImagenURL);
                cmd.Parameters.AddWithValue("@Estado", producto.Estado);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("IndexAdmin");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            string connectionString = _configuration.GetConnectionString("cn");

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_EliminarProducto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProductoID", id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Mensaje"] = "Producto eliminado correctamente.";
            return RedirectToAction("IndexAdmin");
        }
    }
}
