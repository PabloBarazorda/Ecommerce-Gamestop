using Ecommerce_Gamestop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecommerce_Gamestop.Controllers
{
    public class CarritoController : Controller
    {
        private readonly IConfiguration _configuration;

        public CarritoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Ver carrito
        public IActionResult Index()
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            List<CarritoViewModel> carrito = new List<CarritoViewModel>();

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_VerCarrito", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    carrito.Add(new CarritoViewModel
                    {
                        CarritoID = Convert.ToInt32(reader["CarritoID"]),
                        ProductoID = Convert.ToInt32(reader["ProductoID"]),
                        Producto = reader["Producto"].ToString(),
                        Precio = Convert.ToDecimal(reader["Precio"]),
                        Cantidad = Convert.ToInt32(reader["Cantidad"]),
                        Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                        ImagenURL = reader["ImagenURL"].ToString()
                    });
                }
            }

            return View(carrito);
        }

        // Agregar al carrito
        [HttpPost]
        public IActionResult Agregar(int productoID, int cantidad = 1)
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_AgregarAlCarrito", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                cmd.Parameters.AddWithValue("@ProductoID", productoID);
                cmd.Parameters.AddWithValue("@Cantidad", cantidad);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        // Eliminar del carrito
        [HttpPost]
        public IActionResult Eliminar(int carritoID)
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_EliminarDelCarrito", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CarritoID", carritoID);
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }



        // Mostrar formulario de pago
        public IActionResult Pagar()
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            return View();
        }

        [HttpPost]
        public IActionResult ProcesarPago(PagoViewModel pago)
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            if (string.IsNullOrEmpty(pago.NombreTitular) || string.IsNullOrEmpty(pago.NumeroTarjeta))
                return Content("Datos de pago incompletos.");

            // Obtener carrito del usuario
            List<CarritoViewModel> carrito = new List<CarritoViewModel>();
            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_VerCarrito", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    carrito.Add(new CarritoViewModel
                    {
                        ProductoID = Convert.ToInt32(reader["ProductoID"]),
                        Producto = reader["Producto"].ToString(),
                        Precio = Convert.ToDecimal(reader["Precio"]),
                        Cantidad = Convert.ToInt32(reader["Cantidad"]),
                        Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                        TipoProducto = reader["TipoProducto"].ToString() // Agregamos TipoProducto
                    });
                }
            }

            // Generar códigos o direcciones según tipo de producto
            List<string> digitalCodes = new List<string>();
            List<string> storeLocations = new List<string>();

            foreach (var item in carrito)
            {
                if (item.TipoProducto == "Digital")
                {
                    Random rnd = new Random();
                    item.CodigoDigital = rnd.Next(100_000_000, 999_999_999).ToString();
                }
                else if (item.TipoProducto == "Fisico")
                {
                    item.DireccionesLocales = new List<string>
        {
            "Local 1: Av. Lima 123, Lima",
            "Local 2: Av. Arequipa 456, Lima",
            "Local 3: Jr. Cusco 789, Lima",
            "Local 4: Av. Brasil 321, Lima",
            "Local 5: Av. Bolivia 654, Lima"
        };
                }
            }


            // Limpiar carrito después del pago
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_VaciarCarrito", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                cmd.ExecuteNonQuery();
            }

            // Pasar datos a la vista Boleta
            var boletaViewModel = new BoletaViewModel
            {
                Productos = carrito,
                DireccionesFisicas = storeLocations,
                CodigosDigitales = digitalCodes,
                Total = carrito.Sum(x => x.Subtotal)
            };

            return View("Boleta", boletaViewModel);
        }



    }
}
