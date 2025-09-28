using DinkToPdf;
using DinkToPdf.Contracts;
using Ecommerce_Gamestop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecommerce_Gamestop.Controllers
{
    public class CarritoController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IConverter _converter;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;

        public CarritoController(IConfiguration configuration, IConverter converter,
                         IRazorViewEngine razorViewEngine,
                         ITempDataProvider tempDataProvider)
        {
            _configuration = configuration;
            _converter = new SynchronizedConverter(new PdfTools()); // convertidor de pdf de boleta
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
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

        // rendering codigo


        private async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor);

            using var sw = new StringWriter();
            var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
                throw new ArgumentNullException($"{viewName} no se encontró.");

            var viewDictionary = new ViewDataDictionary<TModel>(
                ViewData,
                model
            );

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }


        // final de rendering


        [HttpPost]
        public async Task<IActionResult> ProcesarPago(PagoViewModel pago)
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            // Validar modelo con DataAnnotations
            if (!ModelState.IsValid)
            {
                // Si algo está mal, lo devuelves al formulario de pago
                return View("Pagar", pago);
            }

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
                        TipoProducto = reader["TipoProducto"].ToString()
                    });
                }
            }

            // Generar códigos/direcciones según tipo
            foreach (var item in carrito)
            {
                if (item.TipoProducto == "Digital")
                    item.CodigoDigital = new Random().Next(100_000_000, 999_999_999).ToString();
                else if (item.TipoProducto == "Fisico")
                    item.DireccionesLocales = new List<string>
            {
                "Local 1: CENTRO COMERCIAL PLAZA, Av. de la Marina 2000, San Miguel",
                "Local 2: Jockey Plaza, Av. Javier Prado Este 4200, Santiago de Surco",
                "Local 3: C.C. MegaPlaza, Avenida Globo Terráqueo 3698, Independencia",
                "Local 4: C.C. Real Plaza Centro Cívico, Av. Garcilaso de la Vega 1337, Lima",
                "Local 5: Centro Comercial Real Plaza Salaverry, Jesús María"
            };
            }

            // Vaciar carrito
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_VaciarCarrito", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                cmd.ExecuteNonQuery();
            }

            // Preparar boleta
            var boletaViewModel = new BoletaViewModel
            {
                Productos = carrito,
                Total = carrito.Sum(x => x.Subtotal),
                NombreUsuario = pago.NombreTitular,
                FechaEmision = DateTime.Now,
                CodigoPedido = "PED-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };

            // Si el usuario eligió descargar PDF
            if (pago.GenerarPDF)
            {
                string html = await RenderViewToStringAsync("Boleta", boletaViewModel);

                var pdfDoc = new HtmlToPdfDocument()
                {
                    GlobalSettings = new GlobalSettings
                    {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait
                    },
                    Objects = { new ObjectSettings { HtmlContent = html } }
                };

                var pdf = _converter.Convert(pdfDoc);
                return File(pdf, "application/pdf", "Boleta.pdf");
            }

            // Caso contrario: mostrar boleta en navegador
            return View("Boleta", boletaViewModel);
        }



        [HttpPost]
        public async Task<IActionResult> GenerarBoletaPDF(BoletaViewModel boleta)
        {
            string html = await RenderViewToStringAsync("Boleta", boleta);

            var pdfDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            PaperSize = PaperKind.A4,
            Orientation = Orientation.Portrait,
            DocumentTitle = "Boleta de Compra"
        },
                Objects = {
            new ObjectSettings() { HtmlContent = html }
        }
            };

            byte[] pdf = _converter.Convert(pdfDoc);
            return File(pdf, "application/pdf", "Boleta.pdf");
        }


    }
}
