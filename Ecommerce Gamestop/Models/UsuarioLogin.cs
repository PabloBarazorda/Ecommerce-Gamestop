using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Gamestop.Models
{
    public class UsuarioLogin
    {

        [Required]
        [EmailAddress]
        public string Correo { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; }

    }
}
