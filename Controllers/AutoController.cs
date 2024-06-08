// Импортируем необходимые библиотеки
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text;
using WebApplication1.DB;

// Определяем пространство имен и класс контроллера
namespace WebApplication1.Controllers
{
    // Указываем, что это контроллер API и задаем маршрут для него
    [ApiController]
    [Route("api/Auto")]
    [RequireHttps]
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
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(columnValue))
            {
                return BadRequest("Не указаны параметры для поиска");
            }

            var connectionString = _databaseService.GetConnectionString("DefaultConnection");

            using var connection = new NpgsqlConnection(connectionString);
            var query = $"SELECT * FROM \"Стоянка\".\"Auto\" WHERE cast({columnName} as text) ilike @columnValue;";

            var result = await connection.QueryAsync<Auto>(query, new { columnValue = $"%{columnValue}%" });

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Auto>(
                "SELECT * FROM \"Стоянка\".\"Auto\";"
            );
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Auto auto)
        {
            // Проверяем, что модель валидна
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Используем Dapper для выполнения запроса и получения количества затронутых строк
            int rowsAffected = await connection.ExecuteAsync(
                "UPDATE \"Стоянка\".\"Auto\" SET \"Марка\"=@Марка, \"Цвет\"=@Цвет, \"Тип\"=@Тип, \"Госномер\"=@Госномер, \"Год\"=@Год WHERE \"Код_авто\" = @id;",
                new { id, auto.Марка, auto.Цвет, auto.Тип, auto.Госномер, auto.Год }
            );

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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Auto auto)
        {
            // Проверяем, что модель валидна
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Используем Dapper для выполнения запроса и получения количества затронутых строк
            int rowsAffected = await connection.ExecuteAsync(
                "INSERT INTO \"Стоянка\".\"Auto\"( \"Марка\", \"Цвет\", \"Тип\", \"Госномер\", \"Год\") VALUES (@Марка, @Цвет, @Тип, @Госномер, @Год);",
                new { auto.Марка, auto.Цвет, auto.Тип, auto.Госномер, auto.Год }
            );

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Используем Dapper для выполнения запроса и получения количества затронутых строк
            int rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM \"Стоянка\".\"Auto\" WHERE Код_авто = @id;",
                new { id }
            );

            // Если затронута только одна строка, возвращаем статус 200 OK
            if (rowsAffected == 1)
            {
                return Ok();
            }
            // Если запись не найдена, возвращаем 404 Not Found
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("Delete_All")]
        public async Task<IActionResult> DeleteAll()
        {
            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Используем Dapper для выполнения запроса на удаление всех автомобилей
            await connection.ExecuteAsync(
                "DELETE FROM \"Стоянка\".\"Auto\";"
            );

            // Используем Dapper для выполнения запроса на перезапуск последовательности идентификаторов автомобилей
            await connection.ExecuteAsync(
                "ALTER SEQUENCE \"Стоянка\".\"Auto_Code_auto_seq\" RESTART WITH 0;"
            );

            // Возвращаем статус 200 OK
            return Ok();
        }
    }
}