using Dapper;
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
                string sql = "SELECT * FROM \"Стоянка\".\"Select_Sales\" AS sales WHERE Код_клиента = @clientCode AND " +
                    "(NOT EXISTS (SELECT 1 FROM \"Стоянка\".\"Select_Realisation\" AS realisation WHERE realisation.Код_клиента = sales.Код_клиента AND Место LIKE 'B%')" +
                    "OR (Дата_выезда >= (SELECT Дата_въезда FROM \"Стоянка\".\"Select_Realisation\" AS realisation WHERE realisation.Код_клиента = sales.Код_клиента " +
                    "AND Место LIKE 'B%' ORDER BY Дата_въезда DESC LIMIT 1)))ORDER BY Дата_выезда DESC LIMIT 1;";
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("clientCode", clientCode);
                using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    kvitance.ФИО = reader.GetFieldValue<string>(1);
                    kvitance.Дата_рождения = reader.GetFieldValue<DateTime>(2);
                    kvitance.Почта = reader.GetFieldValue<string>(3);
                    kvitance.Госномер = reader.GetFieldValue<string>(4);
                    kvitance.Марка = reader.GetFieldValue<string>(5);
                    kvitance.Дата_въезда = reader.GetFieldValue<DateTime>(6);
                    kvitance.Дата_выезда = reader.IsDBNull(7) ? null : reader.GetFieldValue<DateTime?>(7);
                    kvitance.Стоимость = reader.IsDBNull(8) ? null : reader.GetFieldValue<int?>(8);
                    connection.Close();
                    connection.Open();
                    sql = "WITH LastDates AS (SELECT Дата_въезда, Дата_выезда FROM Стоянка.\"Sales\" WHERE Код_клиента = @clientCode ORDER BY Дата_въезда DESC, Дата_выезда DESC LIMIT 1)" +
                        "SELECT Название_услуги, Сумма FROM Стоянка.\"Realisation\" WHERE Код_клиента = @clientCode AND Место LIKE 'A%' " +
                        "AND Дата_въезда BETWEEN (SELECT Дата_въезда FROM LastDates) AND (SELECT Дата_выезда FROM LastDates);";
                    using var command3 = new NpgsqlCommand(sql, connection);

                    command3.Parameters.AddWithValue("clientCode", clientCode);
                    using NpgsqlDataReader reader3 = await command3.ExecuteReaderAsync();
                    kvitance.Услуги = new List<Invoice>();
                    while (await reader3.ReadAsync())
                    {
                        kvitance.Услуги.Add(new Invoice { Название = reader3.GetString(0), Стоимость = reader3.GetInt32(1) });
                        kvitance.Итого += reader3.GetInt32(1);
                    }

                }
                else
                {
                    // Закрываем соединение перед созданием новой команды
                    connection.Close();
                    // Если клиент не найден в таблице "Sales", пытаемся получить дату входа из таблицы "Realisation"
                    sql = "SELECT * FROM \"Стоянка\".\"Select_Realisation\" where Код_клиента = @clientCode   ORDER BY \"Дата_въезда\" DESC\r\n LIMIT 1;";
                    using var command2 = new NpgsqlCommand(sql, connection);
                    connection.Open(); // Открываем соединение снова
                    command2.Parameters.AddWithValue("clientCode", clientCode);
                    using NpgsqlDataReader reader2 = await command2.ExecuteReaderAsync();
                    if (await reader2.ReadAsync())
                    {
                        kvitance.ФИО = reader2.GetFieldValue<string>(1);
                        kvitance.Дата_рождения = reader2.GetFieldValue<DateTime>(2);
                        kvitance.Почта = reader2.GetFieldValue<string>(3);
                        kvitance.Госномер = reader2.GetFieldValue<string>(4);
                        kvitance.Марка = reader2.GetFieldValue<string>(5);
                        kvitance.Дата_выезда = reader2.GetFieldValue<DateTime>(6);

                        // Добавляем проверку кода услуги
                        int serviceCode = Convert.ToInt32(reader2.GetFieldValue<int>(9)); // Предполагается, что код услуги находится в 8-м столбце
                        if (serviceCode == 1)
                        {
                            kvitance.Дата_въезда = kvitance.Дата_выезда.HasValue ? kvitance.Дата_выезда.Value.AddMonths(-1) : (DateTime?)null;
                        }
                        else if (serviceCode == 2)
                        {
                            kvitance.Дата_въезда = DateTime.Now;
                        }

                        kvitance.Стоимость = reader2.IsDBNull(7) ? null : reader2.GetFieldValue<int?>(7);
                    connection.Close();
                    connection.Open();

                        // Получаем информацию о услугах
                        if (serviceCode == 1)
                        {
                            kvitance.Дата_въезда = kvitance.Дата_выезда.HasValue ? kvitance.Дата_выезда.Value.AddMonths(-1) : (DateTime?)null;
                           sql= "SELECT Название_услуги, Сумма FROM \"Стоянка\".\"Realisation\" " +
                                           "WHERE Код_клиента = @clientCode and Место like 'B%'  and" +
                                           "(Дата_въезда >= (SELECT (Дата_въезда - INTERVAL '1 month') AS Дата_въезда_минус_месяц FROM \"Стоянка\".\"Realisation\" WHERE Код_клиента = @clientCode  ORDER BY Дата_въезда DESC LIMIT 1)) ";
                        }
                        else if (serviceCode == 2)
                        {
                            kvitance.Дата_въезда = DateTime.Now;
                           sql = "SELECT Название_услуги, Сумма FROM \"Стоянка\".\"Realisation\" " +
                                           "WHERE Код_клиента = @clientCode and Место like 'B%'  and" +
                                           "(Дата_въезда >= (SELECT (Дата_въезда - INTERVAL '1 year') AS Дата_въезда_минус_год FROM \"Стоянка\".\"Realisation\" WHERE Код_клиента = @clientCode  ORDER BY Дата_въезда DESC LIMIT 1)) ";
                        }
                        else
                        {
                           sql = "SELECT Название_услуги, Сумма FROM \"Стоянка\".\"Realisation\" " +
                                           "WHERE Код_клиента = @clientCode and Место like 'B%' ORDER BY Дата_въезда DESC LIMIT 1)) ";
                        }
                        using var command4 = new NpgsqlCommand(sql, connection);
                        command4.Parameters.AddWithValue("clientCode", clientCode);
                        using NpgsqlDataReader reader4 = await command4.ExecuteReaderAsync();
                        kvitance.Услуги = new List<Invoice>();
                        while (await reader4.ReadAsync())
                        {
                            kvitance.Услуги.Add(new Invoice { Название = reader4.GetString(0), Стоимость = reader4.GetInt32(1) });
                            kvitance.Итого += reader4.GetInt32(1);
                        }
                    }
                    else
                    {
                        return NotFound("Клиент не найден.");
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
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();
            // Создаем SQL-запрос для получения свободных мест на парковке
            string sql = "SELECT * FROM \"Стоянка\".\"Resreve_Spaces\"";
            // Выполняем запрос и получаем результаты в виде списка объектов Report_of_free
            var reports = await connection.QueryAsync<Report_of_free>(sql);
            // Возвращаем код ответа 200 (OK) и список отчетов о свободных местах на парковке
            return Ok(reports);
        }


        [HttpGet]
        [Route("FreeBron")]
        public async Task<IActionResult> FreeBron()
        {
            // Создаем подключение к базе данных PostgreSQL
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();

            // Создаем SQL-запрос для получения свободных мест на парковке
            string sql = "SELECT * FROM \"Стоянка\".\"Bron_Spaces\"";

            // Выполняем запрос и читаем данные в список объектов Report_of_free
            var reports = await connection.QueryAsync<Report_of_free>(sql);

            // Возвращаем код ответа 200 (OK) и список отчетов о свободных местах на парковке
            return Ok(reports);
        }

        [HttpGet]
        [Route("Period")]
        public async Task<IActionResult> Period(DateTime date_in, DateTime date_out)
        {
            // Создаем подключение к базе данных PostgreSQL
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();

            // Создаем SQL-запрос для получения отчета за период
            string sql = @"
            SELECT r.""Код_услуги"", s.""Название"", r.""Дата_въезда"", kl.""ФИО"", kl.""Почта"", r.""Сумма""
            FROM ""Стоянка"".""Realisation"" r
            JOIN ""Стоянка"".""Service"" s ON r.""Код_услуги"" = s.""Код_услуги""
            JOIN ""Стоянка"".""Klients"" kl ON r.""Код_клиента"" = kl.""Код_клиента""
            WHERE r.""Дата_въезда"" >= @date_in AND r.""Дата_въезда"" <= @date_out";

            // Выполняем запрос и получаем список отчетов за период
            var period = await connection.QueryAsync<Period>(sql, new { date_in, date_out });

            // Возвращаем код ответа 200 (OK) и список отчетов за период
            return Ok(period);
        }

        [HttpGet]
        [Route("Period_Sales")]
        public async Task<IActionResult> Period_Sales(DateTime date_in, DateTime date_out)
        {
            // Создаем подключение к базе данных PostgreSQL
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();

            // Создаем SQL-запрос для получения отчета о продажах за период
            string sql = @"
            SELECT ""Дата_въезда"", ""Дата_выезда"", ""Тариф"", ""Время_стоянки"", ""Стоимость""
            FROM ""Стоянка"".""Sales""
            WHERE ""Дата_въезда"" >= @date_in AND ""Дата_выезда"" <= @date_out;";

            // Выполняем запрос и получаем список отчетов о продажах за период
            var sales = await connection.QueryAsync<Period_Sales>(sql, new { date_in, date_out });

            // Возвращаем код ответа 200 (OK) и список отчетов о продажах за период
            return Ok(sales);
        }
    }
}