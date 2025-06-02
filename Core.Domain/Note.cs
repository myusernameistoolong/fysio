using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain
{
    public class Note
    {
        public Note(string content, DateTime date, int dossierId, int createdBy, bool visibleForPatient = true)
        {
            Content = content;
            Date = date;
            DossierId = dossierId;
            CreatedBy = createdBy;
            VisibleForPatient = visibleForPatient;
        }

        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Date { get; set; }
        [Required]
        public int DossierId { get; set; }
        [Required]
        public int CreatedBy { get; set; }
        public bool VisibleForPatient { get; set; }
        [ForeignKey("DossierId")]
        public Dossier Dossier { get; set; }
        [ForeignKey("CreatedBy")]
        public Physiotherapist Physiotherapist { get; set; }
    }
}
