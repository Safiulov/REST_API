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


            var result = new List<AutoClientDto>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand($"SELECT k.\"ФИО\",k.\"Почта\",k.\"Логин\",k.\"Пароль\", a.\"Марка\",a.\"Цвет\",a.\"Тип\",a.\"Госномер\",a.\"Год\" FROM \"Стоянка\".\"Klients\" as k JOIN \"Стоянка\".\"Auto\" as a ON k.\"Код_авто\" = a.\"Код_авто\" WHERE \"Логин\" = @columnValue;", connection))
                {
                    command.Parameters.AddWithValue("@columnValue", columnValue);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
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
                }
            }

            return Ok(result);
        }


        [HttpGet]
        [Route("Checkusername")]
        public async Task<IActionResult> GetUsername(string columnValue)
        {


            var result = new List<Klients>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand($"SELECT * FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @columnValue;", connection))
                {
                    command.Parameters.AddWithValue("@columnValue", columnValue);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
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

                            result.Add(klients
                                );
                        }
                    }
                }
            }

            return Ok(result);
        }


        [HttpGet]
        [Route("Checkpassword")]
        public async Task<IActionResult> GetPassword(string columnValue)
        {


            var result = new List<Klients>();

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand($"SELECT * FROM \"Стоянка\".\"Klients\" WHERE \"Пароль\" = @columnValue;", connection))
                {
                    command.Parameters.AddWithValue("@columnValue", columnValue);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
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

                            result.Add(klients
                                );
                        }
                    }
                }
            }

            return Ok(result);
        }



        [HttpGet]
        [Route("Search_klient")]
        public async Task<IActionResult> Get1(string columnValue)
        {

            var result = new List<Realisation>();

            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                string sql = "SELECT * from \"Стоянка\".\"Realisation\" WHERE \"Код_клиента\" = (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @columnValue);";
                await using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@columnValue", columnValue);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
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

                            result.Add(realisation);
                        }
                    }
                }
            }

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] КлиентАвто клиентАвто)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(клиентАвто.Пароль);
                await using (var command = new NpgsqlCommand(@"WITH new_auto AS (
                                                        INSERT INTO ""Стоянка"".""Auto""( ""Марка"", ""Цвет"", ""Тип"", ""Госномер"", ""Год"")
                                                        VALUES (@Марка, @Цвет, @Тип, @Госномер, @Год)
                                                        RETURNING ""Код_авто"")
                                                    INSERT INTO ""Стоянка"".""Klients""( ""Логин"", ""Пароль"", ""ФИО"", ""Дата_рождения"", ""Почта"", ""Код_авто"")
                                                    VALUES (@Логин, @Пароль, @ФИО, @Дата_рождения, @Почта, (SELECT ""Код_авто"" FROM new_auto))
                                                    RETURNING *;", connection))
                {
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


        [HttpPut]
        [Route("Update_klient")]
        public async Task<IActionResult> Update([FromBody] UpdateKlient request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    await using (var transaction = await connection.BeginTransactionAsync())
                    {
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
                                return NotFound();
                            }
                        }

                        await transaction.CommitAsync();
                        return Ok(new { success = true });
                    }
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "An error occurred while updating the data" });
            }
        }




        [HttpPut]
        [Route("Update_auto")]
        public async Task<IActionResult> Update2([FromBody] UpdateAuto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    string updateSql = "UPDATE \"Стоянка\".\"Auto\" SET \"Марка\" = @marka, \"Цвет\" = @color,\"Тип\" = @type,\"Госномер\" = @number,\"Год\" = @year WHERE \"Код_авто\" = (SELECT \"Код_авто\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @login);;";
                    await using (var updateCommand = new NpgsqlCommand(updateSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@marka", request.Марка);
                        updateCommand.Parameters.AddWithValue("@color", request.Цвет);
                        updateCommand.Parameters.AddWithValue("@type", request.Тип);
                        updateCommand.Parameters.AddWithValue("@number", request.Госномер);
                        updateCommand.Parameters.AddWithValue("@year", request.Год);
                        updateCommand.Parameters.AddWithValue("@login", request.Логин);

                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                        if (rowsAffected == 0)
                        {
                            return NotFound();
                        }
                    }


                    return Ok(new { success = true });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "An error occurred while updating the data" });
            }
        }

        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckAvailability(string place)
        {
            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Sales\" WHERE \"Место\" = @place AND \"Дата_выезда\" IS NULL", connection))
                {
                    command.Parameters.AddWithValue("place", place);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return BadRequest(new { message = "Место уже занято" });
                        }
                    }
                }
            }

            return Ok();
        }


        [HttpGet("check-reserve")]
        public async Task<IActionResult> CheckReserve(string place)
        {
            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand("SELECT * FROM \"Стоянка\".\"Realisation\" WHERE \"Место\" = @place", connection))
                {
                    command.Parameters.AddWithValue("place", place);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return BadRequest(new { message = "Место забронировано" });
                        }
                    }
                }
            }

            return Ok();
        }

        [HttpPost("add-vehicle")]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleRequest request)
        {
            try
            {
                if (request.Код == 1)
                {
                    if (request.Место[0] != 'A')
                    {
                        return BadRequest(new { message = "Резервирование применяется только к местам сектора \"А\"" });
                    }
                    string query = "INSERT INTO \"Стоянка\".\"Sales\" (Место, Дата_въезда, Код_клиента) VALUES (@Место, @Дата, (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @Логин))";
                    await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                    await using var command = new NpgsqlCommand(query, connection);
                    connection.Open();
                    command.Parameters.AddWithValue("@Место", request.Место);
                    command.Parameters.AddWithValue("@Дата", request.Дата);
                    command.Parameters.AddWithValue("@Логин", request.Логин);
                    await command.ExecuteNonQueryAsync();
                    return StatusCode(201, new { message = "Автомобиль добавлен на стоянку" });
                }
                else if (request.Код == 2)
                {
                    if (request.Место[0] != 'B')
                    {
                        return BadRequest(new { message = "Бронирование на месяц применяется только к местам сектора \"В\"" });
                    }
                    string query = "INSERT INTO \"Стоянка\".\"Realisation\" (Место, Дата_въезда, Код_клиента, Код_услуги) VALUES (@Место, @Дата, (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @Логин), 1)";
                    await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                    await using var command = new NpgsqlCommand(query, connection);
                    connection.Open();
                    command.Parameters.AddWithValue("@Место", request.Место);
                    command.Parameters.AddWithValue("@Дата", request.Дата);
                    command.Parameters.AddWithValue("@Логин", request.Логин);
                    await command.ExecuteNonQueryAsync();
                    return StatusCode(201, new { message = "Бронирование на месяц добавлено" });
                }
                else if (request.Код == 3)
                {
                    if (request.Место[0] != 'B')
                    {
                        return BadRequest(new { message = "Бронирование на год применяется только к местам сектора \"В\"" });
                    }
                    string query = "INSERT INTO \"Стоянка\".\"Realisation\" (Место, Дата_въезда, Код_клиента, Код_услуги) VALUES (@Место, @Дата, (SELECT \"Код_клиента\" FROM \"Стоянка\".\"Klients\" WHERE \"Логин\" = @Логин), 2)";
                    await using var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection"));
                    await using var command = new NpgsqlCommand(query, connection);
                    connection.Open();
                    command.Parameters.AddWithValue("@Место", request.Место);
                    command.Parameters.AddWithValue("@Дата", request.Дата);
                    command.Parameters.AddWithValue("@Логин", request.Логин);
                    await command.ExecuteNonQueryAsync();
                    return StatusCode(201, new { message = "Бронирование на год добавлено" });
                }
                else
                {
                    return BadRequest(new { message = "Неверный тип услуги" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при добавлении автомобиля на стоянку" });
            }
        }






        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Hash the new password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                // Update the user's record in the database
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
                        return Ok(new ChangePasswordResponse { Success = true, Message = "Password updated successfully." });
                    }
                    else
                    {
                        return Ok(new ChangePasswordResponse { Success = false, Message = "User not found or password not updated." });
                    }
                }
            }
            catch (Exception ex)
            {



                return StatusCode(500, new ChangePasswordResponse { Success = false, Message = "An error occurred while updating the password." });
            }
        }

        public class ChangePasswordRequest
        {
            public string NewPassword { get; set; }
            public string Email { get; set; }
        }

        public class ChangePasswordResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }



        [HttpPost("check-email")]

        public async Task<IActionResult> CheckEmail([FromBody] EmailRequest emailRequest)
        {
            using (var connection = new NpgsqlConnection(_databaseService.GetConnectionString("DefaultConnection")))

            {
                try
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
                                return Ok(new { success = true });
                            }
                            else
                            {
                                return Ok(new { success = false });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Логирование ошибки
                    return StatusCode(500, new { success = false });
                }
            }
        }
    }

    public class EmailRequest
    {
        public string Email { get; set; }
    }

}
