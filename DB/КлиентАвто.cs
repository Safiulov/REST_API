namespace WebApplication2.DB
{
    public class КлиентАвто
    {
       
        public string ФИО { get; set; }

        public DateTime Дата_рождения { get; set; }
        public string Почта { get; set; }

        public string Логин { get; set; }
        public string Пароль { get; set; }

        public int Код_авто { get; set; }

        public string Марка { get; set; }
        public string Цвет { get; set; }
        public string Тип { get; set; }
        public string Госномер { get; set; }
        public int Год { get; set; }
    }
}
