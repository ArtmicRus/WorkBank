using Microsoft.AspNetCore.Mvc;
using WorkBank.Domain.Dtos;
using WorkBank.Infrostructure.Persistence.Database.Interfaces;

namespace PassportDataService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PassportController : ControllerBase
    {
        /// <summary>
        /// ������
        /// </summary>
        private readonly ILogger<PassportController> _logger;

        /// <summary>
        /// �������� ���� ������
        /// </summary>
        private readonly IApplicationDbContext _applicationDbContext;

        public PassportController(ILogger<PassportController> logger,
            IApplicationDbContext applicationDbContext)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
        }

        /// <summary>
        /// ��������� ���������� � ������������
        /// </summary>
        /// <param name="passportDto">���������� ������</param>
        [HttpGet]
        [Route("person-information")]
        public IActionResult Get([FromQuery] PassportDto passportDto)
        {
            var person = _applicationDbContext.Persons
                .Where(p => p.Passport.Serie == passportDto.Serie 
                    && p.Passport.Number == passportDto.Number)
                .FirstOrDefault();

            if (person is not null)
            {
                PersonDto pDto = new PersonDto
                {
                    Id = person.Id,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Birthdate = person.Birthdate,
                };

                return Ok(pDto);
            }

            return BadRequest("������������ �� ������");
        }
    }
}
