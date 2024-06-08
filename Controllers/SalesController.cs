using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.DB;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/Sales")]
    [RequireHttps]
    public class SalesController : Controller
    {
        private readonly IConfiguration _databaseService;

        public SalesController(IConfiguration configuration)
        {
            _databaseService = configuration;
        }


        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Get(string columnName, string columnValue)
        {
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(columnValue))
            {
                // Если не указано имя столбца или значение для поиска, возвращаем ошибку
                return BadRequest("Не указаны параметры для поиска");
            }

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            // Формируем SQL-запрос для поиска продаж по указанному столбцу и значению
            string sql = $"SELECT * from \"Стоянка\".\"Sales\" WHERE cast({columnName} as text) ilike '%' || @columnValue || '%';";

            // Выполняем запрос с использованием Dapper и параметризованных значений
            var result = await connection.QueryAsync<Sales>(sql, new { columnValue });

            // Возвращаем найденные продажи в формате JSON
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            // Выполняем SQL-запрос для получения всех продаж из таблицы "Стоянка"."Sales"
            const string sql = "SELECT * FROM \"Стоянка\".\"Sales\";";

            // Выполняем запрос с использованием Dapper
            var result = await connection.QueryAsync<Sales>(sql);

            // Возвращаем список всех продаж в формате JSON
            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Sales sales)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            // Формируем SQL-запрос для обновления данных продажи по указанному идентификатору
            string sql = @"UPDATE ""Стоянка"".""Sales"" SET ""Дата_въезда""=@Дата_въезда, ""Дата_выезда""=@Дата_выезда, ""Место""=@Место, ""Код_клиента""=@Код_клиента WHERE ""Код"" = @id;";

            // Выполняем запрос с использованием Dapper и параметризованных значений
            int rowsAffected = await connection.ExecuteAsync(sql, new { id, sales.Дата_въезда, sales.Дата_выезда, sales.Место, sales.Код_клиента });

            if (rowsAffected == 1)
            {
                return Ok(); // Если обновлено 1 запись, возвращаем код 200 (OK)
            }
            else
            {
                return NotFound(); // Если не обновлено ни одной записи, возвращаем код 404 (Not Found)
            }
        }

        [HttpPost]
        // Метод для создания новой продажи
        public async Task<IActionResult> Post([FromBody] Sales sales)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Check if space exists
                var spaceExistsCommand = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Spaces\" WHERE \"Место\" = @Место", connection);
                spaceExistsCommand.Parameters.AddWithValue("Место", sales.Место);
                var spaceExists = await spaceExistsCommand.ExecuteScalarAsync();
                if (spaceExists == null)
                {
                    ModelState.AddModelError(string.Empty, "Место не существует");
                    return BadRequest(ModelState);
                }

                // Check if space is already occupied
                var existingRealisationCommand = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Sales\" WHERE \"Место\" = @Место and \"Дата_выезда\" is null;", connection);
                existingRealisationCommand.Parameters.AddWithValue("Место", sales.Место);
                var existingRealisation = await existingRealisationCommand.ExecuteScalarAsync();
                if (existingRealisation != null)
                {
                    ModelState.AddModelError(string.Empty, "Данное место уже занято");
                    return BadRequest(ModelState);
                }

                // Check for time conflicts
                var timeConflictCommand = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Sales\" WHERE \"Место\" = @Место AND ((\"Дата_въезда\" <= @Дата_въезда AND \"Дата_выезда\" >= @Дата_въезда) OR (\"Дата_въезда\" <= @Дата_выезда AND \"Дата_выезда\" >= @Дата_выезда) OR (\"Дата_въезда\" >= @Дата_въезда AND \"Дата_выезда\" <= @Дата_выезда) OR (\"Дата_въезда\" <= @Дата_въезда AND \"Дата_выезда\" IS NULL) OR (\"Дата_въезда\" <= @Дата_выезда AND \"Дата_выезда\" IS NULL))", connection);
                timeConflictCommand.Parameters.AddWithValue("Место", sales.Место);
                timeConflictCommand.Parameters.AddWithValue("Дата_въезда", sales.Дата_въезда.ToUniversalTime());
                timeConflictCommand.Parameters.AddWithValue("Дата_выезда", sales.Дата_выезда.HasValue ? sales.Дата_выезда.Value.ToUniversalTime() : (object)DBNull.Value);
                var timeConflict = await timeConflictCommand.ExecuteScalarAsync();
                if (timeConflict != null)
                {
                    ModelState.AddModelError(string.Empty, "Выберите другое время");
                    return BadRequest(ModelState);
                }

                // Insert new sale
                var insertCommand = new NpgsqlCommand("INSERT INTO \"Стоянка\".\"Sales\"(\"Дата_въезда\", \"Дата_выезда\",  \"Место\", \"Код_клиента\") VALUES (@Дата_въезда, @Дата_выезда, @Место, @Код_клиента);", connection);
                insertCommand.Parameters.AddWithValue("Дата_въезда", sales.Дата_въезда.ToUniversalTime());
                insertCommand.Parameters.AddWithValue("Дата_выезда", sales.Дата_выезда.HasValue ? sales.Дата_выезда.Value.ToUniversalTime() : (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("Место", sales.Место);
                insertCommand.Parameters.AddWithValue("Код_клиента", sales.Код_клиента);

                int rowsAffected = await insertCommand.ExecuteNonQueryAsync();
                if (rowsAffected == 1)
                {
                    transaction.Commit();
                    return Ok();
                }
                else
                {
                    transaction.Rollback();
                    return BadRequest(ModelState);
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            int rowsAffected = await connection.ExecuteAsync("DELETE FROM \"Стоянка\".\"Sales\" WHERE \"Код\" = @id;", new { id });

            if (rowsAffected == 1)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            string deleteQuery = "DELETE FROM \"Стоянка\".\"Sales\"";
            string resetSequenceQuery = "ALTER SEQUENCE \"Стоянка\".\"Sales_Code_sale_seq\" RESTART WITH 0";

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            try
            {
                // Используем Dapper для выполнения DELETE-запроса
                await connection.ExecuteAsync(deleteQuery);

                // Используем Dapper для выполнения ALTER SEQUENCE-запроса
                await connection.ExecuteAsync(resetSequenceQuery);

                return Ok(); // Возвращаем код 200 OK, если операция прошла успешно
            }
            catch (Exception ex)
            {
                // Возвращаем код 500 Internal Server Error и сообщение об ошибке, если операция не удалась
                return StatusCode(500, ex.Message);
            }
        }
    }
}