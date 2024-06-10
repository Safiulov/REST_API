using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DB
{
    public class Kvitance
    {
        public  string ФИО { get; set; }
        public int Код_авто { get; set; }
        
        public  DateTime Дата_рождения { get; set; }
      
        public  string Почта { get; set; }
       
        public  string Госномер { get; set; }
       
        public  string Марка { get; set; }
        public int? Стоимость { get; set; }
        public  DateTime? Дата_въезда{ get; set; }
        public DateTime? Дата_выезда { get; set; }
       
        public List<Invoice> Услуги { get; set; }
        public int Итого { get; set; }
    }
    public class Invoice
    {
        public required string Название { get; set; }
        public int Стоимость { get; set; }
    }

  

}
