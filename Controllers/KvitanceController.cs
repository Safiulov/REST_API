using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using WebApplication2.DB;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/Kvitance")]
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

                // Get client, auto, and stay information
                string sql = "SELECT a.\"ФИО\", a.\"Дата_рождения\", a.\"Почта\", b.\"Госномер\", b.\"Марка\", c.\"Дата_въезда\", c.\"Дата_выезда\", c.\"Стоимость\" " +
                             "FROM \"Стоянка\".\"Klients\" a " +
                             "JOIN \"Стоянка\".\"Sales\" c ON a.\"Код_клиента\" = c.\"Код_клиента\" " +
                             "JOIN \"Стоянка\".\"Auto\" b ON a.\"Код_авто\" = b.\"Код_авто\" " +
                             "WHERE a.\"Код_клиента\" = @clientCode";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("clientCode", clientCode);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
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
                            // Close the connection before creating a new command
                            connection.Close();

                            // If the client is not found in the "Sales" table, try to get the date of entry from the "Realisation" table
                            sql = "SELECT a.\"ФИО\", a.\"Дата_рождения\", a.\"Почта\", b.\"Госномер\", b.\"Марка\", c.\"Дата_въезда\",c.\"Стоимость\" " +
                             "FROM \"Стоянка\".\"Klients\" a " +
                             "JOIN \"Стоянка\".\"Realisation\" c ON a.\"Код_клиента\" = c.\"Код_клиента\" " +
                             "JOIN \"Стоянка\".\"Auto\" b ON a.\"Код_авто\" = b.\"Код_авто\" " +
                             "WHERE a.\"Код_клиента\" = @clientCode";
                            using (var command2 = new NpgsqlCommand(sql, connection))
                            {
                                connection.Open(); // Open the connection again
                                command2.Parameters.AddWithValue("clientCode", clientCode);

                                using (var reader2 = await command2.ExecuteReaderAsync())
                                {
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
                        }
                    }
                }

                // Get services information
                sql = "SELECT \"Название_услуги\", \"Сумма\" FROM \"Стоянка\".\"Realisation\" WHERE \"Код_клиента\" = @clientCode";
                using (var command3 = new NpgsqlCommand(sql, connection))
                {
                    connection.Close(); // Close the connection before creating a new command
                    connection.Open(); // Open the connection again
                    command3.Parameters.AddWithValue("clientCode", clientCode);

                    using (var reader3 = await command3.ExecuteReaderAsync())
                    {
                        kvitance.Услуги = new List<Invoice>();
                        while (await reader3.ReadAsync())
                        {
                            kvitance.Услуги.Add(new Invoice { Название = reader3.GetString(0), Стоимость = reader3.GetInt32(1) });
                            kvitance.Итого += reader3.GetInt32(1);
                        }
                    }
                }
            }

            if (!kvitance.Услуги.Any())
            {
                kvitance.Услуги = null;
            }

            return Ok(kvitance);
        }

        [HttpGet]
        [Route("Free")]
        public async Task<IActionResult> Free()
        {
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                connection.OpenAsync();

                string sql = "select a.\"Дата_въезда\",b.\"Марка\",b.\"Госномер\",c.\"ФИО\" from \"Стоянка\".\"Sales\" a join \"Стоянка\".\"Klients\" c on a.\"Код_клиента\"=c.\"Код_клиента\" join \"Стоянка\".\"Auto\" b on b.\"Код_авто\"=c.\"Код_авто\" where \"Дата_выезда\" is null";
                await using (var command = new NpgsqlCommand(sql, connection))
                {
                    var reports = new List<Report_of_free>();
                    await using (var reader = command.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            reports.Add(new Report_of_free
                            {
                                Дата_въезда = await reader.GetFieldValueAsync<DateTime>(0),
                                Марка = await reader.GetFieldValueAsync<string>(1),
                                Госномер = await reader.GetFieldValueAsync<string>(2),
                                ФИО = await reader.GetFieldValueAsync<string>(3) 
                            });
                        }
                    }
                    return Ok(reports);
                }
            }
        }


        [HttpGet]
        [Route("Period")]
        public async Task<IActionResult> Period(DateTime date_in, DateTime date_out)
        {
            List<Period> period = new List<Period>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                string sql = "SELECT r.\"Код_услуги\", s.\"Название\", r.\"Дата_въезда\", kl.\"ФИО\", kl.\"Почта\", r.\"Сумма\"\r\nFROM \"Стоянка\".\"Realisation\" r\r\nJOIN \"Стоянка\".\"Service\" s ON r.\"Код_услуги\" = s.\"Код_услуги\"\r\nJOIN \"Стоянка\".\"Klients\" kl ON r.\"Код_клиента\" = kl.\"Код_клиента\"\r\nWHERE r.\"Дата_въезда\" BETWEEN @date_in AND @date_out\r\nGROUP BY r.\"Код_услуги\", s.\"Название\", r.\"Дата_въезда\", kl.\"ФИО\", kl.\"Почта\",r.\"Сумма\"\r\nORDER BY r.\"Дата_въезда\";";
                await using(var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("date_in", date_in);
                    command.Parameters.AddWithValue("date_out", date_out);
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
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
                }
            }

            return Ok(period);
        }


[HttpGet]
[Route("Period_Sales")]
public async Task<IActionResult> Period_Sales(DateTime date_in, DateTime date_out)
{
    List<Period_Sales> sales = new List<Period_Sales>();

    await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
    {
        await connection.OpenAsync();

        string sql = "SELECT \"Дата_въезда\",\"Дата_выезда\",\"Тариф\",\"Время_стоянки\",\"Стоимость\" FROM \"Стоянка\".\"Sales\" WHERE (\"Дата_въезда\" >= @date_in AND \"Дата_выезда\" <= @date_out);";
        await using (var command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("date_in", date_in);
            command.Parameters.AddWithValue("date_out", date_out);

            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
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
        }
    }

    return Ok(sales);
}

    }
}
