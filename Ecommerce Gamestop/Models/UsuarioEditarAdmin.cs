namespace Ecommerce_Gamestop.Models
{
    public class UsuarioEditarAdmin
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public IFormFile Imagen { get; set; }
        public string ImagenURL { get; set; }
        public string TipoUsuario { get; set; } // Admin o Cliente
    }
}
