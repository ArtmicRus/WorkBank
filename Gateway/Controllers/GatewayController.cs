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
        /// ������
        /// </summary>
        private readonly ILogger<GatewayController> _logger;

        /// <summary>
        /// �������� ���� ������
        /// </summary>
        private readonly IApplicationDbContext _applicationDbContext;

        /// <summary>
        /// Http ������
        /// </summary>
        private readonly HttpClient _httpClient;

        public GatewayController(ILogger<GatewayController> logger,
            IApplicationDbContext applicationDbContext,
            HttpClient httpClient)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
            _httpClient = httpClient;
        }

        /// <summary>
        /// ���������� �������
        /// </summary>
        /// <param name="request">������ �� ���������� �������</param>
        [HttpPost]
        [Route("post-request")]
        public async Task<IActionResult> Get(RequestFromPhysicalPerson request)
        {
            StringBuilder errorMessage = new StringBuilder();

            var person = await GetPeopleInformation(errorMessage, request);

            RequestValidate(errorMessage, request);

            try
            {
                await CheckBlacklist(errorMessage, request);
            }
            catch (Exception)
            {
                return BadRequest("������! �� ����� �������� ���������� � ���������� ������������ � ������ ������.");
            }

            await FinalCreditValidate(errorMessage, request, person);

            if (errorMessage.Length > 0)
                return BadRequest(errorMessage.ToString());

            Credit credit = new Credit 
            {
                Summa = request.Request.Summa,
                Period = request.Request.Period,
                PersonId = person!.Id
            };

            await _applicationDbContext.Credits.AddAsync(credit);

            await _applicationDbContext.SaveChangesAsync();

            SendToRabbit(request);

            return Ok("������ ������� ��������");
        }

        /// <summary>
        /// ��������� ���������� � ������������
        /// </summary>
        /// <param name="errorMessage">�������� ��� ������ ���� ��������� �������</param>
        /// <param name="request">������ �� ������</param>
        private async Task<PersonPassportDataDto> GetPeopleInformation(StringBuilder errorMessage, RequestFromPhysicalPerson request)
        {
            var passportDataServiceUrl = $"http://localhost:5129/Passport/person-information?Serie={request.Passport.Serie}&Number={request.Passport.Number}";
            var response = await _httpClient.GetAsync(passportDataServiceUrl);

            PersonPassportDataDto passportDataServiceResponse = null!;

            // �������� � ������ ������ �� ������
            if (response.StatusCode == HttpStatusCode.OK)
            {
                passportDataServiceResponse = await response.Content.ReadFromJsonAsync<PersonPassportDataDto>();

                if (passportDataServiceResponse is not null)
                {
                    if (request.Person.FirstName != passportDataServiceResponse.FirstName ||
                        request.Person.LastName != passportDataServiceResponse.LastName ||
                        request.Person.Birthdate != passportDataServiceResponse.Birthdate)
                    {
                        errorMessage.AppendLine("���������� ������ �� ������������� ���������� � ��� ����");
                    }
                }
                else
                    errorMessage.AppendLine("������������ �� ��������������� � �������");
            }
            else
            {
                errorMessage.AppendLine("������������ �� ��������������� � �������");
            }

            return passportDataServiceResponse!;
        }

        /// <summary>
        /// ��������� ������� �� ������
        /// </summary>
        /// <param name="errorMessage">�������� ��� ������ ���� ��������� �������</param>
        /// <param name="request">������ �� ������</param>
        private static void RequestValidate(StringBuilder errorMessage, RequestFromPhysicalPerson request)
        {
            if (request.Request.Summa < 10000 || request.Request.Summa > 10000000)
            {
                errorMessage.AppendLine("����� ������� ������ ���� � ��������� �� 10000 �� 10000000");
            }

            if (request.Request.Period < 3 || request.Request.Period > 120)
            {
                errorMessage.AppendLine("������ ��������� ������� ����� ���� � ��������� �� 3 ������� �� 10 ���");
            }

            var dateOnly = request.Person.Birthdate;
            var dateTime = new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);

            if (!IsAdult(dateTime))
            {
                errorMessage.AppendLine("�������� ������ ����� ������ ����� ��������� 18 ���");
            }
        }

        /// <summary>
        /// �������� ������������ �� ��
        /// </summary>
        /// <param name="errorMessage">�������� ��� ������ ���� ��������� �������</param>
        /// <param name="request">������ �� ������</param>
        private async Task CheckBlacklist(StringBuilder errorMessage, RequestFromPhysicalPerson request)
        {
            var blackListUrl = $"http://localhost:5010/BlackList/check-blacklist?Serie={request.Passport.Serie}&Number={request.Passport.Number}";
            var blackListResponse = await _httpClient.GetAsync(blackListUrl);

            var blackListResultString = blackListResponse.Content.ReadAsStringAsync().Result;
            if (bool.TryParse(blackListResultString, out bool isBlocked))
            {
                if (isBlocked)
                {
                    errorMessage.AppendLine("������������ ��������� � ������ ������");
                }
            }
        }

        /// <summary>
        /// �������� ��������� �������
        /// </summary>
        /// <param name="errorMessage">�������� ��� ������ ���� ��������� �������</param>
        /// <param name="request">������ �� ������</param>
        private Task FinalCreditValidate(StringBuilder errorMessage, RequestFromPhysicalPerson request, PersonPassportDataDto person)
        {
            if (person is not null)
            {
                var personCredits = _applicationDbContext.Credits.Where(c => c.PersonId == person!.Id);
                if (personCredits.Count() >= 5)
                {
                    errorMessage.AppendLine("� ��� �� ����� ���� ������ 5 ��������");
                }

                decimal creditsSum = personCredits.Sum(x => x.Summa);
                creditsSum = creditsSum + request.Request.Summa;

                if (creditsSum > 20000000)
                {
                    errorMessage.AppendLine("����� ����� �������� �� ����� ��������� 20000000");
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// �������� ��������� � �������
        /// </summary>
        /// <param name="request"></param>
        private void SendToRabbit(RequestFromPhysicalPerson request)
        {
            // �������� ����������� � �������
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // ������ ������� ���� ��� �� �������
            channel.QueueDeclare(queue: "WorkBankRabbit", // ��� �������
                         durable: false, // ����������, ������ �� ������� ����������� ����� ����������� ������� RabbitMQ
                         exclusive: false, // ���������, ����� �� ��� ������� �������������� ������� ������������� � ������� (false = �����)
                         autoDelete: false, // ���� ���� �������� ���������� � true, ������� ������������� ��������, ����� ��� ����������� ���������� �� ���. � ����� ������ false, ������, ������� ��������� �� ��� ���, ���� � ������� �� ������.
                         arguments: null); // �������������� ��������� ��� ��������� �������. ��������, ����� ������� �������� ��������� ������� ����� ��������� (TTL), ������������ ���������� ��������� � ������� � ������ ���������. � ����������� ���� �������� �� ������ (null), �������������, ������������ �������� �� ���������

            // ������������ ������� � Json ��� �������� � Rabbit ���������
            string message = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(message);

            // ���������� ���������
            channel.BasicPublish(exchange: string.Empty, // �������� ������ (Exchange). ������ ������������ ��������� �� �������� �������� ������������ �������� �������������. � ������ ������ ������� ������ �������� string.Empty, ��� ������������� ������ ���� default. ���� ����� ���������� ��������� ����� � ������� � ����� �� ������, ��� � ����� ������������� (routing key).
                                 routingKey: "WorkBankRabbit", // ���� �������������. �� ����������, ���� ������ ������ ������� ���������.
                                 basicProperties: null, // �������� ���������. ��� ����� ���� ����� ���������, ��� ���������, ������������� ���������, ��������� � �.�. � ������� ��� �������� �� ������� (null), ��� ��� ����� ������������ ����������� ��������.
                                 body: body); // ���� ���������. ����� ���������� ��������������� ���� ���������, ������� ����� ���������. � ���������� body ���������� ����������, ������� �� ������ �������� ����� RabbitMQ

            return;
        }

        /// <summary>
        /// �������� �������� ������������
        /// </summary>
        /// <param name="dateOfBirth">���� ��������</param>
        private static bool IsAdult(DateTime dateOfBirth)
        {
            // ������� ����
            DateTime today = DateTime.Today;

            // ��������� �������
            int age = today.Year - dateOfBirth.Year;

            // ���������, �� ��� �� ���� �������� � ���� ����
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            // ���������� true, ���� ������� ������ ��� ����� 18 ���
            return age >= 18;
        }
    }
}
