using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class Klients
    {
        public int Код_клиента { get; set; }
        public required string ФИО { get; set; }
        public DateTime Дата_рождения { get; set; }
        [Required]
        [EmailAddress]
        public required string Почта { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Длина логина должна быть от 5 до 20 символов.")]
        public required string Логин { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Длина пароля должна быть от 5 до 20 символов.")]
        public required string Пароль { get; set; }
        public int Код_авто { get; set; }

       
     
    }
}
