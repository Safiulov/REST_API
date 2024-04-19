using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using WebApplication2.DB;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/Spaces")]
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
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> Get()
        {
            var result = new List<Spaces>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
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
            }

            return Ok(result);
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
               await using (var command = new NpgsqlCommand("INSERT INTO \"Стоянка\".\"Spaces\"(\"Место\") VALUES (@Место);", connection))
                {
                    command.Parameters.AddWithValue("Место", spaces.Место);
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
        public async Task<IActionResult> Delete(string id)
        {
           await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Spaces\" WHERE \"Место\" = @id;", connection))
                {
                   
                    command.Parameters.AddWithValue("id",id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
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
        [Route("Delete_All")]
        public async Task<IActionResult> DeleteAll(int id)
        {
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Spaces\";", connection))
                {
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
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
    }
}
