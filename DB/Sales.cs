using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class Sales
    {
        public int Код { get; set; }
        public required DateTime Дата_въезда { get; set; }
        public DateTime? Дата_выезда { get; set; }
        public int? Тариф { get; set; }
        public int? Время_стоянки{ get; set; }
        public int? Стоимость { get; set; }
        public required string Место { get; set; }
        public required int Код_клиента{ get; set; }
        public required string ФИО { get; set; }
       
        public required string Госномер{ get; set; }
    }
}
