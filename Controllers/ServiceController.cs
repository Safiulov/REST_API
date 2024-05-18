using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.DB;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/Service")]
    [RequireHttps]
    public class ServiceController : Controller
    {
        private readonly IConfiguration _databaseService;

        public ServiceController(IConfiguration configuration)
        {
            _databaseService = configuration;
        }

        [HttpGet]
        [Route("Search")]
        // Возвращает список услуг по заданному критерию поиска
        public async Task<IActionResult> Get(string columnName, string columnValue)
        {
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(columnValue))
            {
                return BadRequest("Не указаны параметры для поиска");
            }

            try
            {
                using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction();

                try
                {
                    // Создаем SQL-запрос с условием поиска по заданному столбцу и значению
                    using var command = new NpgsqlCommand($"SELECT * FROM \"Стоянка\".\"Service\" WHERE cast({columnName} as text) ilike @columnValue;", connection);
                    command.Parameters.AddWithValue("columnValue", $"%{columnValue}%");

                    var result = new List<Service>();

                    using var reader = await command.ExecuteReaderAsync();
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

                    transaction.Commit();

                    return Ok(result); // Возвращаем найденные услуги в формате JSON
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw; // rethrow the exception
                }
            }
            catch (Exception)
            {
                // log the exception
                return StatusCode(500, "Ошибка при поиске услуг");
            }
        }

        [HttpGet]
       
        // Возвращает список всех услуг
        public async Task<IActionResult> Get()
        {
            var result = new List<Service>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                 using var transaction = connection.BeginTransaction();

                try
                {
                // Выполняем SQL-запрос на выборку всех услуг из таблицы "Service"
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Service\";", connection))
                {
                        await using var reader = await command.ExecuteReaderAsync();
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
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return Ok(result); // Return found services in JSON format
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Service service)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Устанавливаем соединение с базой данных
            await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {

                // Создаем команду для обновления данных в базе данных
                using var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Service\" SET \"Описание\"=@Описание, \"Оплата\"=@Оплата, \"Стоимость\"=@Стоимость WHERE \"Код_услуги\" = @id;", connection);
                // Добавляем параметры в команду
                command.Parameters.AddWithValue("id", id);
                command.Parameters.AddWithValue("Описание", service.Описание);
                command.Parameters.AddWithValue("Оплата", service.Оплата);
                command.Parameters.AddWithValue("Стоимость", service.Стоимость);

                // Выполняем команду и получаем количество затронутых строк
                int rowsAffected = await command.ExecuteNonQueryAsync();

                // Если была обновлена одна строка, возвращаем статус Ok
                if (rowsAffected == 1)
                {
                    transaction.Commit();
                    return Ok();
                }
                // If no rows were updated, return NotFound status
                else
                {
                    transaction.Rollback();
                    return NotFound();
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }





    }
}
