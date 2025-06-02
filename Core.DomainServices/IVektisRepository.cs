using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.DomainServices
{
    public interface IVektisRepository
    {
        IEnumerable<TreatmentType> GetAllTreatmentTypes();
        TreatmentType GetTreatmentTypeById(int id);
        IEnumerable<Diagnosis> GetAllDiagnosis();
        Diagnosis GetDiagnosisById(int id);
    }
}
