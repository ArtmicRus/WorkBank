namespace WorkBank.Domain.Models
{
    public class Person : BaseEntity
    {
        //public Person(Passport passport) 
        //{
        //    Passport = passport;
        //}

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateOnly Birthdate { get; set; }

        //public long PassportId {  get; set; }

        //public Passport Passport { get; set; }
    }
}
