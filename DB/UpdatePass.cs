using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class UpdatePass
    {
        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Длина пароля должна быть от 5 до 20 символов.")]
        public required string NewPassword { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
       


    }
}
