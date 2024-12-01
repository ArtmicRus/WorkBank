namespace WorkBank.Domain.Models
{
    public class Person : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public bool IsBlocked {  get; set; } = false;

        public DateOnly Birthdate { get; set; }

        //public long PassportId {  get; set; }

        List<Credit> Credits { get; set; } = new List<Credit>();

        public Passport Passport { get; set; }
    }
}
