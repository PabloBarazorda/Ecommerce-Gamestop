namespace Ecommerce_Gamestop.Models
{
    public class BoletaViewModel
    {
        public List<CarritoViewModel> Productos { get; set; }
        public List<string> DireccionesFisicas { get; set; }
        public List<string> CodigosDigitales { get; set; }
        public decimal Total { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaEmision { get; set; }
        public string CodigoPedido { get; set; }
    }
}
