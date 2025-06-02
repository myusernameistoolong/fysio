using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Domain;
using Core.DomainServices;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class VektisRepository : IVektisRepository
    {
        private readonly ApiDbContext _context;

        public VektisRepository(ApiDbContext context)
        {
            _context = context;
        }

        public IEnumerable<TreatmentType> GetAllTreatmentTypes()
        {
            return _context.TreatmentTypes;
        }

        public TreatmentType GetTreatmentTypeById(int id)
        {
            return _context.TreatmentTypes
                .SingleOrDefault(tt => tt.Id == id);
        }

        public IEnumerable<Diagnosis> GetAllDiagnosis()
        {
            return _context.Diagnoses;
        }

        public Diagnosis GetDiagnosisById(int id)
        {
            return _context.Diagnoses
                .SingleOrDefault(d => d.Id == id);
        }
    }
}
