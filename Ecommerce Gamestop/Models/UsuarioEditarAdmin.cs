namespace Ecommerce_Gamestop.Models
{
    public class UsuarioEditarAdmin
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string ContraseniaHash { get; set; }  // editable
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string TipoUsuario { get; set; } // Admin, Cliente o Empleado
    }
}
