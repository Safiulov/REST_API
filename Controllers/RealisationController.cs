using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using System.Security.Cryptography;
using WebApplication2.DB;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/Realisation")]
    public class RealisationController : Controller
    {
        private readonly IConfiguration _databaseService;
        public RealisationController(IConfiguration configuration)
        {
            _databaseService = configuration;
        }

        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Get(string columnName, string columnValue)
        {
            // Проверяем, что параметры для поиска не пустые
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(columnValue))
            {
                return BadRequest("Не указаны параметры для поиска");
            }
            // Создаем список для хранения результатов поиска
            var result = new List<Realisation>();
            // Создаем подключение к базе данных PostgreSQL
            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Открываем соединение
                await connection.OpenAsync();
                // Создаем SQL-запрос для поиска записей в таблице Realisation
                string sql = $"SELECT * from \"Стоянка\".\"Realisation\" WHERE cast({columnName} as text) ilike '%{columnValue}%';";
                // Создаем команду для выполнения SQL-запроса
                await using (var command = new NpgsqlCommand(sql, connection))
                {
                    // Выполняем SQL-запрос и читаем данные из результирующего набора
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Создаем объекты Realisation и добавляем их в список
                            var realisation = new Realisation
                            {
                                Код = await reader.GetFieldValueAsync<int>(0),
                                Дата_въезда = await reader.GetFieldValueAsync<DateTime>(1),
                                Место = await reader.GetFieldValueAsync<string>(2),
                                Код_услуги = await reader.GetFieldValueAsync<int>(3),
                                Название_услуги = reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<string>(4),
                                Код_клиента = await reader.GetFieldValueAsync<int>(5),
                                ФИО = reader.IsDBNull(6) ? null : await reader.GetFieldValueAsync<string>(6),
                                Госномер = reader.IsDBNull(7) ? null : await reader.GetFieldValueAsync<string>(7),
                                Стоимость = reader.IsDBNull(8) ? null : await reader.GetFieldValueAsync<int>(8),
                                Сумма = reader.IsDBNull(9) ? null : await reader.GetFieldValueAsync<int>(9)
                            };

                            result.Add(realisation);
                        }
                    }
                }
            }
            // Возвращаем код ответа 200 (OK) и список результатов поиска
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Создаем список для хранения результатов запроса
            var result = new List<Realisation>();

            // Создаем подключение к базе данных PostgreSQL
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Открываем соединение
                await connection.OpenAsync();

                // Создаем SQL-запрос для получения всех записей из таблицы Realisation
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Realisation\";", connection))
                {
                    // Выполняем SQL-запрос и читаем данные из результирующего набора
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Создаем объекты Realisation и добавляем их в список
                            var realisation = new Realisation
                            {
                                Код = await reader.GetFieldValueAsync<int>(0),
                                Дата_въезда = await reader.GetFieldValueAsync<DateTime>(1),
                                Место = await reader.GetFieldValueAsync<string>(2),
                                Код_услуги = await reader.GetFieldValueAsync<int>(3),
                                Название_услуги = reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<string>(4),
                                Код_клиента = await reader.GetFieldValueAsync<int>(5),
                                ФИО = reader.IsDBNull(6) ? null : await reader.GetFieldValueAsync<string>(6),
                                Госномер = reader.IsDBNull(7) ? null : await reader.GetFieldValueAsync<string>(7),
                                Стоимость = reader.IsDBNull(8) ? null : await reader.GetFieldValueAsync<int>(8),
                                Сумма = reader.IsDBNull(9) ? null : await reader.GetFieldValueAsync<int>(9)
                            };

                            result.Add(realisation);
                            int daysOverdue = (DateTime.Now - realisation.Дата_въезда).Days;

                            if (realisation.Дата_въезда < DateTime.Now)
                {
                                realisation.Место = $"Просрочено место {realisation.Место} на {daysOverdue} дней";
                            }
                        }
                    }
                }
            }

            // Возвращаем код ответа 200 (OK) и список результатов запроса
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Realisation realisation)
        {
            // Проверяем, что модель данных валидна
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Создаем подключение к базе данных PostgreSQL
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Открываем соединение
                await connection.OpenAsync();
                // Создаем SQL-запрос для обновления записи в таблице Realisation
                await using (var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Realisation\" SET  \"Дата_въезда\"=@Дата_въезда, \"Место\"=@Место, \"Код_услуги\"=@Код_услуги, \"Код_клиента\"=@Код_клиента WHERE \"Код\" = @id;", connection))
                {
                    // Добавляем параметры для запроса
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("Дата_въезда", realisation.Дата_въезда);
                    command.Parameters.AddWithValue("Место", realisation.Место);
                    command.Parameters.AddWithValue("Код_услуги", realisation.Код_услуги);
                    command.Parameters.AddWithValue("Код_клиента", realisation.Код_клиента);
                    // Выполняем SQL-запрос и получаем количество затронутых строк
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    // Если запись успешно обновлена, возвращаем код ответа 200 (OK)
                    if (rowsAffected == 1)
                    {
                        return Ok();
                    }
                    // Если запись не найдена, возвращаем код ответа 404 (Not Found)
                    else
                    {
                        return NotFound();
                    }
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Realisation realisation)
        {
            // Проверяем, что модель данных валидна
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Проверяем дополнительные условия для места и кода услуги
            if (realisation.Место.StartsWith("A") && (realisation.Код_услуги == 1 || realisation.Код_услуги == 2))
            {
                ModelState.AddModelError(string.Empty, "Ошибка: место начинается с 'A' и код_услуги равен 1 или 2");
                return BadRequest(ModelState);
            }
            // Создаем подключение к базе данных PostgreSQLz
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Открываем соединение
                await connection.OpenAsync();


                // Проверяем, занято ли указанное место другим автомобилем на указанное время
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Realisation\" WHERE \"Место\" = @Место", connection))
                {
                    command.Parameters.AddWithValue("Место", realisation.Место);

                    var existingRealisation = await command.ExecuteScalarAsync();
                    if (existingRealisation != null)
                    {
                        ModelState.AddModelError(string.Empty, "Данное место уже занято");
                        return BadRequest(ModelState);
                    }
                }

                


                    // Создаем SQL-запрос для вставки записи в таблицу Realisation
                    await using (var command = new NpgsqlCommand("INSERT INTO \"Стоянка\".\"Realisation\"(\"Дата_въезда\", \"Место\",  \"Код_услуги\", \"Код_клиента\") VALUES (@Дата_въезда, @Место, @Код_услуги, @Код_клиента);", connection))
                {
                    // Добавляем параметры для запроса
                    command.Parameters.AddWithValue("Дата_въезда", realisation.Дата_въезда.ToUniversalTime());
                    command.Parameters.AddWithValue("Место", realisation.Место);
                    command.Parameters.AddWithValue("Код_услуги", realisation.Код_услуги);
                    command.Parameters.AddWithValue("Код_клиента", realisation.Код_клиента);
                    // Выполняем SQL-запрос и получаем количество затронутых строк
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    // Если запись успешно вставлена, возвращаем код ответа 200 (OK)
                    if (rowsAffected == 1)
                    {
                        return Ok();
                    }
                    // Если запись не удалось вставить, возвращаем код ответа 400 (Bad Request)
                    else
                    {
                        return BadRequest(ModelState);
                    }
                }
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Создаем подключение к базе данных PostgreSQL
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Открываем соединение
                await connection.OpenAsync();

                // Создаем SQL-запрос для удаления записи из таблицы Realisation
                await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Realisation\" WHERE \"Код\" = @id;", connection))
                {
                    // Добавляем параметры для запроса
                    command.Parameters.AddWithValue("id", id);
                    // Выполняем SQL-запрос и получаем количество затронутых строк
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    // Если запись успешно удалена, возвращаем код ответа 200 (OK)
                    if (rowsAffected == 1)
                    {
                        return Ok();
                    }
                    // Если запись не найдена, возвращаем код ответа 404 (Not Found)
                    else
                    {
                        return NotFound();
                    }
                }
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            // Создаем SQL-запрос для сброса последовательности в таблице Realisation
            string query = "ALTER SEQUENCE \"Стоянка\".\"Realisation_Code_seq\" RESTART WITH 0";

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Открываем соединение
                await connection.OpenAsync();
                {
                    try
                    {
                        // Создаем SQL-запрос для удаления всех записей из таблицы Realisation
                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Realisation\"", connection))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        // Создаем SQL-запрос для сброса последовательности в таблице Realisation
                        await using (var command = new NpgsqlCommand(query, connection))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        // Возвращаем код ответа 200 (OK)
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        // Возвращаем код ответа 500 (Internal Server Error) и сообщение об ошибке
                        return StatusCode(500, ex.Message);
                    }
                }
            }
        }
    }
}
