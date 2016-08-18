using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConcurrentcyApp.Model
{
    public class TestCreateScript
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [StringLength(18, MinimumLength = 2)]
        public string Name { get; set; }
    }
}
