namespace Gateway.Dtos
{
    public class PersonDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateOnly Birthdate { get; set; }
    }
}
