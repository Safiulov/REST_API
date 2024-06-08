namespace WebApplication1.DB
{
    public class Period_Sales
    {
        public required DateTime Дата_въезда { get; set; }
        public DateTime? Дата_выезда { get; set; }
        public int? Тариф { get; set; }
        public int? Время_стоянки { get; set; }
        public int? Стоимость { get; set; }
    }
}
