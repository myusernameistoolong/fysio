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
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser(string name, string lastName, string email)
        {
            Name = name;
            LastName = lastName;
            Email = email;
        }

        [Required]
        public string Name { get; set; }
        [Required]
        public string LastName { get; set; }
    }
}
