using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using WebApplication2.DB;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/Sales")]
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
                return BadRequest("Не указаны параметры для поиска");
            }
            var result = new List<Sales>();

            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                string sql = $"SELECT * from \"Стоянка\".\"Sales\" WHERE cast({columnName} as text) ilike '%{columnValue}%';";
                await using (var command = new NpgsqlCommand(sql, connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var sales = new Sales
                            {
                                Код = await reader.GetFieldValueAsync<int>(0),
                                Дата_въезда = await reader.GetFieldValueAsync<DateTime>(1),
                                Дата_выезда = reader.IsDBNull(2) ? null : await reader.GetFieldValueAsync<DateTime>(2),
                                Тариф = reader.IsDBNull(3) ? null : await reader.GetFieldValueAsync<int>(3),
                                Время_стоянки = reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<int>(4),
                                Стоимость = reader.IsDBNull(5) ? null : await reader.GetFieldValueAsync<int>(5),
                                Место = await reader.GetFieldValueAsync<string>(6),
                                Код_клиента = await reader.GetFieldValueAsync<int>(7),
                                ФИО = await reader.GetFieldValueAsync<string>(8),
                                Госномер = await reader.GetFieldValueAsync<string>(9)
                            };

                            result.Add(sales);
                        }
                    }
                }
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = new List<Sales>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Sales\";", connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var sales = new Sales
                            {
                               
                                Код = await reader.GetFieldValueAsync<int>(0),
                                Дата_въезда = await reader.GetFieldValueAsync<DateTime>(1),
                                Дата_выезда = reader.IsDBNull(2) ? null : await reader.GetFieldValueAsync<DateTime>(2),
                                Тариф = reader.IsDBNull(3) ? null : await reader.GetFieldValueAsync<int>(3),
                                Время_стоянки = reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<int>(4),
                                Стоимость = reader.IsDBNull(5) ? null : await reader.GetFieldValueAsync<int>(5),
                                Место = await reader.GetFieldValueAsync<string>(6),
                                Код_клиента = await reader.GetFieldValueAsync<int>(7),
                                ФИО = await reader.GetFieldValueAsync<string>(8),
                                Госномер = await reader.GetFieldValueAsync<string>(9)

                            };

                            result.Add(sales);
                        }
                    }
                }
            }

            return Ok(result);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Sales sales)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

           await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                connection.OpenAsync();
                await using (var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Sales\" SET  \"Дата_въезда\"=@Дата_въезда, \"Дата_выезда\"=@Дата_выезда, \"Место\"=@Место, \"Код_клиента\"=@Код_клиента WHERE \"Код\" = @id;", connection))
                {



                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("Дата_въезда",sales.Дата_въезда);
                    command.Parameters.AddWithValue("Дата_выезда",sales.Дата_выезда);
                    command.Parameters.AddWithValue("Место", sales.Место);
                    command.Parameters.AddWithValue("Код_клиента", sales.Код_клиента);


                    int rowsAffected = await command.ExecuteNonQueryAsync();

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




        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Sales sales)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Проверка наличия записи с таким местом
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Sales\" WHERE \"Место\" = @Место and \"Дата_выезда\" is null;", connection))
                {
                    command.Parameters.AddWithValue("Место", sales.Место);

                    var existingRealisation = await command.ExecuteScalarAsync();
                    if (existingRealisation != null)
                    {
                        ModelState.AddModelError(string.Empty, "Данное место уже занято");
                        return BadRequest(ModelState);
                    }
                }

                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Sales\" WHERE \"Место\" = @Место AND ((\"Дата_въезда\" <= @Дата_въезда AND \"Дата_выезда\" >= @Дата_въезда) OR (\"Дата_въезда\" <= @Дата_выезда AND \"Дата_выезда\" >= @Дата_выезда) OR (\"Дата_въезда\" >= @Дата_въезда AND \"Дата_выезда\" <= @Дата_выезда) OR (\"Дата_въезда\" <= @Дата_въезда AND \"Дата_выезда\" IS NULL) OR (\"Дата_въезда\" <= @Дата_выезда AND \"Дата_выезда\" IS NULL))", connection))
                {
                    command.Parameters.AddWithValue("Место", sales.Место);
                    command.Parameters.AddWithValue("Дата_въезда", sales.Дата_въезда.ToUniversalTime());
                    command.Parameters.AddWithValue("Дата_выезда", sales.Дата_выезда.HasValue ? sales.Дата_выезда.Value.ToUniversalTime() : (object)DBNull.Value);

                    var existingRealisation = await command.ExecuteScalarAsync();
                    if (existingRealisation != null)
                    {
                        ModelState.AddModelError(string.Empty, "Выберите другое время");
                        return BadRequest(ModelState);
                    }
                }


                await using (var command = new NpgsqlCommand("INSERT INTO \"Стоянка\".\"Sales\"(\"Дата_въезда\", \"Дата_выезда\",  \"Место\", \"Код_клиента\") VALUES (@Дата_въезда, @Дата_выезда, @Место, @Код_клиента);", connection))
                {
                    command.Parameters.AddWithValue("Дата_въезда", sales.Дата_въезда.ToUniversalTime());
                    command.Parameters.AddWithValue("Дата_выезда", sales.Дата_выезда.HasValue ? sales.Дата_выезда.Value.ToUniversalTime() : (object)DBNull.Value);
                    command.Parameters.AddWithValue("Место", sales.Место);
                    command.Parameters.AddWithValue("Код_клиента", sales.Код_клиента);
                    
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected == 1)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest(ModelState);
                    }
                }
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
               await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Sales\" WHERE \"Код\" = @id;", connection))
                {
                    command.Parameters.AddWithValue("id", id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

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



        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            string query = "ALTER SEQUENCE \"Стоянка\".\"Sales_Code_sale_seq\" RESTART WITH 0";

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Sales\"", connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }

                        await using (var command = new NpgsqlCommand(query, connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        await transaction.CommitAsync();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, ex.Message);
                    }
                }
            }
        }




    }
}
