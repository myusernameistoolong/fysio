using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain
{
    public class Dossier
    {
        public Dossier(string desc, int diagnosisCode, int patientId, int physioTherapistId, int intakeDoneBy, int? intakeUnderSuperVisionBy, int headPractitioner, DateTime dateOfRegistration, DateTime? dateOfEndProcedure, DateTime lengthOfTreatments, int amountOfTreatments)
        {
            Desc = desc;
            DiagnosisCode = diagnosisCode;
            PatientId = patientId;
            PhysioTherapistId = physioTherapistId;
            IntakeDoneBy = intakeDoneBy;
            IntakeUnderSuperVisionBy = intakeUnderSuperVisionBy;
            HeadPractitioner = headPractitioner;
            DateOfRegistration = dateOfRegistration;
            DateOfEndProcedure = dateOfEndProcedure;
            LengthOfTreatments = lengthOfTreatments;
            AmountOfTreatments = amountOfTreatments;
        }

        public int Id { get; set; }
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
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DateOfRegistration { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DateOfEndProcedure { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:H:mm}")]
        public DateTime LengthOfTreatments { get; set; }
        [Required]
        [Range(1, 100000)]
        public int AmountOfTreatments { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }
        [ForeignKey("PhysioTherapistId")]
        public Physiotherapist Physiotherapist { get; set; }
        [ForeignKey("IntakeDoneBy")]
        public Physiotherapist PhysiotherapistIntakeDoneBy { get; set; }
        [ForeignKey("IntakeUnderSuperVisionBy")]
        public Physiotherapist PhysiotherapistIntakeUnderSuperVisionBy { get; set; }
        [ForeignKey("HeadPractitioner")]
        public Physiotherapist PhysiotherapistHeadPractitioner { get; set; }
    }
}
