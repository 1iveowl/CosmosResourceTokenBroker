using System;
using System.Collections.Generic;
using System.Text;

namespace Console.EF.Cosmos.Model
{
    public class PersonEx : Person
    {
        public List<Movie> Movies { get; set; }
    }
}
