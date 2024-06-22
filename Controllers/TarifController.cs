using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.DB;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/Tarifs")]
    [RequireHttps]
    public class TarifController : Controller
    {
        private readonly IConfiguration _databaseService;

        public TarifController(IConfiguration configuration)
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
                // Создаем SQL-запрос для выборки данных о тарифах по указанному столбцу
                string query = $"SELECT * FROM \"Стоянка\".\"Tarifs\" WHERE cast({columnName} as text) ILIKE @columnValue";

                // Используем Dapper для выполнения запроса и получения результатов
                var result = await connection.QueryAsync<Tarifs>(query, new { columnValue = $"%{columnValue}%" });

                return Ok(result);
          
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
           
                using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                // Создаем SQL-запрос для выборки всех тарифов
                string query = "SELECT * FROM \"Стоянка\".\"Tarifs\";";

                // Используем Dapper для выполнения запроса и получения результатов
                var result = await connection.QueryAsync<Tarifs>(query);

                return Ok(result);
           
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Tarifs tarifs)
        {
            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            // Создаем SQL-запрос для обновления тарифа по идентификатору
            int rowsAffected = await connection.ExecuteAsync(
           "UPDATE \"Стоянка\".\"Tarifs\" SET \"Название\"=@Название,\"Стоимость\"=@Стоимость  WHERE \"Код_тарифа\" = @id;",
           new { id, tarifs.Название, tarifs.Стоимость }
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


        }
}
