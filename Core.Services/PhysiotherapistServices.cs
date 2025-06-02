using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using Core.DomainServices;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class PhysiotherapistServices : IPhysiotherapistServices
    {
        private readonly IPhysiotherapistRepository _physiotherapistRepository;

        public PhysiotherapistServices(IPhysiotherapistRepository physiotherapistRepository)
        {
            _physiotherapistRepository = physiotherapistRepository;
        }

        public async Task AddPhysiotherapist(Physiotherapist physiotherapist)
        {
            Validation(physiotherapist);
            await _physiotherapistRepository.AddPhysiotherapist(physiotherapist);
        }

        public async Task EditPhysiotherapist(Physiotherapist physiotherapist)
        {
            Validation(physiotherapist);
            await _physiotherapistRepository.EditPhysiotherapist(physiotherapist);
        }

        public void Validation(Physiotherapist physiotherapist)
        {
            if (physiotherapist.StartTime >= physiotherapist.EndTime)
                throw new InvalidOperationException("End time cannot be earlier than start time");
        }
    }
}
