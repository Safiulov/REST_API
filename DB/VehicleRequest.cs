using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class VehicleRequest
    {
        public required int Код { get; set; }

        public required string Место { get; set; }
        public required DateTime Дата { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Длина логина должна быть от 5 до 20 символов.")]
        public required string Логин { get; set; }
    }
}
