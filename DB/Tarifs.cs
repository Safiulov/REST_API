namespace WebApplication1.DB
{
    public class Tarifs
    {
        public int Код_тарифа { get; set; }
        public required string Название { get; set; }

        public required string Условие { get; set; }
        public required string Время_действия { get; set; }
        public int? Стоимость { get; set; }
    }
}
