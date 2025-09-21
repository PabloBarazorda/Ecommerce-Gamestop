namespace Ecommerce_Gamestop.Models
{
    public class BoletaViewModel
    {
        public List<CarritoViewModel> Productos { get; set; }
        public List<string> DireccionesFisicas { get; set; }
        public List<string> CodigosDigitales { get; set; }
        public decimal Total { get; set; }
    }
}
