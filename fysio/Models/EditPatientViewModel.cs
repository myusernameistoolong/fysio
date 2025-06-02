using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using Microsoft.AspNetCore.Http;

namespace fysio.Models
{
    public class EditPatientViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
        [Required]
        public IFormFile Photo { get; set; }
        [Required]
        public DateTime Bday { get; set; }
        [Required]
        public string Sex { get; set; }
        [MinLength(11), MaxLength(11)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Big number must be numeric")]
        public string BigNr { get; set; }
        [MinLength(6), MaxLength(10)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Student number must be numeric")]
        public string StudentNr { get; set; }
    }
}
