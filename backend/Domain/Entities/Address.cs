using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Address
    {
        public string Street { get; private set; } = string.Empty;

        public string Number { get; private set; } = string.Empty;

        public string City { get; private set; } = string.Empty;

        public string State { get; private set; } = string.Empty;

        public string ZipCode { get; private set; } = string.Empty;

        public Address()
        {
        }

        public Address(
            string street,
            string number,
            string city,
            string state,
            string zipCode
        )
        {
            Street = street;
            Number = number;
            City = city;
            State = state;
            ZipCode = zipCode;
        }
    }
}
