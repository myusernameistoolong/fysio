using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain
{
    public class Diagnosis
    {
        public int Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string BodyArea { get; set; }
        [Required]
        public string Pathology { get; set; }
    }
}
