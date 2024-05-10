// Импортируем необходимые библиотеки
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using WebApplication2.DB;

// Определяем пространство имен и класс контроллера
namespace WebApplication2.Controllers
{
    // Указываем, что это контроллер API и задаем маршрут для него
    [ApiController]
    [Route("api/Auto")]
    public class AutoController : Controller
    {
        // Поле для хранения конфигурации подключения к базе данных
        private readonly IConfiguration _databaseService;
        // Конструктор контроллера, принимающий конфигурацию подключения к базе данных
        public AutoController(IConfiguration configuration)
        {
            _databaseService = configuration;
        }
        // Метод для получения автомобилей по заданным параметрам
        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Get(string columnName, string columnValue)
        {
            // Проверяем, что переданы оба параметра
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(columnValue))
            {
                return BadRequest("Не указаны параметры для поиска");
            }
            // Создаем пустой список для хранения результатов
            var result = new List<Auto>();
            // Открываем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                // Создаем команду для запроса автомобилей по заданным параметрам
                var commandText = $"SELECT * FROM \"Стоянка\".\"Auto\" WHERE cast({columnName} as text) ilike @columnValue;";
                await using (var command = new NpgsqlCommand(commandText, connection))
                {
                    // Добавляем параметр в команду
                    command.Parameters.Add(new NpgsqlParameter("columnValue", $"%{columnValue}%"));
                    // Выполняем команду и получаем данные
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Читаем данные, пока не достигнем конца потока
                        while (await reader.ReadAsync())
                        {
                            // Создаем новый объект Auto и заполняем его данными из текущей строки
                            var autos = new Auto
                            {
                                Код_авто = await reader.GetFieldValueAsync<int>(0),
                                Марка = await reader.GetFieldValueAsync<string>(1),
                                Цвет = await reader.GetFieldValueAsync<string>(2),
                                Тип = await reader.GetFieldValueAsync<string>(3),
                                Госномер = await reader.GetFieldValueAsync<string>(4),
                                Год = await reader.GetFieldValueAsync<int>(5)
                            };

                            // Добавляем созданный объект в список
                            result.Add(autos);
                        }
                    }
                }
            }
            // Возвращаем список автомобилей в формате JSON
            return Ok(result);
        }

        // Метод для получения всех автомобилей
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Создаем пустой список для хранения автомобилей
            var result = new List<Auto>();
            // Открываем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                // Создаем команду для запроса всех автомобилей изтаблицы "Стоянка"."Auto"
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Auto\";", connection))
                {
                    // Выполняем команду и получаем данные
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Читаем данные, пока не достигнем конца потока
                        while (await reader.ReadAsync())
                        {
                            // Создаем новый объект Auto и заполняем его ��анными из текущей строки
                            var autos = new Auto
                            {
                                Код_авто = await reader.GetFieldValueAsync<int>(0),
                                Марка = await reader.GetFieldValueAsync<string>(1),
                                Цвет = await reader.GetFieldValueAsync<string>(2),
                                Тип = await reader.GetFieldValueAsync<string>(3),
                                Госномер = await reader.GetFieldValueAsync<string>(4),
                                Год = await reader.GetFieldValueAsync<int>(5)
                            };

                            // Добавляем созданный объект в список
                            result.Add(autos);
                        }
                    }
                }
            }
            // Возвращаем список автомобилей в формате JSON
            return Ok(result);
        }

        // Метод для обновления автомобиля
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Auto auto)
        {
            // Проверяем, что модель валидна
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Открываем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                // Создаем команду для обновления автомобиля
                await using (var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Auto\" SET \"Марка\"=@Марка, \"Цвет\"=@Цвет, \"Тип\"=@Тип, \"Госномер\"=@Госномер, \"Год\"=@Год WHERE \"Код_авто\" = @id;", connection))
                {
                    // Добавляем параметры в команду
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("Марка", auto.Марка);
                    command.Parameters.AddWithValue("Цвет", auto.Цвет);
                    command.Parameters.AddWithValue("Тип", auto.Тип);
                    command.Parameters.AddWithValue("Госномер", auto.Госномер);
                    command.Parameters.AddWithValue("Год", auto.Год);
                    // Выполняем команду и получаем количество затронутых строк
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    // Если затронута только одна строка, возвращаем статус 200 OK
                    if (rowsAffected == 1)
                    {
                        return Ok();
                    }
                    // Если затронутых строк нет, возвращаем статус 404 Not Found
                    else
                    {
                        return NotFound();
                    }
                }
            }
        }

        // Метод для добавления автомобиля
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Auto auto)
        {
            // Проверяем, что модель валидна
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Открываем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                // Создаем команду для добавления автомобиля
                await using (var command = new NpgsqlCommand("INSERT INTO \"Стоянка\".\"Auto\"( \"Марка\", \"Цвет\", \"Тип\", \"Госномер\", \"Год\") VALUES (@Марка, @Цвет, @Тип, @Госномер, @Год);", connection))
                {
                    // Добавляем параметры в команду
                    command.Parameters.AddWithValue("Марка", auto.Марка);
                    command.Parameters.AddWithValue("Цвет", auto.Цвет);
                    command.Parameters.AddWithValue("Тип", auto.Тип);
                    command.Parameters.AddWithValue("Госномер", auto.Госномер);
                    command.Parameters.AddWithValue("Год", auto.Год);
                    // Выполняем команду и получаем количество затронутых строк
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    // Если затронута только одна строка, возвращаем статус 201 Created
                    if (rowsAffected == 1)
                    {
                        return StatusCode(201);
                    }
                    // Если затронутых строк нет, возвращаем статус 400 Bad Request
                    else
                    {
                        return BadRequest(ModelState);
                    }
                }
            }
        }
        // Метод для удаления автомобиля
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Открываем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                try
                {
                    // Создаем команду для удаления автомобиля
                    await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Auto\" WHERE Код_авто = @id;", connection))
                    {
                        // Добавляем параметр в команду
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

        // Метод для удаления всех автомобилей
        [HttpDelete]
        [Route("Delete_All")]
        public async Task<IActionResult> DeleteAll()
        {
            // Создаем команду для перезапуска последовательности идентификаторов автомобилей
            string query = "ALTER SEQUENCE \"Стоянка\".\"Auto_Code_auto_seq\" RESTART WITH 0";

            // Открываем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                try
                {
                    // Создаем команду для удаления всех автомобилей
                    await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Auto\"", connection))
                    {
                        // Выполняем команду
                        await command.ExecuteNonQueryAsync();
                    }
                    // Создаем команду для перезапуска последовательности идентификаторов автомобилей
                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        // Выполняем команду
                        await command.ExecuteNonQueryAsync();
                    }
                    // Возвращаем статус 200 OK
                    return Ok();
                }
                catch (Exception ex)
                {
                    // Возвращаем статус 500 Internal Server Error
                    return StatusCode(500, ex.Message);
                }
            }
        }
    }
}