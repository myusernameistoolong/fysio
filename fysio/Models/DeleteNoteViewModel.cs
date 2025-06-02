using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;

namespace fysio.Models
{
    public class DeleteNoteViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int DossierId { get; set; }
    }
}
