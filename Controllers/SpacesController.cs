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

            var result = new List<Spaces>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // **Added transaction**
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var commandText = $"SELECT * FROM \"Стоянка\".\"Spaces\" WHERE cast({columnName} as text) ILIKE @columnValue";
                        await using (var command = new NpgsqlCommand(commandText, connection))
                        {
                            command.Parameters.Add(new NpgsqlParameter("columnValue", $"%{columnValue}%"));

                            await using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var spaces = new Spaces
                                    {
                                        Место = await reader.GetFieldValueAsync<string>(0),
                                        Статус = await reader.GetFieldValueAsync<string>(1),
                                    };

                                    result.Add(spaces);
                                }
                            }
                        }

                        transaction.Commit(); // Commit the transaction
                        return Ok(result);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Rollback the transaction on error
                        throw; // Rethrow the exception
                    }
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = new List<Spaces>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // **Added transaction**
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Spaces\";", connection))
                        {
                            await using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var spaces = new Spaces
                                    {
                                        Место = await reader.GetFieldValueAsync<string>(0),
                                        Статус = await reader.GetFieldValueAsync<string>(1),
                                    };

                                    result.Add(spaces);
                                }
                            }
                        }

                        transaction.Commit(); // Commit the transaction
                        return Ok(result);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Rollback the transaction on error
                        throw; // Rethrow the exception
                    }
                }
            }
        }



        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Spaces spaces)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // **Added transaction**
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await using (var command = new NpgsqlCommand("INSERT INTO \"Стоянка\".\"Spaces\"(\"Место\") VALUES (@Место);", connection))
                        {
                            command.Parameters.AddWithValue("Место", spaces.Место);
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            if (rowsAffected == 1)
                            {
                                transaction.Commit(); // Commit the transaction
                                return Ok();
                            }
                            else
                            {
                                return BadRequest(ModelState);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Rollback the transaction on error
                        throw; // Rethrow the exception
                    }
                }
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // **Added transaction**
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Spaces\" WHERE \"Место\" = @id;", connection))
                        {

                            command.Parameters.AddWithValue("id", id);
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            if (rowsAffected > 0)
                            {
                                transaction.Commit(); // Commit the transaction
                                return Ok();
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Rollback the transaction on error
                        throw; // Rethrow the exception
                    }
                }
            }
        }



        [HttpDelete]
        [Route("Delete_All")]
        public async Task<IActionResult> DeleteAll()
        {
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // **Added transaction**
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Spaces\";", connection))
                        {
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            if (rowsAffected > 0)
                            {
                                transaction.Commit(); // Commit the transaction
                                return Ok();
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Rollback the transaction on error
                        throw; // Rethrow the exception
                    }
                }
            }
        }
    }
}
