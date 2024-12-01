using Gateway.Dtos;
using WorkBank.Domain.Models;

namespace Gateway.Requests
{
    public sealed class RequestFromPhysicalPerson
    {
        public PersonDto Person { get; set; }

        public PassportDto Passport { get; set; }

        public Request Request { get; set; }
    }
}
