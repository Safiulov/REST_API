using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;

using System.Data;
using WebApplication2.DB;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/Klients")]
    public class KlientController : Controller
    {
        private readonly IConfiguration _databaseService;

        public KlientController(IConfiguration configuration)
        {
            _databaseService = configuration;
        }



        // Аннотация, указывающая, что этот метод отвечает на HTTP-запрос GET
        [HttpGet]
        // Аннотация, указывающая маршрут для этого метода
        [Route("Search")]
        // Асинхронный метод, который возвращает результат IActionResult
        public async Task<IActionResult> Get(string columnName, string columnValue)
        {
            // Проверка, что параметры для поиска не пустые
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(columnValue))
            {
                // Возвращает ответ BadRequest с сообщением об ошибке
                return BadRequest("Не указаны параметры для поиска");
            }

            // Создание пустого списка для хранения результатов поиска
            var result = new List<Klients>();

            // Создание и открытие соединения с базой данных PostgreSQL
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                // Создание и выполнение SQL-запроса для поиска по указанному столбцу
                await using (var command = new NpgsqlCommand($"SELECT * from \"Стоянка\".\"Klients\" WHERE cast({columnName} as text) ILIKE @columnValue;", connection))
                {
                    // Добавление параметра для значения столбца в запрос
                    command.Parameters.AddWithValue("@columnValue", $"%{columnValue}%");

                    // Выполнение запроса и получение результатов
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Чтение результатов запроса
                        while (await reader.ReadAsync())
                        {
                            // Создание нового экземпляра класса Klients и заполнение его данными из результата запроса
                            var klients = new Klients
                            {
                                Код_клиента = await reader.GetFieldValueAsync<int>(0),
                                ФИО = await reader.GetFieldValueAsync<string>(1),
                                Дата_рождения = await reader.GetFieldValueAsync<DateTime>(2),
                                Почта = await reader.GetFieldValueAsync<string>(3),
                                Логин = await reader.GetFieldValueAsync<string>(4),
                                Пароль = await reader.GetFieldValueAsync<string>(5),
                                Код_авто = await reader.GetFieldValueAsync<int>(6)
                            };

                            // Добавление найденного клиента в список результатов
                            result.Add(klients);
                        }
                    }
                }
            }

            // Возвращает ответ OK с результатами поиска
            return Ok(result);
        }

        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> Get()
        {
            var result = new List<Klients>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Klients\";", connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var klients = new Klients
                            {
                                Код_клиента = await reader.GetFieldValueAsync<int>(0),
                                ФИО = await reader.GetFieldValueAsync<string>(1),
                                Дата_рождения = await reader.GetFieldValueAsync<DateTime>(2),
                                Почта = await reader.GetFieldValueAsync<string>(3),
                                Логин = await reader.GetFieldValueAsync<string>(4),
                                Пароль = await reader.GetFieldValueAsync<string>(5),
                                Код_авто = await reader.GetFieldValueAsync<int>(6)
                            };

                            result.Add(klients);
                        }
                    }
                }
            }

            return Ok(result);
        }






        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Klients klient)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Klients\" SET \"ФИО\"=@ФИО, \"Код_авто\"=@Код_авто, \"Дата_рождения\"=@Дата_рождения, \"Почта\"=@Почта,\"Логин\"=@Логин,\"Пароль\"=@Пароль WHERE \"Код_клиента\" = @id;", connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("ФИО", klient.ФИО);
                    command.Parameters.AddWithValue("Дата_рождения", klient.Дата_рождения);
                    command.Parameters.AddWithValue("Почта", klient.Почта);
                    command.Parameters.AddWithValue("Логин", klient.Логин);
                    command.Parameters.AddWithValue("Пароль", klient.Пароль); // Использование хешированного пароля
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 1)
                    {
                        return Ok();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Klients klient)
        {
            int rowsAffected = 0;

            // Проверка на существование клиента с таким же логином
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @Логин;", connection))
                {
                    command.Parameters.AddWithValue("Логин", klient.Логин);
                    var count = (long)await command.ExecuteScalarAsync();

                    if (count > 0)
                    {
                        return BadRequest(new { error = "Клиент с таким логином уже существует" });
                    }
                }
            }

            // Хеширование пароля

            // Добавление нового клиента в базу данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(klient.Пароль);
                await using (var command = new NpgsqlCommand("INSERT INTO \"Стоянка\".\"Klients\"(\"ФИО\", \"Дата_рождения\", \"Почта\", \"Логин\", \"Пароль\",\"Код_авто\") VALUES (@ФИО, @Дата_рождения, @Почта, @Логин, @Пароль,@Код_авто);", connection))
                {
                    command.Parameters.AddWithValue("ФИО", klient.ФИО);
                    command.Parameters.AddWithValue("Дата_рождения", klient.Дата_рождения);
                    command.Parameters.AddWithValue("Почта", klient.Почта);
                    command.Parameters.AddWithValue("Логин", klient.Логин);
                    command.Parameters.AddWithValue("Пароль", hashedPassword); // Использование хешированного пароля
                    command.Parameters.AddWithValue("Код_авто", klient.Код_авто);
                    rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 1)
                    {
                        return CreatedAtAction(nameof(Get), new { id = klient.Код_клиента }, klient);
                    }
                    else
                    {
                        return BadRequest("Некорректно");
                    }
                }
            }

            return BadRequest("Некорректно");
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Sales\" WHERE Код_клиента = @id;", connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", id);
                            await command.ExecuteNonQueryAsync();
                        }

                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Realisation\" WHERE Код_клиента = @id;", connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", id);
                            await command.ExecuteNonQueryAsync();
                        }

                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Klients\" WHERE Код_клиента = @id;", connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", id);
                            await command.ExecuteNonQueryAsync();
                        }
                        transaction.Commit();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, ex.Message);
                    }
                }
            }
        }




        [HttpDelete]
        [Route("Delete_All")]
        public async Task<IActionResult> DeleteAll()
        {
            string query = "ALTER SEQUENCE \"Стоянка\".\"Klients_Code_klient_seq\" RESTART WITH 0";
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Sales\" ", connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Realisation\" ", connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Klients\"", connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        await using (var command = new NpgsqlCommand(query, connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        transaction.Commit();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, ex.Message);
                    }
                }
            }
        }
    }
}
