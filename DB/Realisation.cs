namespace WebApplication2.DB
{
    public class Realisation
    {
        public int Код { get; set; }
        public DateTime Дата_въезда { get; set; }
        public string Место { get; set; }
        public int Код_услуги { get; set; }
        public string? Название_услуги { get; set; }
        public int Код_клиента { get; set; }
        public string? ФИО { get; set; }
        public string? Госномер { get; set; }
        public int? Стоимость { get; set; }
        public int? Сумма { get; set; }

    }
    
}
