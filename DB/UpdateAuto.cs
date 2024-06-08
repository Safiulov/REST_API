using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class UpdateAuto
    {
        [Required]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Длина марки должна быть от 2 до 15 символов.")]
        public required string Марка { get; set; }
        public required string Цвет { get; set; }
        public required string Тип { get; set; }
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Госномер должен состоять из 5 символов.")]
        public required string Госномер { get; set; }
        [Required]
        [Range(1950, int.MaxValue, ErrorMessage = "Год должен быть не меньше 1950.")]
        public required int Год { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Длина логина должна быть от 5 до 20 символов.")]
        public required string Логин { get; set; }
    }
}
