using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
    public class Physiotherapist
    {
        public Physiotherapist(string name, string lastName, string email, string phone, DateTime startTime, DateTime endTime, int? bigNr = null, int? studentNr = null)
        {
            Name = name;
            LastName = lastName;
            Email = email;
            Phone = phone;
            StartTime = startTime;
            EndTime = endTime;
            BigNr = bigNr;
            StudentNr = studentNr;
        }

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        public int? BigNr { get; set; }
        public int? StudentNr { get; set; }
        public string UserId { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:H:mm}")]
        public DateTime StartTime { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:H:mm}")]
        public DateTime EndTime { get; set; }
    }
}
