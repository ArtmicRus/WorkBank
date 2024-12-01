using WorkBank.Domain.Dtos;

namespace Gateway.Requests
{
    public sealed class RequestFromPhysicalPerson
    {
        public PersonDto Person { get; set; }

        public PassportDto Passport { get; set; }

        public RequestDto Request { get; set; }
    }
}
