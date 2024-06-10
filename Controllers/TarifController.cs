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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

                using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                // Создаем SQL-запрос для обновления тарифа по идентификатору
                string query = "UPDATE \"Стоянка\".\"Tarifs\" SET \"Условие\"=@Условие, \"Время_действия\"=@Время_действия, \"Стоимость\"=@Стоимость WHERE \"Код_тарифа\" = @id;";

                // Используем Dapper для выполнения обновления данных
                int rowsAffected = await connection.ExecuteAsync(query, new { id, tarifs.Условие, tarifs.Время_действия, tarifs.Стоимость });

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
