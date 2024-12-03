using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBank.Domain.Dtos;
using WorkBank.Infrostructure.Persistence.Database.Interfaces;

namespace BlacklistVerificationService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlackListController : ControllerBase
    {
        /// <summary>
        /// ������
        /// </summary>
        private readonly ILogger<BlackListController> _logger;

        /// <summary>
        /// �������� ���� ������
        /// </summary>
        private readonly IApplicationDbContext _applicationDbContext;

        public BlackListController(ILogger<BlackListController> logger, IApplicationDbContext applicationDbContext)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
        }

        /// <summary>
        /// �������� ������������ �� ������� ������
        /// </summary>
        /// <param name="passportDto">���������� ������</param>
        [HttpGet]
        [Route("check-blacklist")]
        public IActionResult Get([FromQuery]PassportDto passportDto)
        {
            var person = _applicationDbContext.Persons
                .Include(p => p.Passport)
                .Where(p => p.Passport.Serie == passportDto.Serie && p.Passport.Number == passportDto.Number)
                .FirstOrDefault();

            if (person is not null)
                return Ok(person.IsBlocked);

            return BadRequest("������������ �� ������");
        }
    }
}
