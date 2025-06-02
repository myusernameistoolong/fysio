using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;

namespace fysio.Models
{
    public class EditAvailabilityViewModel
    {
        [Required]
        [DisplayFormat(DataFormatString = "{0:H:mm}")]
        public DateTime StartTime { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:H:mm}")]
        public DateTime EndTime { get; set; }
    }
}
