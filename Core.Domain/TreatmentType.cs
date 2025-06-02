using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain
{
    public class TreatmentType
    {
        public int Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Desc { get; set; }
        [Required]
        public string Required { get; set; }
    }
}
