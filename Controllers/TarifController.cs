using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using WebApplication2.DB;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/Tarifs")]
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
            var result = new List<Tarifs>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand($"SELECT * FROM \"Стоянка\".\"Tarifs\" WHERE cast({columnName} as text) ilike @columnValue", connection))
                {
                    command.Parameters.Add(new NpgsqlParameter("columnValue", $"%{columnValue}%"));
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var tarifs = new Tarifs
                            {
                                Код_тарифа = await reader.GetFieldValueAsync<int>(0),
                                Название = await reader.GetFieldValueAsync<string>(1),
                                Условие = await reader.GetFieldValueAsync<string>(2),
                                Время_действия = await reader.GetFieldValueAsync<string>(3),
                                Стоимость = reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<int>(4),

                            };

                            result.Add(tarifs);
                        }
                    }
                }
            }

            return Ok(result);
        }

        [HttpGet]
      
        public async Task<IActionResult> Get()
        {
            var result = new List<Tarifs>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Tarifs\";", connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var tarifs = new Tarifs
                            {
                                Код_тарифа = await reader.GetFieldValueAsync<int>(0),
                                Название = await reader.GetFieldValueAsync<string>(1),
                                Условие = await reader.GetFieldValueAsync<string>(2),
                                Время_действия = await reader.GetFieldValueAsync<string>(3),
                                Стоимость = reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<int>(4),


                            };

                            result.Add(tarifs);
                        }
                    }
                }
            }

            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Tarifs tarifs)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Tarifs\" SET \"Условие\"=@Условие, \"Время_действия\"=@Время_действия, \"Стоимость\"=@Стоимость WHERE \"Код_тарифа\" = @id;", connection))
                {


                    command.Parameters.AddWithValue("id", id);

                    command.Parameters.AddWithValue("Условие", tarifs.Условие);
                    command.Parameters.AddWithValue("Время_действия", tarifs.Время_действия);
                    command.Parameters.AddWithValue("Стоимость", tarifs.Стоимость);


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




    }
}
