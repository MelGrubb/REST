using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestApi.Domain;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly ILogger<PersonController> _logger;

        public PersonController(ILogger<PersonController> logger)
        {
            _logger = logger;
        }

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

        [HttpGet]
        public ActionResult<IEnumerable<Person>> Get()
        {
            var results = People.ToArray();

            if (!results.Any()) return NotFound();

            return results;
        }

        [HttpGet("{id}")]
        public ActionResult<Person> Get(int id)
        {
            var result = People.FirstOrDefault(x => x.Id == id);

            if (result == null) return NotFound();

            return result;
        }

        [HttpPost]
        public ActionResult Post(Person value)
        {
            if (!ModelState.IsValid) return BadRequest();

            value.Id = People.Max(x => x.Id + 1);
            value.Created = value.Updated = DateTime.Now;
            People.Add(value);

            return Ok();
        }

        [HttpPut("{id}")]
        public ActionResult Put(int id, Person value)
        {
            if (!ModelState.IsValid) return BadRequest();

            People.RemoveAll(x => x.Id == id);
            value.Id = id;
            value.Created = value.Updated = DateTime.Now;
            People.Add(value);

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Patch(int id, Person value)
        {
            if (!ModelState.IsValid) return BadRequest();

            var person = People.FirstOrDefault(x => x.Id == id);

            if (person == null) return NotFound();

            // Naive example.
            // Since we're updating all of the fields except Id, this is effectively the same things as a PUT.
            person.Status = value.Status;
            person.FirstName = value.FirstName;
            person.LastName = value.LastName;
            person.EmailAddress = value.EmailAddress;
            person.AddressLine1 = value.AddressLine1;
            person.AddressLine2 = value.AddressLine2;
            person.City = value.City;
            person.State = value.State;
            person.ZipCode = value.ZipCode;
            person.Updated = DateTime.Now;

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var person = People.FirstOrDefault(x => x.Id == id);

            if (person == null) return NotFound();

            People.Remove(person);

            return Ok();
        }
    }
}