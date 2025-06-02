using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;

namespace fysio.Models
{
    public class NewDossierViewModel
    {
        [Required]
        public string Desc { get; set; }
        [Required]
        public int DiagnosisCode { get; set; }
        [Required]
        public int PatientId { get; set; }
        [Required]
        public int PhysioTherapistId { get; set; }
        [Required]
        public int IntakeDoneBy { get; set; }
        public int? IntakeUnderSuperVisionBy { get; set; }
        [Required]
        public int HeadPractitioner { get; set; }
        [Required]
        public DateTime DateOfRegistration { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:H:mm}")]
        public DateTime LengthOfTreatments { get; set; }
        [Required]
        [Range(1, 100000)]
        public int AmountOfTreatments { get; set; }
    }
}
