using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConcurrentcyApp.Model
{
    public class InputAccount
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(8)]
        public string Name { get; set; }

        public decimal Balance { get; set; }
    }
}
