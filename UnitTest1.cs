using NUnit.Framework;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Configuration;

using AutoMapper.Configuration;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DB;
using WebApplication1.Controllers;
using System.ComponentModel.DataAnnotations;

namespace TestProject3
{
    [TestFixture]
    public class Tests
    {
        private readonly IConfiguration _configuration;
        private readonly Mock<IMapper> _mapper;

        public Tests()
        {
            _configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
               .Build();
            _mapper = new Mock<IMapper>();
        }


        [Test]
        public async Task Get_ReturnsOkObjectResult_WhenParametersAreValid()
        {
            // Arrange
            var controller = CreateKlientController();
            var columnName = "ФИО";
            var columnValue = "User2";

            // Act
            var result = await controller.SearchClients(columnName, columnValue);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var klients = ((OkObjectResult)result).Value as List<Klients>;
            Assert.That(klients, Is.Not.Empty);
        }

        [Test]
        public async Task Get_ReturnsBadRequest_WhenParametersAreInvalid()
        {
            // Arrange
            var controller = CreateKlientController();
            var columnName = "";
            var columnValue = "";

            // Act
            var result = await controller.SearchClients(columnName, columnValue);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var error = (string)((BadRequestObjectResult)result).Value;
            Assert.That(error, Is.EqualTo("He укaзaны napaметры для поиска"));
        }


        [Test]
        public async Task Post_CreatesNewKlient_WhenModelIsValid()
        {
            // Arrange
            var controller = CreateKlientController();
            var klient = new Klients
            {
                ФИО = "John Doe",
                Дата_рождения = Convert.ToDateTime("21-06-2001"),
                Почта = "john.doeexample.com", // Валидная почта
                Логин = "Not repeat2222",
                Пароль = "test",
                Код_авто = 0
            };

            // Act
            var result = await controller.Post(klient);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var createdKlient = (Klients)((CreatedAtActionResult)result).Value;
            Assert.AreEqual(klient.ФИО, createdKlient.ФИО);
            Assert.AreEqual(klient.Дата_рождения, createdKlient.Дата_рождения);
            Assert.AreEqual(klient.Почта, createdKlient.Почта);
            Assert.AreEqual(klient.Логин, createdKlient.Логин);
            Assert.AreEqual(klient.Пароль, createdKlient.Пароль);
            Assert.AreEqual(klient.Код_авто, createdKlient.Код_авто);

            // Проверка валидации почты
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(klient, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(klient, context, results, true);
            Assert.IsTrue(isValid, "Модель не прошла валидацию");
        }

        [Test]
        public async Task Put_UpdatesExistingKlient_WhenModelIsValid()
        {
            // Arrange
            var controller = CreateKlientController();
            var existingKlientId = 0; // 
            var updatedKlient = new Klients
            {
                Код_клиента = existingKlientId,
                ФИО = "Updated John Rambo",
                Дата_рождения = Convert.ToDateTime("22-07-2002"),
                Почта = "mail",
                Логин = "updatedLogin",
                Пароль = "updatedPassword",
                Код_авто = 0
            };
            // Проверка валидации почты
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(updatedKlient, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(updatedKlient, context, results, true);
            Assert.IsTrue(isValid, "Модель не прошла валидацию");
            // Act
            var result = await controller.UpdateClient(existingKlientId, updatedKlient);
            // Assert
            Assert.IsInstanceOf<OkResult>(result);
           
        }

        [Test]
        public async Task Delete_DeletesExistingKlient()
        {
            // Arrange
            var controller = CreateKlientController();
            var existingKlientId = 0; 

            // Act
            var result = await controller.DeleteClient(existingKlientId);

            // Assert
            Assert.IsInstanceOf<OkResult>(result);
            
        }

        private KlientController CreateKlientController()
        {
            return new KlientController(_configuration);
        }
    }
}