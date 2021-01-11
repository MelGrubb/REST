using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RestApi.Domain;

namespace RestApi.Controllers
{
    [ApiController]
    public class PersonOrderController : ControllerBase
    {
        private static List<Person> People { get; } = new List<Person>
        {
            new Person
            {
                Id = 1, Status = "Active", Created = DateTime.Now, Updated = DateTime.Now,
                FirstName = "Hiro", LastName = "Protagonist", EmailAddress = "deliverator@mrlees.com",
                AddressLine1 = "123 Any St.", AddressLine2 = "Apt 456", City = "Los Angeles", State = "California",
                ZipCode = "12345"
            },
            new Person
            {
                Id = 2, Status = "Active", Created = DateTime.Now, Updated = DateTime.Now,
                FirstName = "Yours", LastName = "Truly", EmailAddress = "yt@enzos.com",
                AddressLine1 = "456 Other St.", AddressLine2 = "Apt 123", City = "Los Angeles", State = "California",
                ZipCode = "12345"
            }
        };

        [HttpGet("person/{personId}/order/{category}")]
        public ActionResult<List<Order>> Get(int personId, string category)
        {
            switch (category)
            {
                case "open":
                    return People.FirstOrDefault(x => x.Id == personId).Orders
                        .Where(x => x.Status == "open")
                        .ToList();
                case "recent":
                    return People.FirstOrDefault(x => x.Id == personId).Orders
                        .OrderByDescending(x => x.OrderDate)
                        .Take(10)
                        .ToList();
                default:
                    return null;
            }
        }
    }
}