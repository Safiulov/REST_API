namespace WebApplication1.DB
{
    public class Service
    {
        public int Код_услуги { get; set; }
        public required string Название { get; set; }
        public required string Описание { get; set; }
        public required string Оплата { get; set; }
        public int Стоимость { get; set; }
    }
}
