namespace Ecommerce_Gamestop.Models
{
    public class UsuarioListado
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string TipoUsuario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string ImagenURL { get; set; }
    }
}
