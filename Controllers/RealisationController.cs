using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.DB;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/Realisation")]
    [RequireHttps]
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
            // Проверяем, что параметры для поиска не пустые
            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(columnValue))
            {
                return BadRequest("Не указаны параметры для поиска");
            }

            // Создаем подключение к базе данных PostgreSQL
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();

            // Создаем SQL-запрос для поиска записей в таблице Realisation
            string sql = $"SELECT * from \"Стоянка\".\"Realisation\" WHERE cast({columnName} as text) ilike '%' || @ColumnValue || '%';";

            // Выполняем запрос с использованием Dapper и маппим результаты напрямую в объекты Realisation
            var result = await connection.QueryAsync<Realisation>(sql, new { ColumnValue = columnValue });

            // Возвращаем код ответа 200 (OK) и список результатов поиска
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Создаем подключение к базе данных PostgreSQL
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();

            // Создаем SQL-запрос для получения всех записей из таблицы Realisation
            string sql = "SELECT * FROM \"Стоянка\".\"Realisation\";";

            // Выполняем запрос с использованием Dapper и маппим результаты напрямую в объекты Realisation
            var result = await connection.QueryAsync<Realisation>(sql);

            // Обрабатываем просроченные места
            foreach (var realisation in result)
            {
                int daysOverdue = (DateTime.Now - realisation.Дата_въезда).Days;

                if (realisation.Дата_въезда < DateTime.Now && (realisation.Код_услуги == 1 || realisation.Код_услуги == 2))
                {
                    realisation.Место = $"Просрочено место {realisation.Место} на {daysOverdue} дней";
                }
            }

            // Возвращаем код ответа 200 (OK) и список результатов запроса
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Realisation realisation)
        {
            // Проверяем, что модель данных валидна
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Создаем подключение к базе данных PostgreSQL
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();

            // Создаем SQL-запрос для обновления записи в таблице Realisation
            string sql = @"UPDATE ""Стоянка"".""Realisation"" 
                SET ""Дата_въезда""=@Дата_въезда, ""Место""=@Место, ""Код_услуги""=@Код_услуги, ""Код_клиента""=@Код_клиента 
                WHERE ""Код"" = @id;";

            // Выполняем запрос с использованием Dapper и параметризованных значений
            int rowsAffected = await connection.ExecuteAsync(sql, new
            {
                id,
                realisation.Дата_въезда,
                realisation.Место,
                realisation.Код_услуги,
                realisation.Код_клиента
            });

            // Если запись успешно обновлена, возвращаем код ответа 200 (OK)
            if (rowsAffected == 1)
            {
                return Ok();
            }
            // Если запись не найдена, возвращаем код ответа 404 (Not Found)
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Realisation realisation)
        {
            // Проверяем, что модель данных валидна
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Проверяем дополнительные условия для места и кода услуги
            if (realisation.Место.StartsWith("A") && (realisation.Код_услуги == 1 || realisation.Код_услуги == 2))
            {
                ModelState.AddModelError(string.Empty, "Ошибка: место начинается с 'A' и код_услуги равен 1 или 2");
                return BadRequest(ModelState);
            }

            if (realisation.Место.StartsWith("B") && (realisation.Код_услуги == 3))
            {
                ModelState.AddModelError(string.Empty, "Ошибка: Услуга недоступна в данном секторе");
                return BadRequest(ModelState);
            }

            // Создаем подключение к базе данных PostgreSQL
            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Открываем соединение
                await connection.OpenAsync();

                // Проверяем, есть ли уже занятое место для услуги 1 или 2 в секторе B
                if (realisation.Место.StartsWith("B") && (realisation.Код_услуги == 1 || realisation.Код_услуги == 2))
                {
                    var existingRealisation = await connection.QueryFirstOrDefaultAsync<Realisation>(
                        "SELECT * FROM \"Стоянка\".\"Realisation\" WHERE \"Место\" = @Место AND Дата_въезда >= NOW() AND (Код_услуги = 1 OR Код_услуги = 2)",
                        new { Место = realisation.Место });

                    if (existingRealisation != null)
                    {
                        ModelState.AddModelError(string.Empty, "Данное место уже занято");
                        return BadRequest(ModelState);
                    }
                }

                // Вставляем новую запись в таблицу Realisation
                int rowsAffected = await connection.ExecuteAsync(
                    "INSERT INTO \"Стоянка\".\"Realisation\"(\"Дата_въезда\", \"Место\", \"Код_услуги\", \"Код_клиента\") VALUES (@Дата_въезда, @Место, @Код_услуги, @Код_клиента);",
                    new
                    {
                        Дата_въезда = realisation.Дата_въезда.ToUniversalTime(),
                        realisation.Место,
                        realisation.Код_услуги,
                        realisation.Код_клиента
                    });

                // Если запись успешно вставлена, возвращаем код ответа 200 (OK)
                if (rowsAffected == 1)
                {
                    return Ok();
                }
                // Если запись не удалось вставить, возвращаем код ответа 400 (Bad Request)
                else
                {
                    return BadRequest(ModelState);
                }
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Создаем подключение к базе данных PostgreSQL
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            // Открываем соединение
            await connection.OpenAsync();

            // Создаем SQL-запрос для удаления записи из таблицы Realisation
            string sql = @"DELETE FROM ""Стоянка"".""Realisation"" WHERE ""Код"" = @id;";

            // Выполняем запрос с использованием Dapper и параметризованных значений
            int rowsAffected = await connection.ExecuteAsync(sql, new { id });

            // Если запись успешно удалена, возвращаем код ответа 200 (OK)
            if (rowsAffected == 1)
            {
                return Ok();
            }
            // Если запись не найдена, возвращаем код ответа 404 (Not Found)
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var connectionString = _databaseService.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Используем Dapper для выполнения запроса на удаление всех автомобилей
            await connection.ExecuteAsync(
                "DELETE FROM \"Стоянка\".\"Realisation\";"
            );

            // Используем Dapper для выполнения запроса на перезапуск последовательности идентификаторов автомобилей
            await connection.ExecuteAsync(
                "ALTER SEQUENCE \"Стоянка\".\"Realisation_Code_seq\" RESTART WITH 0;"
            );

            // Возвращаем статус 200 OK
            return Ok();
        }
    }
}