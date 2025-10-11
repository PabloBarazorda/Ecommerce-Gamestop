namespace Ecommerce_Gamestop.Models
{
    public class CarritoViewModel
    {
        public int CarritoID { get; set; }

        public int ItemID { get; set; }

        public string NombreItem { get; set; }
        public string TipoItem { get; set; } // detectar accesorio o producto
       
        public string TipoProducto { get; set; } // 🔹 Indica si es Físico o Digital

        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal => Precio * Cantidad;

        public int referenciaId { get; set; }

        public string ImagenURL { get; set; }
        public string CodigoDigital { get; set; }
        public List<string> DireccionesLocales { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaAgregado { get; set; }

    }
}
