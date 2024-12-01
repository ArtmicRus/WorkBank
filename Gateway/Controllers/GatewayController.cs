using Gateway.Requests;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Net;
using System.Text;
using System.Text.Json;
using WorkBank.Domain.Dtos;
using WorkBank.Domain.Models;
using WorkBank.Infrostructure.Persistence.Database.Interfaces;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GatewayController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly ILogger<GatewayController> _logger;

        /// <summary>
        /// 
        /// </summary>
        private readonly IApplicationDbContext _applicationDbContext;

        /// <summary>
        /// 
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="applicationDbContext"></param>
        public GatewayController(ILogger<GatewayController> logger,
            IApplicationDbContext applicationDbContext,
            HttpClient httpClient)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
            _httpClient = httpClient;
        }


        [HttpPost]
        [Route("post-request")]
        public IActionResult Get(RequestFromPhysicalPerson request)
        {
            StringBuilder errorMessage = new StringBuilder();

            var passportDataServiceUrl = $"http://localhost:5129/Passport/person-information?Serie={request.Passport.Serie}&Number={request.Passport.Number}";
            var response = _httpClient.GetAsync(passportDataServiceUrl);

            if(response.Result.StatusCode == HttpStatusCode.OK)
            {
                var passportDataServiceResponse = response.Result.Content.ReadFromJsonAsync<PersonDto>().Result;

                if (passportDataServiceResponse is not null)
                {
                    if (request.Person.FirstName != passportDataServiceResponse.FirstName ||
                        request.Person.LastName != passportDataServiceResponse.LastName ||
                        request.Person.Birthdate != passportDataServiceResponse.Birthdate)
                    {
                        errorMessage.AppendLine("Паспортные данные не соответствуют информации о физ лице");
                    }
                }
                else
                    errorMessage.AppendLine("Пользователь не зарегистрирован в системе");
            }
            else
            {
                errorMessage.AppendLine("Пользователь не зарегистрирован в системе");
            }

            var person = _applicationDbContext.Persons
                .Where(p => p.Passport.Serie == request.Passport.Serie
                    && p.Passport.Number == request.Passport.Number)
                .FirstOrDefault();


            if (request.Request.Summa < 10000 || request.Request.Summa > 10000000)
            {
                errorMessage.AppendLine("Сумма запроса должна быть в диапозоне от 10000 до 10000000");
            }

            if (request.Request.Period < 3 || request.Request.Period > 120)
            {
                errorMessage.AppendLine("Период погашения кредита должн быть в диапозоне от 3 месяцев до 10 лет");
            }

            var dateOnly = request.Person.Birthdate;
            var dateTime = new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);

            if (IsAdult(dateTime))
            {
                errorMessage.AppendLine("Оформить кредит можно только лицам достигшим 18 лет");
            }

            var blackListUrl = $"http://localhost:5010/BlackList/check-blacklist?Serie={request.Passport.Serie}&Number={request.Passport.Number}";
            var blackListResponse = _httpClient.GetAsync(blackListUrl);

            var blackListResultString = blackListResponse.Result.Content.ReadAsStringAsync().Result;
            if (bool.TryParse(blackListResultString, out bool isBlocked))
            {
                if (isBlocked)
                {
                    errorMessage.AppendLine("Пользователь находится в чёрном списке");
                }
            }
            else
            {
                return BadRequest("Invalid boolean value received from the other API.");
            }

            if (person is not null)
            {
                var personCredits =_applicationDbContext.Credits.Where(c => c.PersonId == person.Id);
                if (personCredits.Count() >= 5)
                {
                    errorMessage.AppendLine("У вас не может быть больше 5 кредитов");
                }

                decimal creditsSum = personCredits.Sum(x => x.Summa);
                creditsSum = creditsSum + request.Request.Summa;

                if (creditsSum > 20000000)
                {
                    errorMessage.AppendLine("Сумма ваших кредитов не может привышать 20'000'000");
                }
            }

            //SendToRabbit(request);

            if(errorMessage.Length > 0)
                return BadRequest(errorMessage.ToString());

            Credit credit = new Credit 
            {
                Summa = request.Request.Summa,
                Period = request.Request.Period,
                PersonId = person.Id
            };

            _applicationDbContext.Credits.Add(credit);

            return Ok("Кредит успешно оформлен");
        }

        /// <summary>
        /// Отправка сообщения в кролика
        /// </summary>
        /// <param name="request"></param>
        private Task SendToRabbit(RequestFromPhysicalPerson request)
        {
            // Создание подключения к серверу
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Создаёт очередь если она не создана
            channel.QueueDeclare(queue: "WorkBankRabbit", // Имя очереди
                         durable: false, // Определяет, должна ли очередь сохраняться после перезапуска брокера RabbitMQ
                         exclusive: false, // Указывает, может ли эта очередь использоваться другими подключениями к серверу (false = может)
                         autoDelete: false, // Если этот параметр установлен в true, очередь автоматически удалится, когда все потребители отключатся от нее. В нашем случае false, значит, очередь останется до тех пор, пока её вручную не удалят.
                         arguments: null); // Дополнительные аргументы для настройки очереди. Например, можно указать политику истечения времени жизни сообщений (TTL), максимальное количество сообщений в очереди и другие параметры. В приведенном коде аргумент не указан (null), следовательно, используются значения по умолчанию

            // Сериализация запроса в Json для передачи в Rabbit сообщения
            string message = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(message);

            // Публикация сообщения
            channel.BasicPublish(exchange: string.Empty, // Название обмена (Exchange). Обмены распределяют сообщения по очередям согласно определенным правилам маршрутизации. В данном случае указано пустое значение string.Empty, что соответствует обмену типа default. Этот обмен отправляет сообщение прямо в очередь с таким же именем, как у ключа маршрутизации (routing key).
                                 routingKey: "WorkBankRabbit", // Ключ маршрутизации. Он определяет, куда именно должно попасть сообщение.
                                 basicProperties: null, // Свойства сообщения. Это могут быть такие параметры, как заголовки, идентификатор сообщения, приоритет и т.п. В примере эти свойства не указаны (null), так что будут использованы стандартные значения.
                                 body: body); // Тело сообщения. Здесь передается непосредственно само сообщение, которое нужно отправить. В переменной body содержится информация, которую вы хотите передать через RabbitMQ

            return Task.CompletedTask;
        }

        private static bool IsAdult(DateTime dateOfBirth)
        {
            // Текущая дата
            DateTime today = DateTime.Today;

            // Вычисляем возраст
            int age = today.Year - dateOfBirth.Year;

            // Проверяем, не был ли день рождения в этом году
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            // Возвращаем true, если возраст больше или равен 18 лет
            return age >= 18;
        }
    }
}
