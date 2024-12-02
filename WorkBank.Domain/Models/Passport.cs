﻿namespace WorkBank.Domain.Models
{
    public class Passport : BaseEntity
    {
        public string Serie { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;


        public long PersonId { get; set; }
        public Person Person { get; set; }
    }
}
