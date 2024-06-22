using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class Report_of_free
    {
        public required string Место { get; set; }
        public required string ФИО { get; set; }
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Госномер должен состоять из 5 символов.")]
        public required string Госномер { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Длина марки должна быть от 2 до 15 символов.")]
        public required string Марка { get; set; }
        public required DateTime Дата_выезда { get; set; }
        
    }
}
