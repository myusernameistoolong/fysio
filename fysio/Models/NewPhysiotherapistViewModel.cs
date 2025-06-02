using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;

namespace fysio.Models
{
    public class NewPhysiotherapistViewModel
    {
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
    }
}
