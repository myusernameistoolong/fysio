using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.DomainServices
{
    public interface ITreatmentRepository
    {
        IEnumerable<Treatment> GetAll();
        Treatment GetById(int Id);
        Task AddTreatment(Treatment treatment);
        Task EditTreatment(Treatment treatment);
        Task DeleteTreatment(Treatment treatment);
        IEnumerable<Treatment> GetAllByAssociatedUsers(ApplicationUser userId);
        IEnumerable<Treatment> GetAllByDossierId(int id);
        IEnumerable<Treatment> GetTreatmentsByPhysioTherapistId(int id, DateTime startDate, DateTime endDate, int? treatmentId = null);
    }
}
