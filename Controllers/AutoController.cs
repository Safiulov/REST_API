using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using WebApplication2.DB;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/Auto")]
    public class AutoController : Controller
    {
        private readonly IConfiguration _databaseService;

        public AutoController(IConfiguration configuration)
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
            var result = new List<Auto>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                var commandText = $"SELECT * FROM \"Стоянка\".\"Auto\" WHERE cast({columnName} as text) ilike @columnValue;";
                await using (var command = new NpgsqlCommand(commandText, connection))
                {
                    command.Parameters.Add(new NpgsqlParameter("columnValue", $"%{columnValue}%"));

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var autos = new Auto
                            {
                                Код_авто = await reader.GetFieldValueAsync<int>(0),
                                Марка = await reader.GetFieldValueAsync<string>(1),
                                Цвет = await reader.GetFieldValueAsync<string>(2),
                                Тип = await reader.GetFieldValueAsync<string>(3),
                                Госномер = await reader.GetFieldValueAsync<string>(4),
                                Год = await reader.GetFieldValueAsync<int>(5)
                            };

                            result.Add(autos);
                        }
                    }
                }
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Создаем пустой список для хранения автомобилей
            var result = new List<Auto>();

            // Открываем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Создаем команду для запроса всех автомобилей из таблицы "Стоянка"."Auto"
                await using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Auto\";", connection))
                {
                    // Выполняем команду и получаем данные
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Читаем данные, пока не достигнем конца потока
                        while (await reader.ReadAsync())
                        {
                            // Создаем новый объект Auto и заполняем его данными из текущей строки
                            var autos = new Auto
                            {
                                Код_авто = await reader.GetFieldValueAsync<int>(0),
                                Марка = await reader.GetFieldValueAsync<string>(1),
                                Цвет = await reader.GetFieldValueAsync<string>(2),
                                Тип = await reader.GetFieldValueAsync<string>(3),
                                Госномер = await reader.GetFieldValueAsync<string>(4),
                                Год = await reader.GetFieldValueAsync<int>(5)
                            };

                            // Добавляем созданный объект в список
                            result.Add(autos);
                        }
                    }
                }
            }

            // Возвращаем список автомобилей в формате JSON
            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Auto auto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand("UPDATE \"Стоянка\".\"Auto\" SET \"Марка\"=@Марка, \"Цвет\"=@Цвет, \"Тип\"=@Тип, \"Госномер\"=@Госномер, \"Год\"=@Год WHERE \"Код_авто\" = @id;", connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("Марка", auto.Марка);
                    command.Parameters.AddWithValue("Цвет", auto.Цвет);
                    command.Parameters.AddWithValue("Тип", auto.Тип);
                    command.Parameters.AddWithValue("Госномер", auto.Госномер);
                    command.Parameters.AddWithValue("Год", auto.Год);

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
        public async Task<IActionResult> Post([FromBody] Auto auto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

               await using (var command = new NpgsqlCommand("INSERT INTO \"Стоянка\".\"Auto\"( \"Марка\", \"Цвет\", \"Тип\", \"Госномер\", \"Год\") VALUES (@Марка, @Цвет, @Тип, @Госномер, @Год);", connection))
                {
                    command.Parameters.AddWithValue("Марка", auto.Марка);
                    command.Parameters.AddWithValue("Цвет", auto.Цвет);
                    command.Parameters.AddWithValue("Тип", auto.Тип);
                    command.Parameters.AddWithValue("Госномер", auto.Госномер);
                    command.Parameters.AddWithValue("Год", auto.Год);

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
                await using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await using (var command = new NpgsqlCommand("delete from \"Стоянка\".\"Sales\" where \"Код_клиента\" = (select \"Код_клиента\" from \"Стоянка\".\"Klients\" where \"Код_авто\" = @id)", connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", id);
                            await command.ExecuteNonQueryAsync();
                        }

                        await using (var command = new NpgsqlCommand("delete from \"Стоянка\".\"Realisation\" where \"Код_клиента\" = (select \"Код_клиента\" from \"Стоянка\".\"Klients\" where \"Код_авто\" = @id )", connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", id);
                            await command.ExecuteNonQueryAsync();
                        }

                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Klients\" WHERE Код_авто = @id;", connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", id);
                            await command.ExecuteNonQueryAsync();
                        }

                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Auto\" WHERE Код_авто = @id;", connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", id);
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

        [HttpDelete]
        [Route("Delete_All")]
        public async Task<IActionResult> DeleteAll()
        {
            string query = "ALTER SEQUENCE \"Стоянка\".\"Auto_Code_auto_seq\" RESTART WITH 0";
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await using (var command = new NpgsqlCommand("delete from \"Стоянка\".\"Sales\"", connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }

                        await using (var command = new NpgsqlCommand("delete from \"Стоянка\".\"Realisation\"", connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }

                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Klients\"", connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }

                        await using (var command = new NpgsqlCommand("DELETE FROM \"Стоянка\".\"Auto\"", connection, transaction))
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

