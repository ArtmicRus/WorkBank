using Microsoft.AspNetCore.Mvc;
using WorkBank.Domain.Dtos;
using WorkBank.Infrostructure.Persistence.Database.Interfaces;

namespace BlacklistVerificationService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlackListController : ControllerBase
    {
        private readonly ILogger<BlackListController> _logger;
        private readonly IApplicationDbContext _applicationDbContext;

        public BlackListController(ILogger<BlackListController> logger, IApplicationDbContext applicationDbContext)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet]
        [Route("check-blacklist")]
        public IActionResult Get([FromQuery]PassportDto passportDto)
        {
            return Ok(true);
        }
    }
}
