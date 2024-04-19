using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using WebApplication2.DB;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/Service")]
    public class ServiceController : Controller
    {
        private readonly IConfiguration _databaseService;

        public ServiceController(IConfiguration configuration)
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
            var result = new List<Service>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand($"SELECT * FROM \"Стоянка\".\"Service\" WHERE cast({columnName} as text) ilike @columnValue;", connection))
                {
                    command.Parameters.AddWithValue("columnValue", $"%{columnValue}%");

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var service = new Service
                            {
                                Код_услуги = await reader.GetFieldValueAsync<int>(0),
                                Название = await reader.GetFieldValueAsync<string>(1),
                                Описание = await reader.GetFieldValueAsync<string>(2),
                                Оплата = await reader.GetFieldValueAsync<string>(3),
                                Стоимость = await reader.GetFieldValueAsync<int>(4),
                            };

                            result.Add(service);
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
            var result = new List<Service>();

           await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
               await connection.OpenAsync();
               await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Service\";", connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var service = new Service
                            {
                                Код_услуги = await reader.GetFieldValueAsync<int>(0),
                                Название = await reader.GetFieldValueAsync<string>(1),
                                Описание = await reader.GetFieldValueAsync<string>(2),
                                Оплата = await reader.GetFieldValueAsync<string>(3),
                                Стоимость = await reader.GetFieldValueAsync<int>(4),


                            };

                            result.Add(service);
                        }
                    }
                }
            }

            return Ok(result);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Service service)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Service\" SET \"Название\"=@Название, \"Описание\"=@Описание, \"Оплата\"=@Оплата, \"Стоимость\"=@Стоимость WHERE \"Код_услуги\" = @id;", connection))
                {
                    command.Parameters.AddWithValue("id",id);
                    command.Parameters.AddWithValue("Название", service.Название);
                    command.Parameters.AddWithValue("Описание", service.Описание);
                    command.Parameters.AddWithValue("Оплата", service.Оплата);
                    command.Parameters.AddWithValue("Стоимость", service.Стоимость);

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
