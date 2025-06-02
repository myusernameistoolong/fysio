using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using Core.DomainServices;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreatmentTypesController : ControllerBase
    {
        private readonly IVektisRepository _vektisRepository;

        public TreatmentTypesController(IVektisRepository vektisRepository)
        {
            _vektisRepository = vektisRepository ?? throw new ArgumentNullException(nameof(vektisRepository));
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_vektisRepository.GetAllTreatmentTypes());
        }

        [HttpGet("{id}")]
        public ActionResult<TreatmentType> Get(int id)
        {
            var client = _vektisRepository.GetTreatmentTypeById(id);

            if (client == null)
                return NotFound();

            return Ok(client);
        }
    }
}
