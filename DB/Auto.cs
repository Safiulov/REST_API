using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public partial class Auto
    {
        public int Код_авто { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Длина марки должна быть от 2 до 15 символов.")]
        public required string Марка { get; set; }
        public required string Цвет { get; set; }
        public required string Тип { get; set; }
        [Required]
        [StringLength(10, MinimumLength =6 , ErrorMessage = "Госномер должен состоять из 6-10 символов.")]
        public required string Госномер { get; set; }
        [Required]
        [Range(1950, int.MaxValue, ErrorMessage = "Год должен быть не меньше 1950.")]
        public int Год { get; set; }
  
    }
}
