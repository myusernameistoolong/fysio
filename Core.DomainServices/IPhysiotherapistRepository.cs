using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.DomainServices
{
    public interface IPhysiotherapistRepository
    {
        IEnumerable<Physiotherapist> GetAll();
        Physiotherapist GetById(int Id);
        Task AddPhysiotherapist(Physiotherapist physiotherapist);
        Task EditPhysiotherapist(Physiotherapist physiotherapist);
        Task DeletePhysiotherapist(Physiotherapist physiotherapist);
        Physiotherapist GetByUserId(string Id);
        IEnumerable<Physiotherapist> GetAllHeadByUserId(string Id);
    }
}
