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
        // Метод для поиска продаж по определенному столбцу и значению
        // Возвращает список найденных продаж в формате JSON
        public async Task<IActionResult> Get(string columnName, string columnValue)
        {
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(columnValue))
            {
                // Если не указано имя столбца или значение для поиска, возвращаем ошибку
                return BadRequest("Не указаны параметры для поиска");
            }

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Формируем SQL-запрос для поиска продаж по указанному столбцу и значению
                string sql = $"SELECT * from \"Стоянка\".\"Sales\" WHERE cast({columnName} as text) ilike '%{columnValue}%';";
                await using var command = new NpgsqlCommand(sql, connection, transaction);
                await using var reader = await command.ExecuteReaderAsync();
                var result = new List<Sales>();

                while (await reader.ReadAsync())
                {
                    var sales = new Sales
                    {
                        Код = await reader.GetFieldValueAsync<int>(0), // Код продажи
                        Дата_въезда = await reader.GetFieldValueAsync<DateTime>(1), // Дата въезда автомобиля
                        Дата_выезда = reader.IsDBNull(2) ? null : await reader.GetFieldValueAsync<DateTime>(2), // Дата выезда автомобиля (может быть null, если автомобиль еще не покинул стоянку)
                        Тариф = reader.IsDBNull(3) ? null : await reader.GetFieldValueAsync<int>(3), // Тариф за 1 час стоянки
                        Время_стоянки = reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<int>(4), // Время стоянки в часах
                        Стоимость = reader.IsDBNull(5) ? null : await reader.GetFieldValueAsync<int>(5), // Стоимость стоянки
                        Место = await reader.GetFieldValueAsync<string>(6), // Номер места на стоянке
                        Код_клиента = await reader.GetFieldValueAsync<int>(7), // Код клиента, совершившего продажу
                        ФИО = await reader.GetFieldValueAsync<string>(8), // ФИО клиента
                        Госномер = await reader.GetFieldValueAsync<string>(9) // Государственный номер автомобиля
                    };

                    result.Add(sales);
                }

                transaction.Commit();
                return Ok(result); // Возвращаем найденные продажи в формате JSON
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw; // rethrow the exception
            }
        }

        [HttpGet]
        // Метод для получения списка всех продаж из базы данных
        // Возвращает список всех продаж в формате JSON
        public async Task<IActionResult> Get()
        {
            var result = new List<Sales>();

            await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Выполняем SQL-запрос для получения всех продаж из таблицы "Стоянка"."Sales"
                const string sql = "SELECT * FROM \"Стоянка\".\"Sales\";";

                await using (var command = new NpgsqlCommand(sql, connection, transaction))
                {
                    await using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        result.Add(new Sales
                        {
                            Код = reader.GetInt32(0), // Код продажи
                            Дата_въезда = reader.GetDateTime(1), // Дата въезда автомобиля
                            Дата_выезда = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2), // Дата выезда автомобиля (может быть null, если автомобиль еще не покинул стоянку)
                            Тариф = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3), // Тариф за 1 час стоянки
                            Время_стоянки = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4), // Время стоянки в часах
                            Стоимость = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5), // Стоимость стоянки
                            Место = reader.GetString(6), // Номер места на стоянке
                            Код_клиента = reader.GetInt32(7), // Код клиента, совершившего продажу
                            ФИО = reader.GetString(8), // ФИО клиента
                            Госномер = reader.GetString(9) // Государственный номер автомобиля
                        });
                    }
                }

                transaction.Commit();
                return Ok(result); // Возвращаем список всех продаж в формате JSON
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw; // rethrow the exception
            }
        }


        [HttpPut("{id}")]
        // Метод для обновления данных продажи по указанному идентификатору
        public async Task<IActionResult> Put(int id, [FromBody] Sales sales)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            // Формируем SQL-запрос для обновления данных продажи по указанному идентификатору
            await using var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Sales\" SET \"Дата_въезда\"=@Дата_въезда, \"Дата_выезда\"=@Дата_выезда, \"Место\"=@Место, \"Код_клиента\"=@Код_клиента WHERE \"Код\" = @id;", connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("Дата_въезда", sales.Дата_въезда);
            if (sales.Дата_выезда != null)
                command.Parameters.AddWithValue("Дата_выезда", sales.Дата_выезда);
            else
                command.Parameters.AddWithValue("Дата_выезда", DBNull.Value);
            command.Parameters.AddWithValue("Место", sales.Место);
            command.Parameters.AddWithValue("Код_клиента", sales.Код_клиента);

            int rowsAffected = await command.ExecuteNonQueryAsync();

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
        // Метод для удаления продажи по указанному идентификатору
        public async Task<IActionResult> Delete(int id)
        {
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Формируем SQL-запрос для удаления продажи по указанному идентификатору
                await using var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Sales\" WHERE \"Код\" = @id;", connection);
                command.Parameters.AddWithValue("id", id);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 1)
                {
                    transaction.Commit();
                    return Ok(); // Если удалена 1 запись, возвращаем код 200 (OK)
                }
                else
                {
                    transaction.Rollback();
                    return NotFound(); // Если не удалена ни одна запись, возвращаем код 404 (Not Found)
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }



        [HttpDelete]
        // Делетит все данные в таблице "Sales" и сбрасывает последовательность "Sales_Code_sale_seq" в PostgreSQL базе данных
        public async Task<IActionResult> DeleteAll()
        {
            string query = "ALTER SEQUENCE \"Стоянка\".\"Sales_Code_sale_seq\" RESTART WITH 0";

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Удаляем все данные в таблице "Sales"
                await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Sales\"", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                // Сбрасываем последовательность "Sales_Code_sale_seq" в PostgreSQL базе данных
                await using (var command = new NpgsqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return Ok(); // Возвращаем код 200 OK, если операция прошла успешно
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                // Возвращаем код 500 Internal Server Error и сообщение об ошибке, если операция не удалась
                return StatusCode(500, ex.Message);
            }
        }



    }
}
