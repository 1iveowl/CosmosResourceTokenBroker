using System;
using System.Collections.Generic;
using System.Text;
using CosmosResourceToken.Core;

namespace XamarinForms.Client.Model
{
    [Preserve(AllMembers = true)]
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Person() { }

        public Person(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
