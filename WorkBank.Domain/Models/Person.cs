namespace WorkBank.Domain.Models
{
    public class Person : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateOnly Birthdate { get; set; }
    }
}
