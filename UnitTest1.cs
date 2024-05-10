using NUnit.Framework;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using WebApplication2.Controllers;
using WebApplication2.DB;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Mvc;

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

        [SetUp]
        public void SetUp()
        {
            // Arrange
            _mapper.Setup(m => m.Map(It.IsAny<object>(), It.IsAny<object>()))
               .Returns(new object());
        }

        [Test]
        public async Task Get_ReturnsOkObjectResult_WhenParametersAreValid()
        {
            // Arrange
            var controller = CreateKlientController();
            var columnName = "ФИО";
            var columnValue = "Updated John Rambo";

            // Act
            var result = await controller.Get(columnName, columnValue);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var klients = (List<Klients>)((OkObjectResult)result).Value;
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
            var result = await controller.Get(columnName, columnValue);

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
            var klient = new Klients { ФИО = "John Doe",Дата_рождения=Convert.ToDateTime("21-06-2001"), Почта = "john.doe@example.com",Логин="Not repeat",Пароль="test",Код_авто=5 };

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

        }

        [Test]
        public async Task Put_UpdatesExistingKlient_WhenModelIsValid()
        {
            // Arrange
            var controller = CreateKlientController();
            var existingKlientId = 1; // В бд не существует клиент с ID 1
            var updatedKlient = new Klients
            {
                Код_клиента = existingKlientId,
                ФИО = "Updated John Rambo",
                Дата_рождения = Convert.ToDateTime("22-07-2002"),
                Почта = "121212",
                Логин = "updatedLogin",
                Пароль = "updatedPassword",
                Код_авто = 7
            };

            // Act
            var result = await controller.Put(existingKlientId, updatedKlient);

            // Assert
            Assert.IsInstanceOf<OkResult>(result); 
        }

        [Test]
        public async Task Delete_DeletesExistingKlient()
        {
            // Arrange
            var controller = CreateKlientController();
            var existingKlientId = 31; // Предполагаем, что у нас есть существующий клиент с ID 1

            // Act
            var result = await controller.Delete(existingKlientId);

            // Assert
            Assert.IsInstanceOf<OkResult>(result);
            // Можно добавить дополнительную проверку, чтобы убедиться, что запись была фактически удалена из базы данных
        }

        private KlientController CreateKlientController()
        {
            return new KlientController(_configuration);
        }
    }
}