using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using WebApplication1.DB;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/Klients")]
    [RequireHttps]
    public class KlientController : Controller
    {
        private readonly IConfiguration _databaseService;
      
        public KlientController(IConfiguration configuration)
        {
            _databaseService = configuration;
           
        }

        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> SearchClients(string columnName, string columnValue)
        {
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(columnValue))
            {
                return BadRequest("Не указаны параметры для поиска");
            }

            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Use Dapper to execute the query and get the results
            var result = await connection.QueryAsync<Klients>(
                $"SELECT * from \"Стоянка\".\"Klients\" WHERE cast({columnName} as text) ILIKE @columnValue;",
                new { columnValue = $"%{columnValue}%" }
            );

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClients()
        {
            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Use Dapper to execute the query and get the results
            var result = await connection.QueryAsync<Klients>(
                "SELECT * FROM \"Стоянка\".\"Klients\";"
            );

            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] Klients klient)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Use Dapper to execute the query and get the number of rows affected
            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE \"Стоянка\".\"Klients\" SET \"ФИО\"=@ФИО, \"Код_авто\"=@Код_авто, \"Дата_рождения\"=@Дата_рождения, \"Почта\"=@Почта,\"Логин\"=@Логин WHERE \"Код_клиента\" = @id;",
                new { id, klient.ФИО, klient.Дата_рождения, klient.Почта, klient.Логин, klient.Код_авто }
            );

            // Check if the update was successful
            if (rowsAffected == 1)
            {
                return Ok(); // Return 200 OK if the update was successful
            }
            else
            {
                return NotFound(); // Return 404 Not Found if no rows were affected
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Klients klient)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(klient.Пароль);

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Проверка на существование клиента с таким же логином
            var count = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @Логин;", new { klient.Логин });
            if (count > 0)
            {
                return BadRequest(new { error = "Клиент с таким логином уже существует" });
            }

            // Добавление нового клиента в базу данных
            var rowsAffected = await connection.ExecuteAsync(@"
            INSERT INTO ""Стоянка"".""Klients""(""ФИО"", ""Дата_рождения"", ""Почта"", ""Логин"", ""Пароль"",""Код_авто"")
            VALUES (@ФИО, @Дата_рождения, @Почта, @Логин, @Пароль,@Код_авто);",
                new
                {
                    klient.ФИО,
                    klient.Дата_рождения,
                    klient.Почта,
                    klient.Логин,
                    Пароль = hashedPassword, // Используем хешированный пароль
                    klient.Код_авто
                });

            if (rowsAffected == 1)
            {
                // Возвращаем созданного клиента, если он был успешно добавлен
                return CreatedAtAction(nameof(GetAllClients), new { id = klient.Код_клиента }, klient);
            }
            else
            {
                // Возвращаем ошибку, если клиент не был добавлен
                return BadRequest("Некорректно");
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Use Dapper to execute the delete query and get the number of rows affected
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM \"Стоянка\".\"Klients\" WHERE Код_клиента = @id;",
                new { id }
            );

            if (rowsAffected == 0)
            {
                // If no rows were affected, the client was not found, so return 404 Not Found
                return NotFound();
            }

            // If rows were affected, return 200 OK
            return Ok();
        }


        [HttpDelete]
        [Route("Delete_All")]
        public async Task<IActionResult> DeleteAllClients()
        {
            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Use Dapper to execute the delete query for all clients
            await connection.ExecuteAsync(
                "DELETE FROM \"Стоянка\".\"Klients\";"
            );
            // Use Dapper to execute the query to reset the sequence for Код_клиента
            await connection.ExecuteAsync(
                "ALTER SEQUENCE \"Стоянка\".\"Klients_Code_klient_seq\" RESTART WITH 0;"
            );

            // Return 200 OK to indicate success
            return Ok();
        }
    }
}