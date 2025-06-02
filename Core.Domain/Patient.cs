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
    public class Patient
    {
        public Patient(string name, string lastName, string email, string phone, string photo, DateTime bday, string sex, string bigNr = null, string studentNr = null)
        {
            Name = name;
            LastName = lastName;
            Email = email;
            Phone = phone;
            Photo = photo;
            Bday = bday;
            Sex = sex;
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
        [Phone]
        public string Phone { get; set; }
        [Required]
        public string Photo { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Bday { get; set; }
        [Required]
        public string Sex { get; set; }
        [MinLength(11), MaxLength(11)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Big number must be numeric")]
        public string BigNr { get; set; }
        [MinLength(6), MaxLength(10)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Student number must be numeric")]
        public string StudentNr { get; set; }
        public string UserId { get; set; }
        public virtual ICollection<Dossier> Dossiers { get; set; }
    }
}
