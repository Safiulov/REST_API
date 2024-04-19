namespace WebApplication2.DB
{
    public class Tarifs
    {
        public int Код_тарифа { get; set; }
        public string Название { get; set; }
        public string Условие { get; set; }
        public string Время_действия { get; set; }
        public int? Стоимость { get; set; }
    }
}
