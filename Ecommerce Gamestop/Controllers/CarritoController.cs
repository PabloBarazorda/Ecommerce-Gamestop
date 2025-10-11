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
            _converter = new SynchronizedConverter(new PdfTools());
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
        }

        // ============================
        // VER CARRITO
        // ============================
        public IActionResult Index()
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            List<CarritoViewModel> carrito = new List<CarritoViewModel>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("cn")))
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
                        ItemID = Convert.ToInt32(reader["ItemID"]),
                        NombreItem = reader["NombreItem"].ToString(),
                        TipoItem = reader["TipoItem"].ToString(),
                        TipoProducto = reader["TipoProducto"].ToString(),
                        Precio = Convert.ToDecimal(reader["Precio"]),
                        Cantidad = Convert.ToInt32(reader["Cantidad"]),
                        ImagenURL = reader["ImagenURL"].ToString()
                    });
                }
            }

            return View(carrito);
        }

        // ============================
        // AGREGAR AL CARRITO
        // ============================
        [HttpPost]
        public IActionResult Agregar(int itemID, int referenciaId, string tipoItem, int cantidad = 1)
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("cn")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_AgregarAlCarrito", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                cmd.Parameters.AddWithValue("@ItemID", DBNull.Value);
                cmd.Parameters.AddWithValue("@TipoItem", tipoItem);  // 👈 nuevo parámetro
                cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                cmd.Parameters.AddWithValue("@ReferenciaID", referenciaId);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }


        // ============================
        // ELIMINAR DEL CARRITO
        // ============================
        [HttpPost]
        public IActionResult Eliminar(int carritoID)
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("cn")))
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

        // ============================
        // FORMULARIO DE PAGO
        // ============================
        public IActionResult Pagar()
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            return View();
        }

        // ============================
        // MÉTODO DE RENDERIZADO DE VISTAS A STRING (para PDF)
        // ============================
        private async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor);

            using var sw = new StringWriter();
            var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
                throw new ArgumentNullException($"{viewName} no se encontró.");

            var viewDictionary = new ViewDataDictionary<TModel>(ViewData, model);

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

        // ============================
        // PROCESAR PAGO Y GENERAR BOLETA
        // ============================
        [HttpPost]
        public async Task<IActionResult> ProcesarPago(PagoViewModel pago)
        {
            int? usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
                return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid)
                return View("Pagar", pago);

            List<CarritoViewModel> carrito = new List<CarritoViewModel>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("cn")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_VerCarrito", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        carrito.Add(new CarritoViewModel
                        {
                            ItemID = Convert.ToInt32(reader["ItemID"]),
                            NombreItem = reader["NombreItem"].ToString(),
                            TipoItem = reader["TipoItem"].ToString(),
                            TipoProducto = reader["TipoProducto"].ToString(),
                            Precio = Convert.ToDecimal(reader["Precio"]),
                            Cantidad = Convert.ToInt32(reader["Cantidad"]),
                            ImagenURL = reader["ImagenURL"].ToString()
                        });
                    }
                }
            }

            // 🔹 Generar códigos o direcciones según tipo de ítem
            foreach (var item in carrito)
            {
                // Normalizar strings para evitar nulls y espacios
                string tipoItem = (item.TipoItem ?? "").Trim().ToLower();
                string tipoProducto = (item.TipoProducto ?? "").Trim().ToLower();

                // Accesorios → siempre direcciones
                if (tipoItem == "accesorio")
                {
                    item.DireccionesLocales = new List<string>
        {
            "Local 1: CENTRO COMERCIAL PLAZA, Av. de la Marina 2000, San Miguel",
            "Local 2: Jockey Plaza, Av. Javier Prado Este 4200, Santiago de Surco",
            "Local 3: C.C. MegaPlaza, Av. Globo Terráqueo 3698, Independencia",
            "Local 4: C.C. Real Plaza Centro Cívico, Av. Garcilaso de la Vega 1337, Lima",
            "Local 5: Real Plaza Salaverry, Jesús María"
        };
                    continue; // ya procesado, pasamos al siguiente
                }

                // Productos digitales → código de descarga
                if (tipoItem == "producto" && tipoProducto.Contains("digital"))
                {
                    item.CodigoDigital = new Random().Next(100_000_000, 999_999_999).ToString();
                    continue;
                }

                // Productos físicos → direcciones
                if (tipoItem == "producto" && (tipoProducto.Contains("fisico") || tipoProducto.Contains("físico")))
                {
                    item.DireccionesLocales = new List<string>
        {
            "Local 1: CENTRO COMERCIAL PLAZA, Av. de la Marina 2000, San Miguel",
            "Local 2: Jockey Plaza, Av. Javier Prado Este 4200, Santiago de Surco",
            "Local 3: C.C. MegaPlaza, Av. Globo Terráqueo 3698, Independencia",
            "Local 4: C.C. Real Plaza Centro Cívico, Av. Garcilaso de la Vega 1337, Lima",
            "Local 5: Real Plaza Salaverry, Jesús María"
        };
                }
            }




            // 🔹 Vaciar carrito
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("cn")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_VaciarCarrito", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                cmd.ExecuteNonQuery();
            }

            // 🔹 Crear modelo de boleta
            var codigoBoleta = "PED-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            var boletaViewModel = new BoletaViewModel
            {
                Productos = carrito,
                Total = carrito.Sum(x => x.Subtotal),
                NombreUsuario = pago.NombreTitular,
                FechaEmision = DateTime.Now,
                CodigoPedido = codigoBoleta
            };

            // 🔹 Registrar boleta en BD
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("cn")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_RegistrarBoleta", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                cmd.Parameters.AddWithValue("@NumeroBoleta", codigoBoleta);
                cmd.Parameters.AddWithValue("@Total", boletaViewModel.Total);

                cmd.ExecuteNonQuery();
            }

            // 🔹 Generar PDF si el usuario lo pide
            if (pago.GenerarPDF)
            {
                string html = await RenderViewToStringAsync("Boleta", boletaViewModel);

                lock (_converter)
                {
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
            }

            // 🔹 Mostrar boleta en vista
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


        public IActionResult Gracias()
        {
            ViewBag.NombreUsuario = TempData["NombreUsuario"];
            ViewBag.Total = TempData["Total"];
            return View();
        }

        public IActionResult ListadoVentas()
        {
            List<CarritoViewModel> compras = new List<CarritoViewModel>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("cn")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_ListarVentas", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    compras.Add(new CarritoViewModel
                    {
                        CarritoID = Convert.ToInt32(reader["CarritoID"]),
                        ItemID = reader["TipoItem"].ToString() == "Producto" ? Convert.ToInt32(reader["CarritoID"]) : 0,
                        NombreItem = reader["NombreItem"].ToString(),
                        TipoItem = reader["TipoItem"].ToString(),
                        Precio = Convert.ToDecimal(reader["Precio"]),
                        Cantidad = Convert.ToInt32(reader["Cantidad"]),
                        NombreUsuario = reader["NombreUsuario"].ToString(),
                        FechaAgregado = Convert.ToDateTime(reader["FechaAgregado"])
                    });
                }
            }

            return View(compras);
        }


        public IActionResult ListadoBoletas()
        {
            List<BoletaViewModel> lista = new List<BoletaViewModel>();
            string connectionString = _configuration.GetConnectionString("cn");

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ListarBoletas", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new BoletaViewModel
                    {
                        NumeroBoleta = dr["NumeroBoleta"].ToString(),
                        Usuario = dr["Usuario"].ToString(),
                        Total = Convert.ToDecimal(dr["Total"]),
                        FechaEmision = Convert.ToDateTime(dr["FechaEmision"])
                    });
                }
            }

            return View(lista);
        }


    }
}
