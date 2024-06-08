using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class Period
    {
        public int Код_услуги { get; set; }
        public required string Название { get; set; }
        public required DateTime Дата_въезда { get; set; }   
        public required string ФИО {  get; set; }
        [Required]
        [EmailAddress]
        public required string Почта { get; set; } 
        public required int Сумма { get; set; }
    }
}
