using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Services
{
    public interface IPatientServices
    {
        Task AddPatient(Patient patient);
        Task EditPatient(Patient patient);
    }
}
