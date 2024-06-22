using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class AutoClientDto
    {
        public required string FIO { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Длина логина должна быть от 5 до 20 символов.")]
        public required string Login { get; set; }
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Длина пароля должна быть от 5 до 20 символов.")]
        public required string Password { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Длина марки должна быть от 2 до 15 символов.")]
        public required string Mark { get; set; }
        public required string Color { get; set; }
        public required string Type { get; set; }
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Госномер должен состоять из 6 символов.")]
        public required string GovernmentNumber { get; set; }
        [Required]
        [Range(1950, int.MaxValue, ErrorMessage = "Год должен быть не меньше 1950.")]
        public required int Year { get; set; }
    }
}
