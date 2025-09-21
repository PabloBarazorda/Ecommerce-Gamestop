namespace Ecommerce_Gamestop.Models
{
    public class CarritoViewModel
    {
        public int CarritoID { get; set; }
        public int ProductoID { get; set; }
        public string Producto { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public string TipoProducto { get; set; }
        public string ImagenURL { get; set; }
        public string CodigoDigital { get; set; }
        public List<string> DireccionesLocales { get; set; }

    }
}
