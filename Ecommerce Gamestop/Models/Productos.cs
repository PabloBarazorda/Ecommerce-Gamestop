using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations; // <-- necesario para [Required]
using Microsoft.AspNetCore.Http;

namespace Ecommerce_Gamestop.Models
{
    public class Productos
    {
        public int ProductoID { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Descripcion { get; set; }
        [Required]
        public decimal Precio { get; set; }
        [Required]
        public string TipoProducto { get; set; }
        [Required]
        public string Plataforma { get; set; }
        [Required(ErrorMessage = "Debes indicar la ruta de la imagen.")]
        public string ImagenURL { get; set; }
        public string Estado { get; set; } = "Activo";
    }
}
