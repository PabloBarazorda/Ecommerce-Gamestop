using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Gamestop.Models
{
    public class UsuarioRegistro
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Contrasenia { get; set; }
        public string ConfirmarContrasenia { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string TipoUsuario { get; set; }
        public IFormFile Imagen { get; set; }
    }
}
