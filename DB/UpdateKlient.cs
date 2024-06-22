using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class UpdateKlient
    {
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Длина логина должна быть от 5 до 20 символов.")]
        public required string OldLogin { get; set; }
        public required string FIO { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Длина логина должна быть от 5 до 20 символов.")]
        public required string NewLogin { get; set; }
    }
}
