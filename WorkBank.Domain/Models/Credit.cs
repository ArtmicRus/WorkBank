namespace WorkBank.Domain.Models
{
    public class Credit : BaseEntity
    {
        public decimal Summa { get; set; }

        public short Period { get; set; }

        public long PersonId { get; set; }
        public Person Person { get; set; }
    }
}
