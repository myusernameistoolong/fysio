using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.DomainServices
{
    public interface IPatientRepository
    {
        IEnumerable<Patient> GetAll();
        Patient GetById(int Id);
        Patient GetByUserId(string Id);
        Task AddPatient(Patient patient);
        Task EditPatient(Patient patient);
        Task DeletePatient(Patient patient);
        Patient GetByEmail(string email);
        IEnumerable<Patient> GetAllWithoutDossierWithinDate();
    }
}
