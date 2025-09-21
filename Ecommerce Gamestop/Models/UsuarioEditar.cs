namespace Ecommerce_Gamestop.Models
{
    public class UsuarioEditar
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }

        public string ImagenURL { get; set; } // ruta actual en BD
        public IFormFile Imagen { get; set; } // nueva foto (opcional)

    }
}
