using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;

namespace fysio.Models
{
    public class EditEmail
    {
        [Required]
        public string Email { get; set; }
    }
}
