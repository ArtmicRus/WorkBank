using Gateway.Dtos;
using Gateway.Requests;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
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

            var person = _applicationDbContext.Persons
                .Where(p => p.Passport.Serie == request.Passport.Serie
                    && p.Passport.Number == request.Passport.Number)
                .FirstOrDefault();

            if (person is not null)
            {
                if (request.Person.FirstName != person.FirstName ||
                    request.Person.LastName != person.LastName ||
                    request.Person.Birthdate != person.Birthdate)
                {
                    errorMessage.AppendLine("��������������� ������ �� ��������� � ����������������");
                }
            }
            else
                errorMessage.AppendLine("������������ �� ��������������� � �������");

            if (request.Request.Summa < 10000 || request.Request.Summa > 10000000)
            {
                errorMessage.AppendLine("����� ������� ������ ���� � ��������� �� 10'000 �� 10'000'000");
            }

            if (request.Request.Period < 3 || request.Request.Period > 120)
            {
                errorMessage.AppendLine("������ ��������� ������� ����� ���� � ��������� �� 3 ������� �� 10 ���");
            }

            var dateOnly = request.Person.Birthdate;
            var dateTime = new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);

            if (IsAdult(dateTime))
            {
                errorMessage.AppendLine("�������� ������ ����� ������ ����� ��������� 18 ���");
            }

            var url = $"http://localhost:5010/BlackList/check-blacklist?Serie={request.Passport.Serie}&Number={request.Passport.Number}";
            var response = _httpClient.GetAsync(url);

            var resultString = response.Result.Content.ReadAsStringAsync().Result;
            if (bool.TryParse(resultString, out bool isBlocked))
            {
                if (isBlocked)
                {
                    errorMessage.AppendLine("������������ ��������� � ������ ������");
                }
            }
            else
            {
                return BadRequest("Invalid boolean value received from the other API.");
            }



            SendToRabbit(request);

            if(errorMessage.Length > 0)
                return BadRequest(errorMessage.ToString());

            return Ok("������ ������� ��������");
        }

        /// <summary>
        /// �������� ��������� � �������
        /// </summary>
        /// <param name="request"></param>
        private Task SendToRabbit(RequestFromPhysicalPerson request)
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

            return Task.CompletedTask;
        }

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
