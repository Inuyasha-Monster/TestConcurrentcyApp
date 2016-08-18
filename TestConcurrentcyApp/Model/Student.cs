using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConcurrentcyApp.Model
{
    public class Student
    {
        public int StudentId { get; set; }

        public string RollNumber { get; set; }

        //[System.ComponentModel.DataAnnotations.ConcurrencyCheck]
        public string FirstName { get; set; }

        [Required]
        [StringLength(18, MinimumLength = 2)]
        public string LastName { get; set; }

        [System.ComponentModel.DataAnnotations.Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
