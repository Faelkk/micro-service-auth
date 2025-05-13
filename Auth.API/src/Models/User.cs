using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.API.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail fornecido não é válido.")]
        [StringLength(150, ErrorMessage = "O e-mail não pode ter mais de 150 caracteres.")]
        public string Email { get; set; }

        [StringLength(200, ErrorMessage = "A senha não pode ter mais de 200 caracteres.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "O papel (role) é obrigatório.")]
        [StringLength(50, ErrorMessage = "O papel (role) não pode ter mais de 50 caracteres.")]
        public string Role { get; set; }
    }
}
