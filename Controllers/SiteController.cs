using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication1.DB;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/Site")]
    [RequireHttps]
    public class SiteController : Controller
    {
        private readonly IConfiguration _databaseService;

        public SiteController(IConfiguration configuration)
        {
            _databaseService = configuration;
        }

        [HttpGet]
        [Route("Check-login")]
        public async Task<IActionResult> GetAutoClientByLogin(string columnValue)
        {
            if (string.IsNullOrWhiteSpace(columnValue))
            {
                return BadRequest("Значение не может быть пустым");
            }

            columnValue = columnValue.Trim();

            var result = new List<AutoClientDto>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction();
                try
                {
                    using (var command = new NpgsqlCommand($"SELECT k.\"ФИО\",k.\"Почта\",k.\"Логин\",k.\"Пароль\", a.\"Марка\",a.\"Цвет\",a.\"Тип\",a.\"Госномер\",a.\"Год\" FROM \"Стоянка\".\"Klients\" as k JOIN \"Стоянка\".\"Auto\" as a ON k.\"Код_авто\" = a.\"Код_авто\" WHERE \"Логин\" = @columnValue;", connection))
                    {
                        command.Parameters.AddWithValue("@columnValue", columnValue);

                        await using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
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

                            result.Add(auto);
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

            return Ok(result);
        }

        [HttpGet]
        [Route("Checkusername")]
        public IActionResult GetKlientsByUsername(string columnValue)
        {
            if (string.IsNullOrWhiteSpace(columnValue))
            {
                return BadRequest("Значение не может быть пустым");
            }

            columnValue = columnValue.Trim();

            var result = new List<Klients>();

            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                using var transaction = connection.BeginTransaction();
                try
                {
                    using (var command = new NpgsqlCommand($"SELECT * FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @columnValue;", connection))
                    {
                        command.Parameters.AddWithValue("@columnValue", columnValue);

                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var klients = new Klients
                            {
                                Код_клиента = reader.GetFieldValue<int>(0),
                                ФИО = reader.GetFieldValue<string>(1),
                                Дата_рождения = reader.GetFieldValue<DateTime>(2),
                                Почта = reader.GetFieldValue<string>(3),
                                Логин = reader.GetFieldValue<string>(4),
                                Пароль = reader.GetFieldValue<string>(5),
                                Код_авто = reader.GetFieldValue<int>(6)
                            };

                            result.Add(klients);
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

            return Ok(result);
        }

        [HttpGet]
        [Route("Checkpassword")]
        public IActionResult GetKlientsByPassword(string columnValue)
        {
            if (string.IsNullOrWhiteSpace(columnValue))
            {
                return BadRequest("Значение не может быть пустым");
            }

            columnValue = columnValue.Trim();

            var result = new List<Klients>();

            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                using var transaction = connection.BeginTransaction();
                try
                {
                    using (var command = new NpgsqlCommand($"SELECT * FROM \"Стоянка\".\"Klients\" WHERE \"Пароль\" = @columnValue;", connection))
                    {
                        command.Parameters.AddWithValue("@columnValue", columnValue);

                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var klients = new Klients
                            {
                                Код_клиента = reader.GetFieldValue<int>(0),
                                ФИО = reader.GetFieldValue<string>(1),
                                Дата_рождения = reader.GetFieldValue<DateTime>(2),
                                Почта = reader.GetFieldValue<string>(3),
                                Логин = reader.GetFieldValue<string>(4),
                                Пароль = reader.GetFieldValue<string>(5),
                                Код_авто = reader.GetFieldValue<int>(6)
                            };

                            result.Add(klients);
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

            return Ok(result);
        }



        [HttpGet]
        [Route("Search_klient")]
        public IActionResult GetRealisationByClientLogin(string columnValue)
        {
            if (string.IsNullOrWhiteSpace(columnValue))
            {
                return BadRequest("Значение не может быть пустым");
            }

            columnValue = columnValue.Trim();

            var result = new List<Realisation>();

            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                using var transaction = connection.BeginTransaction();
                try
                {
                    string sql = "SELECT * from \"Стоянка\".\"Realisation\" WHERE \"Код_клиента\" = (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @columnValue);";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@columnValue", columnValue);

                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var realisation = new Realisation
                            {
                                Код = reader.GetFieldValue<int>(0),
                                Дата_въезда = reader.GetFieldValue<DateTime>(1),
                                Место = reader.GetFieldValue<string>(2),
                                Код_услуги = reader.GetFieldValue<int>(3),
                                Название_услуги = reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                                Код_клиента = reader.GetFieldValue<int>(5),
                                ФИО = reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                                Госномер = reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                                Стоимость = reader.IsDBNull(8) ? null : reader.GetFieldValue<int>(8),
                                Сумма = reader.IsDBNull(9) ? null : reader.GetFieldValue<int>(9)
                            };

                            result.Add(realisation);
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

            return Ok(result);
        }

        [HttpPost]
        [Route("AddClientAndCar")]

        public IActionResult AddClientAndCar(КлиентАвто клиентАвто)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(клиентАвто.Пароль);

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                string sql = @"WITH new_auto AS (
                                 INSERT INTO ""Стоянка"".""Auto""( ""Марка"", ""Цвет"", ""Тип"", ""Госномер"", ""Год"")
                                 VALUES (@Марка, @Цвет, @Тип, @Госномер, @Год)
                                 RETURNING ""Код_авто"")
                             INSERT INTO ""Стоянка"".""Klients""( ""Логин"", ""Пароль"", ""ФИО"", ""Дата_рождения"", ""Почта"", ""Код_авто"")
                             VALUES (@Логин, @Пароль, @ФИО, @Дата_рождения, @Почта, (SELECT ""Код_авто"" FROM new_auto))
                             RETURNING *;";

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("Марка", клиентАвто.Марка);
                command.Parameters.AddWithValue("Цвет", клиентАвто.Цвет);
                command.Parameters.AddWithValue("Тип", клиентАвто.Тип);
                command.Parameters.AddWithValue("Госномер", клиентАвто.Госномер);
                command.Parameters.AddWithValue("Год", клиентАвто.Год);
                command.Parameters.AddWithValue("Логин", клиентАвто.Логин);
                command.Parameters.AddWithValue("Пароль", hashedPassword);
                command.Parameters.AddWithValue("ФИО", клиентАвто.ФИО);
                command.Parameters.AddWithValue("Дата_рождения", клиентАвто.Дата_рождения);
                command.Parameters.AddWithValue("Почта", клиентАвто.Почта);

                var result = command.ExecuteScalar();

                if (result != null)
                {
                    transaction.Commit();
                    return Ok();
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                // Log the exception here
                return StatusCode(500, "Произошла ошибка...");
            }
        }

        [HttpPut]
        [Route("Update_klient")]
        public IActionResult UpdateClient(UpdateKlient request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Проверяем, не используется ли уже новый логин
                string checkNewLoginSql = "SELECT \"Логин\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @newLogin;";
                using (var checkNewLoginCommand = new NpgsqlCommand(checkNewLoginSql, connection))
                {
                    checkNewLoginCommand.Parameters.AddWithValue("@newLogin", request.NewLogin);
                    using var checkNewLoginReader = checkNewLoginCommand.ExecuteReader();
                    if (checkNewLoginReader.Read())
                    {
                        return BadRequest(new { success = false, message = "New login is already in use" });
                    }
                }

                // Обновляем данные в таблице Klients
                string updateSql = "UPDATE \"Стоянка\".\"Klients\" SET \"ФИО\" = @fio, \"Почта\" = @email, \"Логин\" = @newLogin WHERE \"Логин\" = @oldLogin;";
                using (var updateCommand = new NpgsqlCommand(updateSql, connection))
                {
                    updateCommand.Parameters.AddWithValue("@fio", request.FIO);
                    updateCommand.Parameters.AddWithValue("@email", request.Email);
                    updateCommand.Parameters.AddWithValue("@newLogin", request.NewLogin);
                    updateCommand.Parameters.AddWithValue("@oldLogin", request.OldLogin);
                    int rowsAffected = updateCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(); // Возвращаем код 404 Not Found, если запись не найдена
                    }
                }

                // Commit транзакции
                transaction.Commit();

                return Ok(new { success = true }); // Возвращаем код 200 OK, если запрос выполнен успешно
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }



        [HttpPut]
        [Route("Update_auto")]
        public IActionResult UpdateCar(UpdateAuto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Создаем SQL-запрос для обновления данных в таблице Auto
                string updateSql = "UPDATE \"Стоянка\".\"Auto\" SET \"Марка\" = @marka, \"Цвет\" = @color, \"Тип\" = @type, \"Госномер\" = @number, \"Год\" = @year WHERE \"Код_авто\" = (SELECT \"Код_авто\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @login);";
                using (var updateCommand = new NpgsqlCommand(updateSql, connection))
                {
                    updateCommand.Parameters.AddWithValue("@marka", request.Марка);
                    updateCommand.Parameters.AddWithValue("@color", request.Цвет);
                    updateCommand.Parameters.AddWithValue("@type", request.Тип);
                    updateCommand.Parameters.AddWithValue("@number", request.Госномер);
                    updateCommand.Parameters.AddWithValue("@year", request.Год);
                    updateCommand.Parameters.AddWithValue("@login", request.Логин);

                    // Выполняем запрос и получаем результат
                    int rowsAffected = updateCommand.ExecuteNonQuery();

                    // Проверяем, была ли запись обновлена
                    if (rowsAffected == 0)
                    {
                        return NotFound(); // Возвращаем код 404 Not Found, если запись не найдена
                    }
                }

                // Commit транзакции
                transaction.Commit();

                return Ok(new { success = true }); // Возвращаем код 200 OK, если запрос выполнен успешно
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }


        [HttpGet("check-availability")]
        public IActionResult CheckParkingSpaceAvailability(string place)
        {
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Sales\" WHERE \"Место\" = @place AND \"Дата_выезда\" IS NULL", connection))
                {
                    command.Parameters.AddWithValue("place", place);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        return BadRequest(new { message = "Место уже занято" });
                    }
                }

                transaction.Commit();

                return Ok();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }


        [HttpGet("check-reserve")]
        public IActionResult CheckParkingSpaceReservation(string place)
        {
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Realisation\" WHERE \"Место\" = @place AND (\"Код_услуги\" = 1 OR \"Код_услуги\" = 2);", connection))
                {
                    command.Parameters.AddWithValue("place", place);
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        return BadRequest(new { message = "Место уже забронировано" });
                    }
                }


                transaction.Commit();

                return Ok();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        [HttpPost("add-vehicle")]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleRequest request)
        {
            // Получение данных из тела запроса

            if (request.Код == 1 || request.Код == 2 || request.Код == 3)
            {
                // Begin transaction
                await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    if (request.Код == 1)
                    {
                        // Резервирование места на стоянке сектора "А" на день
                        if (request.Место[0] != 'A')
                        {
                            // Если место не из сектора "А", то возвращаем ошибку
                            return BadRequest(new { message = "Резервирование применяется только к местам сектора \"А\"" });
                        }

                        string query = "INSERT INTO \"Стоянка\".\"Sales\" (Место, Дата_въезда, Код_клиента) VALUES (@Место, @Дата, (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @Логин))";

                        await using var command = new NpgsqlCommand(query, connection, transaction);

                        command.Parameters.AddWithValue("@Место", request.Место);
                        command.Parameters.AddWithValue("@Дата", request.Дата);
                        command.Parameters.AddWithValue("@Логин", request.Логин);

                        await command.ExecuteNonQueryAsync();
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

                        await using var command = new NpgsqlCommand(query, connection, transaction);

                        command.Parameters.AddWithValue("@Место", request.Место);
                        command.Parameters.AddWithValue("@Дата", request.Дата);
                        command.Parameters.AddWithValue("@Логин", request.Логин);

                        await command.ExecuteNonQueryAsync();
                    }
                    else if (request.Код == 3)
                    {
                        // Бронирование места на стоянке сектора "В" на год
                        if (request.Место[0] != 'B')
                        {
                            // Если место не из сектора "В", то возвращаем ошибку
                            return BadRequest(new { message = "Бронирование на год применяется только к местам сектора \"В\"" });
                        }

                        string query = "INSERT INTO \"Стоянка\".\"Realisation\" (Место, Дата_въезда, Код_клиента, Код_услуги) VALUES (@Место, @Дата, (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @Логин), 2)";

                        await using var command = new NpgsqlCommand(query, connection, transaction);

                        command.Parameters.AddWithValue("@Место", request.Место);
                        command.Parameters.AddWithValue("@Дата", request.Дата);
                        command.Parameters.AddWithValue("@Логин", request.Логин);

                        await command.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        // Если услуга не определена, то возвращаем ошибку
                        return BadRequest(new { message = "Неверный тип услуги" });
                    }

                    // Commit transaction
                    transaction.Commit();

                    return Ok();
                }
                catch (Exception)
                {
                    // If there was an error, roll back the transaction
                    transaction.Rollback();

                    // Log the error and return a bad request

                    return BadRequest(new { message = "Произошла ошибка при выполнении операции" });
                }
            }
            else
            {
                // Если услуга не определена, то возвращаем ошибку
                return BadRequest(new { message = "Неверный тип услуги" });
            }
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // Хэширование нового пароля
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Обновление записи пользователя в базе данных
            using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            // Begin transaction
            var transaction = connection.BeginTransaction();

            try
            {
                var query = "UPDATE \"Стоянка\".\"Klients\" SET \"Пароль\" = @HashedPassword WHERE \"Почта\" = @Email";
                var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("HashedPassword", hashedPassword);
                command.Parameters.AddWithValue("Email", request.Email);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                {
                    // Если пароль обновлен успешно, то совершаем commit транзакции
                    transaction.Commit();
                    // Возвращаем статус 200 OK и объект с полем "success" со значением true
                    return Ok(new { success = true });
                }
                else
                {
                    // Если пользователь не найден или пароль не обновлен, то откатываем транзакцию и возвращаем статус 404 Not Found и объект с полем "success" со значением false
                    transaction.Rollback();
                    return NotFound(new { success = false });
                }
            }
            catch (Exception)
            {
                // Если произошла ошибка, то откатываем транзакцию
                transaction.Rollback();
                // Записываем ошибку в лог
                // Возвращаем статус 500 Internal Server Error
                return StatusCode(500, new { success = false });
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