using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.DB;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/Service")]
    [RequireHttps]
    public class ServiceController : Controller
    {
        private readonly IConfiguration _databaseService;

        public ServiceController(IConfiguration configuration)
        {
            _databaseService = configuration;
        }
        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Get(string columnName, string columnValue)
        {
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(columnValue))
            {
                return BadRequest("Не указаны параметры для поиска");
            }

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Создаем SQL-запрос с условием поиска по заданному столбцу и значению
            string query = $"SELECT * FROM \"Стоянка\".\"Service\" WHERE cast({columnName} as text) ilike @columnValue;";

            // Используем Dapper для выполнения запроса и получения результатов
            var result = await connection.QueryAsync<Service>(query, new { columnValue = $"%{columnValue}%" });

            return Ok(result); // Возвращаем найденные услуги в формате JSON

        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Выполняем SQL-запрос на выборку всех услуг из таблицы "Service"
            string query = "SELECT * FROM \"Стоянка\".\"Service\";";

            // Используем Dapper для выполнения запроса и получения результатов
            var result = await connection.QueryAsync<Service>(query);

            return Ok(result); // Возвращаем найденные услуги в формате JSON
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Service service)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Создаем SQL-запрос для обновления услуги по идентификатору
            string query = "UPDATE \"Стоянка\".\"Service\" SET \"Стоимость\"=@Стоимость WHERE \"Код_услуги\" = @id;";

            // Используем Dapper для выполнения запроса обновления и получения количества затронутых строк
            int rowsAffected = await connection.ExecuteAsync(query, new { id, service.Стоимость });

            // Если была обновлена одна строка, возвращаем статус Ok
            if (rowsAffected == 1)
            {
                return Ok();
            }
            // Если ни одна строка не была обновлена, возвращаем статус NotFound
            else
            {
                return NotFound();
            }

        }




    }
}
