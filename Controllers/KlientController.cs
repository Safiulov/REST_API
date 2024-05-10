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
        public async Task<IActionResult> Get()
        {
            // Создаем пустой список для хранения объектов Klients
            var result = new List<Klients>();
            // Устанавливаем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                // Создаем команду для выполнения SQL-запроса
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Klients\";", connection))
                {
                    // Выполняем команду и получаем объект для чтения данных
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Проходим по каждой строке результата
                        while (await reader.ReadAsync())
                        {
                            // Создаем новый объект Klients и заполняем его данными из текущей строки
                            var klients = new Klients
                            {
                                Код_клиента = await reader.GetFieldValueAsync<int>(0), // Получаем код клиента
                                ФИО = await reader.GetFieldValueAsync<string>(1),   // Получаем ФИО клиента
                                Дата_рождения = await reader.GetFieldValueAsync<DateTime>(2), // Получаем дату рождения клиента
                                Почта = await reader.GetFieldValueAsync<string>(3),   // Получаем почту клиента
                                Логин = await reader.GetFieldValueAsync<string>(4),   // Получаем логин клиента
                                Пароль = await reader.GetFieldValueAsync<string>(5),   // Получаем пароль клиента
                                Код_авто = await reader.GetFieldValueAsync<int>(6)    // Получаем код автомобиля клиента
                            };
                            // Добавляем полученного клиента в список результатов
                            result.Add(klients);
                        }
                    }
                }
            }
            // Возвращаем результат в формате JSON
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Klients klient)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Обновляем данные клиента в базе данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Klients\" SET \"ФИО\"=@ФИО, \"Код_авто\"=@Код_авто, \"Дата_рождения\"=@Дата_рождения, \"Почта\"=@Почта,\"Логин\"=@Логин WHERE \"Код_клиента\" = @id;", connection))
                {
                    // Добавляем параметры запроса
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("ФИО", klient.ФИО);
                    command.Parameters.AddWithValue("Дата_рождения", klient.Дата_рождения);
                    command.Parameters.AddWithValue("Почта", klient.Почта);
                    command.Parameters.AddWithValue("Логин", klient.Логин);
                    command.Parameters.AddWithValue("Код_авто", klient.Код_авто);
                    // Выполняем обновление данных
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    // Проверяем, был ли аффект от обновления
                    if (rowsAffected == 1)
                    {
                        return Ok(); // Если обновление прошло успешно, возвращаем код 200
                    }
                    else
                    {
                        return NotFound(); // Если клиента с указанным id не существует, возвращаем код 404
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
                        // Возвращаем ошибку, если клиент с таким логином уже существует
                        return BadRequest(new { error = "Клиент с таким логином уже существует" });
                    }
                }
            }
            // Хеширование пароля
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(klient.Пароль);
            // Добавление нового клиента в базу данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("INSERT INTO \"Стоянка\".\"Klients\"(\"ФИО\", \"Дата_рождения\", \"Почта\", \"Логин\", \"Пароль\",\"Код_авто\") VALUES (@ФИО, @Дата_рождения, @Почта, @Логин, @Пароль,@Код_авто);", connection))
                {
                    command.Parameters.AddWithValue("ФИО", klient.ФИО);
                    command.Parameters.AddWithValue("Дата_рождения", klient.Дата_рождения);
                    command.Parameters.AddWithValue("Почта", klient.Почта);
                    command.Parameters.AddWithValue("Логин", klient.Логин);
                    command.Parameters.AddWithValue("Пароль", hashedPassword); // Используем хешированный пароль
                    command.Parameters.AddWithValue("Код_авто", klient.Код_авто);
                    rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected == 1)
                    {
                        // Возвращаем созданного клиента, если он был успешно добавлен
                        return CreatedAtAction(nameof(Get), new { id = klient.Код_клиента }, klient);
                    }
                    else
                    {
                        // Возвращаем ошибку, если клиент не был добавлен
                        return BadRequest("Некорректно");
                    }
                }
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Создаем подключение к базе данных PostgreSQL
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Ожидаем, пока откроется соединение
                await connection.OpenAsync();
                try
                {
                    // Создаем команду для удаления клиента по его коду
                    await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Klients\" WHERE Код_клиента = @id;", connection))
                    {
                        // Добавляем параметр для идентификатора клиента
                        command.Parameters.AddWithValue("id", id);
                        // Выполняем команду
                        int affectedRows = await command.ExecuteNonQueryAsync();
                        if (affectedRows == 0)
                        {
                            // Если запись не найдена, возвращаем 404 Not Found
                            return NotFound();
                        }
                        // Возвращаем статус 200 OK
                        return Ok();
                    }
                }
                catch (NpgsqlException ex)
                {
                    // Возвращаем статус 500 Internal Server Error с описанием ошибки
                    return StatusCode(500, $"Error deleting auto: {ex.Message}");
                }
            }
        }




        [HttpDelete]
        [Route("Delete_All")]
        public async Task<IActionResult> DeleteAll()
        {
            // SQL-запрос на сброс последовательности Код_клиента в таблице Klients
            string query = "ALTER SEQUENCE \"Стоянка\".\"Klients_Code_klient_seq\" RESTART WITH 0";

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                try
                {
                    // Удаляем все записи из таблицы Klients
                    await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Klients\"", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    // Сбрасываем последовательность Код_клиента в таблице Klients
                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    // Возвращаем код ответа 200 (OK)
                    return Ok();
                }
                catch (Exception ex)
                {
                    // При ошибке возвращаем код ответа 500 (Internal Server Error) и сообщение об ошибке
                    return StatusCode(500, ex.Message);
                }
            }
        }
    }
}
