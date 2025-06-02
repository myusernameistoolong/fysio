using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using Core.DomainServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public class TreatmentRepository : ITreatmentRepository
    {
        private readonly ClientDbContext _context;
        private readonly ILogger<TreatmentRepository> _logger;

        public TreatmentRepository(ClientDbContext context, ILogger<TreatmentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public IEnumerable<Treatment> GetAll()
        {
            return _context.Treatments
                .Include(treatment => treatment.Physiotherapist)
                .Include(treatment => treatment.Dossier.Patient);
        }

        public Treatment GetById(int id)
        {
            return _context.Treatments
                .Include(treatment => treatment.Physiotherapist)
                .Include(treatment => treatment.Dossier.Patient)
                .SingleOrDefault(treatment => treatment.Id == id);
        }

        public async Task AddTreatment(Treatment treatment)
        {
            _context.Treatments.Add(treatment);
            await _context.SaveChangesAsync();
        }

        public async Task EditTreatment(Treatment treatment)
        {
            _context.Treatments.Update(treatment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTreatment(Treatment treatment)
        {
            _context.Treatments.Remove(treatment);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<Treatment> GetAllByAssociatedUsers(ApplicationUser user)
        {
            int? id = null;

            id = _context.Patients
                .Where(m => m.UserId == user.Id)
                .Select(m => m.Id)
                .SingleOrDefault();

            if(id == null || id == 0)
            {
                id = _context.Physiotherapists
                    .Where(m => m.UserId == user.Id)
                    .Select(m => m.Id)
                    .SingleOrDefault();

                return _context.Treatments
                    .Include(treatment => treatment.Physiotherapist)
                    .Include(treatment => treatment.Dossier.Patient)
                    .Where(treatment => treatment.PerformedBy == id);
            }
            else
            {
                return _context.Treatments
                    .Include(treatment => treatment.Physiotherapist)
                    .Include(treatment => treatment.Dossier.Patient)
                    .Where(treatment => treatment.Dossier.PatientId == id);
            }
        }

        public IEnumerable<Treatment> GetAllByDossierId(int id)
        {
            return _context.Treatments
                .Include(treatment => treatment.Dossier)
                .Where(treatment => treatment.Dossier.Id == id && treatment.EndDate != DateTime.MinValue);
        }

        public IEnumerable<Treatment> GetTreatmentsByPhysioTherapistId(int id, DateTime startDate, DateTime endDate, int? treatmentId = null)
        {
            return _context.Treatments
                 .Where(treatment => treatment.PerformedBy == id && treatment.Id != treatmentId && treatment.EndDate != DateTime.MinValue && treatment.StartDate < endDate && treatment.EndDate > startDate);
        }
    }
}
