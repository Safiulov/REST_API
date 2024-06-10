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
                // Проверяем, существует ли место
                var spaceExists = await connection.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM \"Стоянка\".\"Spaces\" WHERE \"Место\" = @Место)",
                    new { sales.Место });

                if (!spaceExists)
                {
                    ModelState.AddModelError(string.Empty, "Место не существует");
                    return BadRequest(ModelState);
                }

                // Проверяем, занято ли место
                var existingSale = await connection.QueryFirstOrDefaultAsync<Sales>(
                    "SELECT * FROM \"Стоянка\".\"Sales\" WHERE \"Место\" = @Место AND \"Дата_выезда\" IS NULL",
                    new { sales.Место });

                if (existingSale != null)
                {
                    ModelState.AddModelError(string.Empty, "Данное место уже занято");
                    return BadRequest(ModelState);
                }

                // Проверяем наличие конфликтов по времени
                var timeConflict = await connection.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM \"Стоянка\".\"Sales\" WHERE \"Место\" = @Место AND ((\"Дата_въезда\" <= @Дата_въезда AND (\"Дата_выезда\" >= @Дата_въезда OR \"Дата_выезда\" IS NULL)) OR (\"Дата_въезда\" >= @Дата_въезда AND \"Дата_въезда\" <= @Дата_выезда)))",
                    new
                    {
                        sales.Место,
                        Дата_въезда = sales.Дата_въезда.ToUniversalTime(),
                        Дата_выезда = sales.Дата_выезда?.ToUniversalTime()
                    });

                if (timeConflict)
                {
                    ModelState.AddModelError(string.Empty, "Выберите другое время");
                    return BadRequest(ModelState);
                }

                // Вставляем новую продажу
                var rowsAffected = await connection.ExecuteAsync(
                    "INSERT INTO \"Стоянка\".\"Sales\"(\"Дата_въезда\", \"Дата_выезда\", \"Место\", \"Код_клиента\") VALUES (@Дата_въезда, @Дата_выезда, @Место, @Код_клиента);",
                    new
                    {
                        Дата_въезда = sales.Дата_въезда.ToUniversalTime(),
                        Дата_выезда = sales.Дата_выезда?.ToUniversalTime(),
                        sales.Место,
                        sales.Код_клиента
                    },
                    transaction);

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
            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Используем Dapper для выполнения запроса на удаление всех автомобилей
            await connection.ExecuteAsync(
                "DELETE FROM \"Стоянка\".\"Sales\";"
            );

            // Используем Dapper для выполнения запроса на перезапуск последовательности идентификаторов автомобилей
            await connection.ExecuteAsync(
                "ALTER SEQUENCE \"Стоянка\".\"Sales_Code_sale_seq\" RESTART WITH 0;"
            );

            // Возвращаем статус 200 OK
            return Ok();
        }
    }
}