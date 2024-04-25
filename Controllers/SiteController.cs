using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Data.SqlClient;
using WebApplication2.DB;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/Site")]
    public class SiteController : Controller
    {
        private readonly IConfiguration _databaseService;

        public SiteController(IConfiguration configuration)
        {
            _databaseService = configuration;
        }

        [HttpGet]
        [Route("Check-login")]
        public async Task<IActionResult> GetLogin(string columnValue)
        {
            // Создаем список для хранения результатов
            var result = new List<AutoClientDto>();

            // Устанавливаем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Создаем команду для выборки данных из базы данных
                using (var command = new NpgsqlCommand($"SELECT k.\"ФИО\",k.\"Почта\",k.\"Логин\",k.\"Пароль\", a.\"Марка\",a.\"Цвет\",a.\"Тип\",a.\"Госномер\",a.\"Год\" FROM \"Стоянка\".\"Klients\" as k JOIN \"Стоянка\".\"Auto\" as a ON k.\"Код_авто\" = a.\"Код_авто\" WHERE \"Логин\" = @columnValue;", connection))
                {
                    // Добавляем параметр в команду
                    command.Parameters.AddWithValue("@columnValue", columnValue);

                    // Выполняем команду и получаем данные
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Создаем объект AutoClientDto и заполняем его данными из базы данных
                            var auto = new AutoClientDto
                            {
                                FIO = await reader.GetFieldValueAsync<string>(0),
                                Email = await reader.GetFieldValueAsync<string>(1),
                                Login = await reader.GetFieldValueAsync<string>(2),
                                Password = await reader.GetFieldValueAsync<string>(3),
                                Mark = await reader.GetFieldValueAsync<string>(4),
                                Color = await reader.GetFieldValueAsync<string>(5),
                                Type = await reader.GetFieldValueAsync<string>(6),
                                GovernmentNumber = await reader.GetFieldValueAsync<string>(7),
                                Year = await reader.GetFieldValueAsync<int>(8)
                            };

                            // Добавляем объект в список результатов
                            result.Add(auto);
                        }
                    }
                }
            }

            // Возвращаем результат в формате Json
            return Ok(result);
        }

        [HttpGet]
        [Route("Checkusername")]
        public async Task<IActionResult> GetUsername(string columnValue)
        {
            // Создаем список для хранения результатов
            var result = new List<Klients>();

            // Устанавливаем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Создаем команду для выборки данных из базы данных
                using (var command = new NpgsqlCommand($"SELECT * FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @columnValue;", connection))
                {
                    // Добавляем параметр в команду
                    command.Parameters.AddWithValue("@columnValue", columnValue);

                    // Выполняем команду и получаем данные
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Создаем объект Klients и заполняем его данными из базы данных
                            var klients = new Klients
                            {
                                Код_клиента = await reader.GetFieldValueAsync<int>(0),
                                ФИО = await reader.GetFieldValueAsync<string>(1),
                                Дата_рождения = await reader.GetFieldValueAsync<DateTime>(2),
                                Почта = await reader.GetFieldValueAsync<string>(3),
                                Логин = await reader.GetFieldValueAsync<string>(4),
                                Пароль = await reader.GetFieldValueAsync<string>(5),
                                Код_авто = await reader.GetFieldValueAsync<int>(6)
                            };

                            // Добавляем объект в список результатов
                            result.Add(klients);
                        }
                    }
                }
            }

            // Возвращаем результат в формате Json
            return Ok(result);
        }

        [HttpGet]
        [Route("Checkpassword")]
        public async Task<IActionResult> GetPassword(string columnValue)
        {
            // Создаем список для хранения результатов
            var result = new List<Klients>();

            // Устанавливаем соединение с базой данных
            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Создаем команду для выборки данных из базы данных
                using (var command = new NpgsqlCommand($"SELECT * FROM \"Стоянка\".\"Klients\" WHERE \"Пароль\" = @columnValue;", connection))
                {
                    // Добавляем параметр в команду
                    command.Parameters.AddWithValue("@columnValue", columnValue);

                    // Выполняем команду и получаем данные
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Создаем объект Klients и заполняем его данными из базы данных
                            var klients = new Klients
                            {
                                Код_клиента = await reader.GetFieldValueAsync<int>(0),
                                ФИО = await reader.GetFieldValueAsync<string>(1),
                                Дата_рождения = await reader.GetFieldValueAsync<DateTime>(2),
                                Почта = await reader.GetFieldValueAsync<string>(3),
                                Логин = await reader.GetFieldValueAsync<string>(4),
                                Пароль = await reader.GetFieldValueAsync<string>(5),
                                Код_авто = await reader.GetFieldValueAsync<int>(6)
                            };

                            // Добавляем объект в список результатов
                            result.Add(klients);
                        }
                    }
                }
            }

            // Возвращаем результат в формате Json
            return Ok(result);
        }



        [HttpGet] // Определяем метод как GET-запрос
        [Route("Search_klient")] // Создаем маршрут для запроса
        public async Task<IActionResult> Get1(string columnValue)
        {
            // Инициализируем список для хранения результатов запроса
            var result = new List<Realisation>();

            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Ожидаем открытия соединения с базой данных
                await connection.OpenAsync();

                // Создаем SQL-запрос
                string sql = "SELECT * from \"Стоянка\".\"Realisation\" WHERE \"Код_клиента\" = (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @columnValue);";

                // Создаем команду для выполнения SQL-запроса
                await using (var command = new NpgsqlCommand(sql, connection))
                {
                    // Добавляем параметр для запроса
                    command.Parameters.AddWithValue("@columnValue", columnValue);

                    // Выполняем запрос и получаем результат
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Цикл для чтения строк из результата запроса
                        while (await reader.ReadAsync())
                        {
                            // Создаем объект Realisation и заполняем его данными из текущей строки результата запроса
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

                            // Добавляем объект Realisation в список результатов
                            result.Add(realisation);
                        }
                    }
                }
            }

            // Возвращаем результат в формате JSON
            return Ok(result);
        }

        [HttpPost] // Определяем метод как POST-запрос
        public async Task<IActionResult> Post([FromBody] КлиентАвто клиентАвто)
        {
            // Проверяем корректность полученных данных
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Хэшируем пароль перед сохранением в базу данных
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(клиентАвто.Пароль);

            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Ожидаем открытия соединения с базой данных
                await connection.OpenAsync();

                // Создаем SQL-запрос для вставки данных в таблицы Auto и Klients
                await using (var command = new NpgsqlCommand(@"WITH new_auto AS (
                                                      INSERT INTO ""Стоянка"".""Auto""( ""Марка"", ""Цвет"", ""Тип"", ""Госномер"", ""Год"")
                                                      VALUES (@Марка, @Цвет, @Тип, @Госномер, @Год)
                                                      RETURNING ""Код_авто"")
                                                  INSERT INTO ""Стоянка"".""Klients""( ""Логин"", ""Пароль"", ""ФИО"", ""Дата_рождения"", ""Почта"", ""Код_авто"")
                                                  VALUES (@Логин, @Пароль, @ФИО, @Дата_рождения, @Почта, (SELECT ""Код_авто"" FROM new_auto))
                                                  RETURNING *;", connection))
                {
                    // Добавляем параметры для запроса
                    command.Parameters.AddWithValue("Марка", клиентАвто.Марка);
                    command.Parameters.AddWithValue("Цвет", клиентАвто.Цвет);
                    command.Parameters.AddWithValue("Тип", клиентАвто.Тип);
                    command.Parameters.AddWithValue("Госномер", клиентАвто.Госномер);
                    command.Parameters.AddWithValue("Год", клиентАвто.Год);
                    command.Parameters.AddWithValue("ФИО", клиентАвто.ФИО);
                    command.Parameters.AddWithValue("Дата_рождения", клиентАвто.Дата_рождения);
                    command.Parameters.AddWithValue("Почта", клиентАвто.Почта);
                    command.Parameters.AddWithValue("Логин", клиентАвто.Логин);
                    command.Parameters.AddWithValue("Пароль", hashedPassword);
                    command.Parameters.AddWithValue("Код_авто", клиентАвто.Код_авто);

                    // Выполняем запрос и получаем результат
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    // Проверяем, был ли запрос выполнен успешно
                    if (rowsAffected == 1)
                    {
                        return Ok(); // Возвращаем код 200 OK, если запрос выполнен успешно
                    }
                    else
                    {
                        return BadRequest(ModelState); // Возвращаем код 400 Bad Request, если запрос не был выполнен успешно
                    }
                }
            }
        }


        [HttpPut] // Определяем метод как PUT-запрос
        [Route("Update_klient")] // Определяем маршрут запроса
        public async Task<IActionResult> Update([FromBody] UpdateKlient request)
        {
            // Проверяем корректность полученных данных
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Ожидаем открытия соединения с базой данных
                await connection.OpenAsync();

                // Начинаем транзакцию
                await using (var transaction = await connection.BeginTransactionAsync())
                {
                    // Проверяем, не используется ли уже новый логин
                    string checkNewLoginSql = "SELECT \"Логин\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @newLogin;";
                    await using (var checkNewLoginCommand = new NpgsqlCommand(checkNewLoginSql, connection))
                    {
                        checkNewLoginCommand.Parameters.AddWithValue("@newLogin", request.NewLogin);
                        using (var checkNewLoginReader = await checkNewLoginCommand.ExecuteReaderAsync())
                        {
                            if (await checkNewLoginReader.ReadAsync())
                            {
                                return BadRequest(new { success = false, message = "New login is already in use" });
                            }
                        }
                    }

                    // Обновляем данные в таблице Klients
                    string updateSql = "UPDATE \"Стоянка\".\"Klients\" SET \"ФИО\" = @fio, \"Почта\" = @email, \"Логин\" = @newLogin WHERE \"Логин\" = @oldLogin;";
                    await using (var updateCommand = new NpgsqlCommand(updateSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@fio", request.FIO);
                        updateCommand.Parameters.AddWithValue("@email", request.Email);
                        updateCommand.Parameters.AddWithValue("@newLogin", request.NewLogin);
                        updateCommand.Parameters.AddWithValue("@oldLogin", request.OldLogin);
                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                        if (rowsAffected == 0)
                        {
                            return NotFound(); // Возвращаем код 404 Not Found, если запись не найдена
                        }
                    }

                    // Commit транзакции
                    await transaction.CommitAsync();

                    return Ok(new { success = true }); // Возвращаем код 200 OK, если запрос выполнен успешно
                }
            }
        }
           




        [HttpPut] // Определяем метод как PUT-запрос
        [Route("Update_auto")] // Определяем маршрут запроса
        public async Task<IActionResult> Update2([FromBody] UpdateAuto request)
        {
            // Проверяем корректность полученных данных
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Ожидаем открытия соединения с базой данных
                await connection.OpenAsync();

                // Создаем SQL-запрос для обновления данных в таблице Auto
                string updateSql = "UPDATE \"Стоянка\".\"Auto\" SET \"Марка\" = @marka, \"Цвет\" = @color, \"Тип\" = @type, \"Госномер\" = @number, \"Год\" = @year WHERE \"Код_авто\" = (SELECT \"Код_авто\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @login);";
                await using (var updateCommand = new NpgsqlCommand(updateSql, connection))
                {
                    updateCommand.Parameters.AddWithValue("@marka", request.Марка);
                    updateCommand.Parameters.AddWithValue("@color", request.Цвет);
                    updateCommand.Parameters.AddWithValue("@type", request.Тип);
                    updateCommand.Parameters.AddWithValue("@number", request.Госномер);
                    updateCommand.Parameters.AddWithValue("@year", request.Год);
                    updateCommand.Parameters.AddWithValue("@login", request.Логин);

                    // Выполняем запрос и получаем результат
                    int rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                    // Проверяем, была ли запись обновлена
                    if (rowsAffected == 0)
                    {
                        return NotFound(); // Возвращаем код 404 Not Found, если запись не найдена
                    }
                }

                return Ok(new { success = true }); // Возвращаем код 200 OK, если запрос выполнен успешно
            }
        }
            

        [HttpGet("check-availability")] // Определяем маршрут запроса
        public async Task<IActionResult> CheckAvailability(string place)
        {
            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Ожидаем открытия соединения с базой данных
                await connection.OpenAsync();

                // Создаем SQL-запрос для проверки доступности места на стоянке
                using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Sales\" WHERE \"Место\" = @place AND \"Дата_выезда\" IS NULL", connection))
                {
                    command.Parameters.AddWithValue("place", place);

                    // Выполняем запрос и получаем результат
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Если место уже занято, возвращаем код 400 Bad Request
                        if (await reader.ReadAsync())
                        {
                            return BadRequest(new { message = "Место уже занято" });
                        }
                    }
                }
            }

            // Если место свободно, возвращаем код 200 OK
            return Ok();
        }


        [HttpGet("check-reserve")] // Определяем маршрут запроса
        public async Task<IActionResult> CheckReserve(string place)
        {
            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                // Ожидаем открытия соединения с базой данных
                await connection.OpenAsync();

                // Создаем SQL-запрос для проверки забронированности места на стоянке
                using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Realisation\" WHERE \"Место\" = @place", connection))
                {
                    command.Parameters.AddWithValue("place", place);

                    // Выполняем запрос и получаем результат
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Если место забронировано, возвращаем код 400 Bad Request
                        if (await reader.ReadAsync())
                        {
                            return BadRequest(new { message = "Место забронировано" });
                        }
                    }
                }
            }

            // Если место не забронировано, возвращаем код 200 OK
            return Ok();
        }

        [HttpPost("add-vehicle")]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleRequest request)
        {
            // Получение данных из тела запроса

            if (request.Код == 1)
            {
                // Резервирование места на стоянке сектора "А" на день
                if (request.Место[0] != 'A')
                {
                    // Если место не из сектора "А", то возвращаем ошибку
                    return BadRequest(new { message = "Резервирование применяется только к местам сектора \"А\"" });
                }

                string query = "INSERT INTO \"Стоянка\".\"Sales\" (Место, Дата_въезда, Код_клиента) VALUES (@Место, @Дата, (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @Логин))";

                // Создание подключения и команды для выполнения SQL-запроса
                await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                await using var command = new NpgsqlCommand(query, connection);

                // Открытие подключения и добавление параметров в команду
                connection.Open();
                command.Parameters.AddWithValue("@Место", request.Место);
                command.Parameters.AddWithValue("@Дата", request.Дата);
                command.Parameters.AddWithValue("@Логин", request.Логин);

                // Выполнение SQL-запроса
                await command.ExecuteNonQueryAsync();

                // Возвращение статуса 201 Created и сообщения об успешном добавлении автомобиля
                return StatusCode(201, new { message = "Автомобиль добавлен на стоянку" });
            }
            else if (request.Код == 2)
            {
                // Бронирование места на стоянке сектора "В" на месяц
                if (request.Место[0] != 'B')
                {
                    // Если место не из сектора "В", то возвращаем ошибку
                    return BadRequest(new { message = "Бронирование на месяц применяется только к местам сектора \"В\"" });
                }

                string query = "INSERT INTO \"Стоянка\".\"Realisation\" (Место, Дата_въезда, Код_клиента, Код_услуги) VALUES (@Место, @Дата, (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @Логин), 1)";

                // Создание подключения и команды для выполнения SQL-запроса
                await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                await using var command = new NpgsqlCommand(query, connection);

                // Открытие подключения и добавление параметров в команду
                connection.Open();
                command.Parameters.AddWithValue("@Место", request.Место);
                command.Parameters.AddWithValue("@Дата", request.Дата);
                command.Parameters.AddWithValue("@Логин", request.Логин);

                // Выполнение SQL-запроса
                await command.ExecuteNonQueryAsync();

                // Возвращение статуса 201 Created и сообщения об успешном добавлении бронирования на месяц
                return StatusCode(201, new { message = "Бронирование на месяц добавлено" });
            }
            else if (request.Код == 3)
            {
                // Бронирование места настоянке сектора "В" на год
                if (request.Место[0] != 'B')
                {
                    // Если место не из сектора "В", то возвращаем ошибку
                    return BadRequest(new { message = "Бронирование на год применяется только к местам сектора \"В\"" });
                }

                string query = "INSERT INTO \"Стоянка\".\"Realisation\" (Место, Дата_въезда, Код_клиента, Код_услуги) VALUES (@Место, @Дата, (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @Логин), 2)";

                // Создание подключения и команды для выполнения SQL-запроса
                await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                await using var command = new NpgsqlCommand(query, connection);

                // Открытие подключения и добавление параметров в команду
                connection.Open();
                command.Parameters.AddWithValue("@Место", request.Место);
                command.Parameters.AddWithValue("@Дата", request.Дата);
                command.Parameters.AddWithValue("@Логин", request.Логин);

                // Выполнение SQL-запроса
                await command.ExecuteNonQueryAsync();

                // Возвращение статуса 201 Created и сообщения об успешном добавлении бронирования на год
                return StatusCode(201, new { message = "Бронирование на год добавлено" });
            }
            else
            {
                // Если тип услуги неизвестен, то возвращаем ошибку
                return BadRequest(new { message = "Неверный тип услуги" });
            }
        }
    
          
        




        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {

            // Хэширование нового пароля
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Обновление записи пользователя в базе данных
            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                var query = $"UPDATE \"Стоянка\".\"Klients\" SET \"Пароль\" = @HashedPassword WHERE \"Почта\" = @Email";
                var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("HashedPassword", hashedPassword);
                command.Parameters.AddWithValue("Email", request.Email);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                {
                    // Если пароль обновлен успешно, то возвращаем статус 200 OK и объект с полем "success" со значением true
                    return Ok(new { success = true });
                }
                else
                {
                    // Если пользователь не найден или пароль не обновлен, то возвращаем статус 200 OK и объект с полем "success" со значением true
                    return Ok(new { success = true });
                }
            }
        }



        [HttpPost("check-email")]
        public async Task<IActionResult> CheckEmail([FromBody] EmailRequest emailRequest)
        {
            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {

                await connection.OpenAsync();

                string query = "SELECT * FROM \"Стоянка\".\"Klients\" WHERE \"Почта\" = @Email";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Email", emailRequest.Email);

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            // Если найден пользователь с указанным адресом электронной почты, то возвращаем статус 200 OK и объект с полем "success" со значением true
                            return Ok(new { success = true });
                        }
                        else
                        {
                            // Если пользователь с указанным адресом электронной почты не найден, то возвращаем статус 200 OK и объект с полем "success" со значением false
                            return Ok(new { success = false });
                        }
                    }
                }
            }

        }
    }
}

