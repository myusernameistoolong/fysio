using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Services
{
    public interface IPhysiotherapistServices
    {
        Task AddPhysiotherapist(Physiotherapist physiotherapist);
        Task EditPhysiotherapist(Physiotherapist physiotherapist);
    }
}
