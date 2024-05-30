using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.DB;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/Kvitance")]
    [RequireHttps]
    public class KvitanceController : Controller
    {
        private readonly IConfiguration _databaseService;
        public KvitanceController(IConfiguration configuration)
        {
            _databaseService = configuration;
        }
        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> GetInvoice(int clientCode)
        {
            var kvitance = new Kvitance();
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                // Получаем информацию о клиенте, автомобиле и проживании
                string sql = "SELECT a.\"ФИО\", a.\"Дата_рождения\", a.\"Почта\", b.\"Госномер\", b.\"Марка\", c.\"Дата_въезда\", c.\"Дата_выезда\", c.\"Стоимость\" " +
                             "FROM \"Стоянка\".\"Klients\" a " +
                             "JOIN \"Стоянка\".\"Sales\" c ON a.\"Код_клиента\" = c.\"Код_клиента\" " +
                             "JOIN \"Стоянка\".\"Auto\" b ON a.\"Код_авто\" = b.\"Код_авто\" " +
                             "WHERE a.\"Код_клиента\" = @clientCode " +
                             "ORDER BY c.\"Дата_выезда\" DESC " +
                             "LIMIT 1";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("clientCode", clientCode);
                    using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        kvitance.ФИО = reader.GetFieldValue<string>(0);
                        kvitance.Дата_рождения = reader.GetFieldValue<DateTime>(1);
                        kvitance.Почта = reader.GetFieldValue<string>(2);
                        kvitance.Госномер = reader.GetFieldValue<string>(3);
                        kvitance.Марка = reader.GetFieldValue<string>(4);
                        kvitance.Дата_въезда = reader.GetFieldValue<DateTime>(5);
                        kvitance.Дата_выезда = reader.IsDBNull(6) ? null : reader.GetFieldValue<DateTime?>(6);
                        kvitance.Стоимость = reader.IsDBNull(7) ? null : reader.GetFieldValue<int?>(7);
                    }
                    else
                    {
                        // Закрываем соединение перед созданием новой команды
                        connection.Close();
                        // Если клиент не найден в таблице "Sales", пытаемся получить дату входа из таблицы "Realisation"
                        sql = "SELECT a.\"ФИО\", a.\"Дата_рождения\", a.\"Почта\", b.\"Госномер\", b.\"Марка\", c.\"Дата_въезда\",c.\"Стоимость\" " +
                         "FROM \"Стоянка\".\"Klients\" a " +
                         "JOIN \"Стоянка\".\"Realisation\" c ON a.\"Код_клиента\" = c.\"Код_клиента\" " +
                         "JOIN \"Стоянка\".\"Auto\" b ON a.\"Код_авто\" = b.\"Код_авто\" " +
                        "WHERE a.\"Код_клиента\" = @clientCode " +
                             "ORDER BY c.\"Дата_въезда\" DESC " +
                             "LIMIT 1";

                        using var command2 = new NpgsqlCommand(sql, connection);
                        connection.Open(); // Открываем соединение снова
                        command2.Parameters.AddWithValue("clientCode", clientCode);
                        using NpgsqlDataReader reader2 = await command2.ExecuteReaderAsync();
                        if (await reader2.ReadAsync())
                        {
                            kvitance.ФИО = reader2.GetFieldValue<string>(0);
                            kvitance.Дата_рождения = reader2.GetFieldValue<DateTime>(1);
                            kvitance.Почта = reader2.GetFieldValue<string>(2);
                            kvitance.Госномер = reader2.GetFieldValue<string>(3);
                            kvitance.Марка = reader2.GetFieldValue<string>(4);
                            kvitance.Дата_въезда = DateTime.Today;
                            kvitance.Дата_выезда = reader2.GetFieldValue<DateTime>(5);
                            kvitance.Стоимость = reader2.IsDBNull(6) ? null : reader2.GetFieldValue<int?>(6);
                        }
                        else
                        {
                            return NotFound("Клиент не найден.");
                        }
                    }
                }
                // Получаем информацию о услугах
                sql = "SELECT Название_услуги, Сумма FROM Стоянка.\"Realisation\" WHERE Код_клиента = @clientCode and Дата_въезда between (select Дата_въезда from Стоянка.\"Sales\" where Код_клиента = @clientCode  order by \"Дата_въезда\" desc  limit 1) and (select Дата_выезда from Стоянка.\"Sales\" where Код_клиента = @clientCode order by \"Дата_выезда\" desc limit 1)";
                using var command3 = new NpgsqlCommand(sql, connection);
                connection.Close(); // Закрываем соединение перед созданием новой команды
                connection.Open(); // Открываем соединение снова
                command3.Parameters.AddWithValue("clientCode", clientCode);
                using NpgsqlDataReader reader3 = await command3.ExecuteReaderAsync();
                kvitance.Услуги = new List<Invoice>();
                while (await reader3.ReadAsync())
                {
                    kvitance.Услуги.Add(new Invoice { Название = reader3.GetString(0), Стоимость = reader3.GetInt32(1) });
                    kvitance.Итого += reader3.GetInt32(1);
                }
                if (kvitance.Услуги.Count==0)
                {
                    sql = "SELECT Название_услуги, Сумма FROM \"Стоянка\".\"Realisation\" WHERE Код_клиента = @clientCode and (Дата_въезда >= (select Дата_въезда from Стоянка.\"Sales\" where Код_клиента = @clientCode  order by \"Дата_въезда\" desc  limit 1 ) or (Дата_въезда >= (SELECT (Дата_въезда - INTERVAL '1 month') AS Дата_въезда_минус_месяц FROM \"Стоянка\".\"Realisation\" WHERE Код_клиента =@clientCode ORDER BY Дата_въезда DESC LIMIT 1)))  and Дата_въезда between (SELECT (Дата_въезда - INTERVAL '1 month') AS Дата_въезда_минус_месяц FROM \"Стоянка\".\"Realisation\" WHERE Код_клиента =@clientCode ORDER BY Дата_въезда DESC LIMIT 1) and (select Дата_въезда from \"Стоянка\".\"Realisation\" where Код_клиента = @clientCode order by \"Дата_въезда\" desc limit 1)";
                    using var command4 = new NpgsqlCommand(sql, connection);
                    connection.Close(); // Закрываем соединение перед созданием новой команды
                    connection.Open(); // Открываем соединение снова
                    command4.Parameters.AddWithValue("clientCode", clientCode);
                    using NpgsqlDataReader reader4 = await command4.ExecuteReaderAsync();
                    kvitance.Услуги = new List<Invoice>();
                    while (await reader4.ReadAsync())
                    {
                        kvitance.Услуги.Add(new Invoice { Название = reader4.GetString(0), Стоимость = reader4.GetInt32(1) });
                        kvitance.Итого += reader4.GetInt32(1);
                    }
                }
            }
                       
            return Ok(kvitance);
        }

        [HttpGet]
        [Route("Free")]
        public async Task<IActionResult> Free()
        {
            // Создаем подключение к базе данных PostgreSQL
            await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();
            // Создаем SQL-запрос для получения свободных мест на парковке
            string sql = "select a.\"Дата_въезда\",a.\"Место\",b.\"Марка\",b.\"Госномер\",c.\"ФИО\" " +
                         "from \"Стоянка\".\"Sales\" a " +
                         "join \"Стоянка\".\"Klients\" c on a.\"Код_клиента\"=c.\"Код_клиента\" " +
                         "join \"Стоянка\".\"Auto\" b on b.\"Код_авто\"=c.\"Код_авто\" " +
                         "where \"Дата_выезда\" is null";
            // Выполняем запрос и читаем данные из результирующего набора
            await using var command = new NpgsqlCommand(sql, connection);
            var reports = new List<Report_of_free>();
            await using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                while (await reader.ReadAsync())
                {
                    // Создаем объекты Report_of_free и добавляем их в список
                    reports.Add(new Report_of_free
                    {
                        Дата_въезда = await reader.GetFieldValueAsync<DateTime>(0),
                        Место = await reader.GetFieldValueAsync<String>(1),
                        Марка = await reader.GetFieldValueAsync<string>(2),
                        Госномер = await reader.GetFieldValueAsync<string>(3),
                        ФИО = await reader.GetFieldValueAsync<string>(4)
                    });
                }
            }
            // Возвращаем код ответа 200 (OK) и список отчетов о свободных местах на парковке
            return Ok(reports);
        }


        [HttpGet]
        [Route("FreeBron")]
        public async Task<IActionResult> FreeBron()
        {
            // Создаем подключение к базе данных PostgreSQL
            await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();
            // Создаем SQL-запрос для получения свободных мест на парковке
            string sql = "select a.\"Дата_въезда\" as \"Дата_выезда\" ,a.\"Место\",b.\"Марка\",b.\"Госномер\",c.\"ФИО\" from \"Стоянка\".\"Realisation\" a join \"Стоянка\".\"Klients\" c on a.\"Код_клиента\"=c.\"Код_клиента\" join \"Стоянка\".\"Auto\" b on b.\"Код_авто\"=c.\"Код_авто\" where \"Дата_въезда\" > now()";
            // Выполняем запрос и читаем данные из результирующего набора
            await using var command = new NpgsqlCommand(sql, connection);
            var reports = new List<Report_of_free>();
            await using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                while (await reader.ReadAsync())
                {
                    // Создаем объекты Report_of_free и добавляем их в список
                    reports.Add(new Report_of_free
                    {
                        Дата_въезда = await reader.GetFieldValueAsync<DateTime>(0),
                        Место = await reader.GetFieldValueAsync<String>(1),
                        Марка = await reader.GetFieldValueAsync<string>(2),
                        Госномер = await reader.GetFieldValueAsync<string>(3),
                        ФИО = await reader.GetFieldValueAsync<string>(4)
                    });
                }
            }
            // Возвращаем код ответа 200 (OK) и список отчетов о свободных местах на парковке
            return Ok(reports);
        }

        [HttpGet]
        [Route("Period")]
        public async Task<IActionResult> Period(DateTime date_in, DateTime date_out)
        {
            // Создаем список для хранения отчета за период
            List<Period> period = new();
            // Создаем подключение к базе данных PostgreSQL
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Открываем соединение
                await connection.OpenAsync();
                // Создаем SQL-запрос для получения отчета за период
                string sql = "SELECT r.\"Код_услуги\", s.\"Название\", r.\"Дата_въезда\", kl.\"ФИО\", kl.\"Почта\", r.\"Сумма\"\r\nFROM \"Стоянка\".\"Realisation\" r\r\nJOIN \"Стоянка\".\"Service\" s ON r.\"Код_услуги\" = s.\"Код_услуги\"\r\nJOIN \"Стоянка\".\"Klients\" kl ON r.\"Код_клиента\" = kl.\"Код_клиента\"\r\nWHERE r.\"Дата_въезда\" BETWEEN @date_in AND @date_out\r\nGROUP BY r.\"Код_услуги\", s.\"Название\", r.\"Дата_въезда\", kl.\"ФИО\", kl.\"Почта\",r.\"Сумма\"\r\nORDER BY r.\"Дата_въезда\";";
                // Создаем команду для выполнения SQL-запроса
                await using var command = new NpgsqlCommand(sql, connection);
                // Добавляем параметры для указания диапазона дат
                command.Parameters.AddWithValue("date_in", date_in);
                command.Parameters.AddWithValue("date_out", date_out);
                // Выполняем SQL-запрос и читаем данные из результирующего набора
                await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    // Создаем объекты Period и добавляем их в список
                    var report = new Period
                    {
                        Код_услуги = await reader.GetFieldValueAsync<int>(0),
                        Название = await reader.GetFieldValueAsync<string>(1),
                        Дата_въезда = await reader.GetFieldValueAsync<DateTime>(2),
                        ФИО = await reader.GetFieldValueAsync<string>(3),
                        Почта = await reader.GetFieldValueAsync<string>(4),
                        Сумма = await reader.GetFieldValueAsync<int>(5)
                    };
                    period.Add(report);
                }
            }
            // Возвращаем код ответа 200 (OK) и список отчетов за период
            return Ok(period);
        }

        [HttpGet]
        [Route("Period_Sales")]
        public async Task<IActionResult> Period_Sales(DateTime date_in, DateTime date_out)
        {
            // Создаем список для хранения отчета о продажах за период
            List<Period_Sales> sales = new();
            // Создаем подключение к базе данных PostgreSQL
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Открываем соединение
                await connection.OpenAsync();
                // Создаем SQL-запрос для получения отчета о продажах за период
                string sql = "SELECT \"Дата_въезда\",\"Дата_выезда\",\"Тариф\",\"Время_стоянки\",\"Стоимость\" FROM \"Стоянка\".\"Sales\" WHERE (\"Дата_въезда\" >= @date_in AND \"Дата_выезда\" <= @date_out);";
                // Создаем команду для выполнения SQL-запроса
                await using var command = new NpgsqlCommand(sql, connection);
                // Добавляем параметры для указания диапазона дат
                command.Parameters.AddWithValue("date_in", date_in);
                command.Parameters.AddWithValue("date_out", date_out);
                // Выполняем SQL-запрос и читаем данные из результирующего набора
                await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    // Создаем объекты Period_Sales и добавляем их в список
                    var report = new Period_Sales
                    {
                        Дата_въезда = await reader.GetFieldValueAsync<DateTime>(0),
                        Дата_выезда = reader.IsDBNull(1) ? null : await reader.GetFieldValueAsync<DateTime>(1),
                        Тариф = reader.IsDBNull(2) ? null : await reader.GetFieldValueAsync<int>(2),
                        Время_стоянки = reader.IsDBNull(3) ? null : await reader.GetFieldValueAsync<int>(3),
                        Стоимость = reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<int>(4),
                    };
                    sales.Add(report);
                }
            }
            // Возвращаем код ответа 200 (OK) и список отчетов о продажах за период
            return Ok(sales);
        }
    }
}