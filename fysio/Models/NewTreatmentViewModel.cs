using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;

namespace fysio.Models
{
    public class NewTreatmentViewModel
    {
        [Required]
        public string Type { get; set; }
        public string Desc { get; set; }
        public string Location { get; set; }
        public string Specialities { get; set; }
        [Required]
        public int PerformedBy { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm}")]
        public DateTime StartDate { get; set; }
        [Required]
        public int DossierId { get; set; }
    }
}
