using System;
using System.Collections.Generic;

namespace RestApi.Domain
{
    public class Person
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}