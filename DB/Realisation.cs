using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class Realisation
    {
        public int Код { get; set; }
        public required DateTime Дата_въезда { get; set; }
        public required string Место { get; set; }
        public required int Код_услуги { get; set; }
        public string? Название_услуги { get; set; }
        public required int Код_клиента { get; set; }
        public required string ФИО { get; set; }
        
        public required string Госномер { get; set; }
        public int? Стоимость { get; set; }
        public int? Сумма { get; set; }

    }
    
}
