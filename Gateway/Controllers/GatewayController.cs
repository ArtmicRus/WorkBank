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
        /// <param name="logger"></param>
        /// <param name="applicationDbContext"></param>
        public GatewayController(ILogger<GatewayController> logger,
            IApplicationDbContext applicationDbContext)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
        }


        [HttpPost]
        [Route("post-request")]
        public IEnumerable<Person> Get(RequestFromPhysicalPerson request)
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

            return _applicationDbContext.Persons.ToList();
        }
    }
}
