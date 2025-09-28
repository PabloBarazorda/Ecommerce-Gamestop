using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Gamestop.Models
{
    public class PagoViewModel
    {
        [Required(ErrorMessage = "Nombre del titular requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre del titular")]
        public string NombreTitular { get; set; }

        [Required(ErrorMessage = "Número de tarjeta requerido")]
        [CreditCard(ErrorMessage = "Número de tarjeta inválido")]
        [Display(Name = "Número de tarjeta")]
        public string NumeroTarjeta { get; set; }

        [Required(ErrorMessage = "Fecha de expiración requerida")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Formato MM/AA")]
        [Display(Name = "Expira (MM/AA)")]
        public string FechaExp { get; set; }

        [Required(ErrorMessage = "CVV requerido")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV invalido")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }

        // Si quieres forzar descarga PDF al pagar
        public bool GenerarPDF { get; set; }
    }
}
