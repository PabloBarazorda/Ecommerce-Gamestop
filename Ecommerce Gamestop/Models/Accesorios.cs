using System.ComponentModel.DataAnnotations;
namespace Ecommerce_Gamestop.Models

{
    public class Accesorios
    {
        public int AccesorioID { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Compatibilidad { get; set; }
        [Required(ErrorMessage = "Debe ingresar un precio válido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        public string Marca { get; set; }

        public string Modelo { get; set; }

        public string TipoProducto { get; set; } = "Fisico";

        public string ImagenURL { get; set; }

        public string Estado { get; set; } = "Activo";
    }
}
