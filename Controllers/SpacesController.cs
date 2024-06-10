using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.DB;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/Spaces")]
    [RequireHttps]
    public class SpacesController : Controller
    {
        private readonly IConfiguration _databaseService;

        public SpacesController(IConfiguration configuration)
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
                // Создаем SQL-запрос для выборки данных о местах по указанному столбцу
                string query = $"SELECT * FROM \"Стоянка\".\"Spaces\" WHERE cast({columnName} as text) ILIKE @columnValue";

                // Используем Dapper для выполнения запроса и получения результатов
                var result = await connection.QueryAsync<Spaces>(query, new { columnValue = $"%{columnValue}%" });

                return Ok(result);
            
           
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
           
                using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                // Создаем SQL-запрос для выборки всех мест
                string query = "SELECT * FROM \"Стоянка\".\"Spaces\";";

                // Используем Dapper для выполнения запроса и получения результатов
                var result = await connection.QueryAsync<Spaces>(query);

                return Ok(result);
           
           
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Spaces spaces)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

                using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                // Создаем SQL-запрос для вставки места
                string query = "INSERT INTO \"Стоянка\".\"Spaces\"(\"Место\") VALUES (@Место);";

                // Используем Dapper для выполнения вставки данных
                int rowsAffected = await connection.ExecuteAsync(query, new { spaces.Место });

                if (rowsAffected == 1)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(ModelState);
                }

        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
                using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                // Создаем SQL-запрос для удаления места по идентификатору
                string query = "DELETE FROM \"Стоянка\".\"Spaces\" WHERE \"Место\" = @id;";

                // Используем Dapper для выполнения удаления данных
                int rowsAffected = await connection.ExecuteAsync(query, new { id });

                if (rowsAffected > 0)
                {
                    return Ok();
                }
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
                "DELETE FROM \"Стоянка\".\"Spaces\";"
            );
            // Возвращаем статус 200 OK
            return Ok();

        }
    }
}