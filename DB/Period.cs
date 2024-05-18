namespace WebApplication1.DB
{
    public class Period
    {
        public int Код_услуги { get; set; }
        public string Название { get; set; }
        public DateTime Дата_въезда { get; set; }   
        public string ФИО {  get; set; }
        public string Почта { get; set; } 
        public int Сумма { get; set; }
    }
}
