using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Auth.API.Models;

namespace Auth.API.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int TokenId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "O token de redefinição de senha é obrigatório.")]
        [StringLength(256, ErrorMessage = "O token não pode ter mais de 256 caracteres.")]
        public string Token { get; set; }

        [Required(ErrorMessage = "A data de expiração é obrigatória.")]
        [DataType(DataType.DateTime, ErrorMessage = "A data de expiração deve ser válida.")]
        public DateTime ExpirationDate { get; set; }

        public virtual User User { get; set; }
    }
}
