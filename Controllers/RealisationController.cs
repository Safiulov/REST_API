using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using WebApplication2.DB;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/Realisation")]
    public class RealisationController : Controller
    {
        private readonly IConfiguration _databaseService;

        public RealisationController(IConfiguration configuration)
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
            var result = new List<Realisation>();

            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                string sql = $"SELECT * from \"Стоянка\".\"Realisation\" WHERE cast({columnName} as text) ilike '%{columnValue}%';";
                await using (var command = new NpgsqlCommand(sql, connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var realisation = new Realisation
                            {
                                Код = await reader.GetFieldValueAsync<int>(0),
                                Дата_въезда = await reader.GetFieldValueAsync<DateTime>(1),
                                Место = await reader.GetFieldValueAsync<string>(2),
                                Код_услуги = await reader.GetFieldValueAsync<int>(3),
                                Название_услуги = reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<string>(4),
                                Код_клиента = await reader.GetFieldValueAsync<int>(5),
                                ФИО = reader.IsDBNull(6) ? null : await reader.GetFieldValueAsync<string>(6),
                                Госномер = reader.IsDBNull(7) ? null : await reader.GetFieldValueAsync<string>(7),
                                Стоимость = reader.IsDBNull(8) ? null : await reader.GetFieldValueAsync<int>(8),
                                Сумма = reader.IsDBNull(9) ? null : await reader.GetFieldValueAsync<int>(9)
                            };

                            result.Add(realisation);
                        }
                    }
                }
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = new List<Realisation>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Realisation\";", connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var realisation = new Realisation
                            {
                                Код = await reader.GetFieldValueAsync<int>(0),
                                Дата_въезда = await reader.GetFieldValueAsync<DateTime>(1),
                                Место = await reader.GetFieldValueAsync<string>(2),
                                Код_услуги = await reader.GetFieldValueAsync<int>(3),
                                Название_услуги = reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<string>(4),
                                Код_клиента = await reader.GetFieldValueAsync<int>(5),
                                ФИО = reader.IsDBNull(6) ? null : await reader.GetFieldValueAsync<string>(6),
                                Госномер = reader.IsDBNull(7) ? null : await reader.GetFieldValueAsync<string>(7),
                                Стоимость = reader.IsDBNull(8) ? null : await reader.GetFieldValueAsync<int>(8),
                                Сумма = reader.IsDBNull(9) ? null : await reader.GetFieldValueAsync<int>(9)
                            };

                            result.Add(realisation);
                        }
                    }
                }
            }

            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Realisation realisation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Realisation\" SET  \"Дата_въезда\"=@Дата_въезда, \"Место\"=@Место, \"Код_услуги\"=@Код_услуги, \"Код_клиента\"=@Код_клиента WHERE \"Код\" = @id;", connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("Дата_въезда", realisation.Дата_въезда);
                    command.Parameters.AddWithValue("Место", realisation.Место);
                    command.Parameters.AddWithValue("Код_услуги", realisation.Код_услуги);
                    command.Parameters.AddWithValue("Код_клиента", realisation.Код_клиента);

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
        public async Task<IActionResult> Post([FromBody] Realisation realisation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (realisation.Место.StartsWith("A") && (realisation.Код_услуги == 1 || realisation.Код_услуги == 2))
            {
                ModelState.AddModelError(string.Empty, "Ошибка: место начинается с 'A' и код_услуги равен 1 или 2");
                return BadRequest(ModelState);
            }

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                          

                await using (var command = new NpgsqlCommand("INSERT INTO \"Стоянка\".\"Realisation\"(\"Дата_въезда\", \"Место\",  \"Код_услуги\", \"Код_клиента\") VALUES (@Дата_въезда, @Место, @Код_услуги, @Код_клиента);", connection))
                {
                    command.Parameters.AddWithValue("Дата_въезда", realisation.Дата_въезда.ToUniversalTime());
                    command.Parameters.AddWithValue("Место", realisation.Место);
                    command.Parameters.AddWithValue("Код_услуги", realisation.Код_услуги);
                    command.Parameters.AddWithValue("Код_клиента", realisation.Код_клиента);
                    
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
                await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Realisation\" WHERE \"Код\" = @id;", connection))
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
            string query = "ALTER SEQUENCE \"Стоянка\".\"Realisation_Code_seq\" RESTART WITH 0";

           await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
               await using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                       await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Realisation\"", connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }

                        await using (var command = new NpgsqlCommand(query, connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        transaction.Commit();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, ex.Message);
                    }
                }
            }
        }



    }
}
