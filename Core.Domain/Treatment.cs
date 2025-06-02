using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
    public class Treatment
    {
        public Treatment(string type, string desc, string location, string specialities, int performedBy, DateTime startDate, DateTime endDate, int dossierId)
        {
            Type = type;
            Desc = desc;
            Location = location;
            Specialities = specialities;
            PerformedBy = performedBy;
            StartDate = startDate;
            EndDate = endDate;
            DossierId = dossierId;
        }

        public int Id { get; set; }
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
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm}")]
        public DateTime EndDate { get; set; }
        [Required]
        public int DossierId { get; set; }
        [ForeignKey("PerformedBy")]
        public Physiotherapist Physiotherapist { get; set; }
        [ForeignKey("DossierId")]
        public Dossier Dossier { get; set; }
        public DateTime? TreatmentTimeOut { get; set; }
    }
}
