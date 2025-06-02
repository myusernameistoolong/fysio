using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using Core.DomainServices;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class PhysiotherapistRepository : IPhysiotherapistRepository
    {
        private readonly ClientDbContext _context;

        public PhysiotherapistRepository(ClientDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Physiotherapist> GetAll()
        {
            return _context.Physiotherapists;
        }

        public Physiotherapist GetById(int id)
        {
            return _context.Physiotherapists.SingleOrDefault(physiotherapist => physiotherapist.Id == id);
        }

        public async Task AddPhysiotherapist(Physiotherapist physiotherapist)
        {
            _context.Physiotherapists.Add(physiotherapist);
            await _context.SaveChangesAsync();
        }

        public async Task EditPhysiotherapist(Physiotherapist physiotherapist)
        {
            _context.Physiotherapists.Update(physiotherapist);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePhysiotherapist(Physiotherapist physiotherapist)
        {
            _context.Physiotherapists.Remove(physiotherapist);
            await _context.SaveChangesAsync();
        }

        public Physiotherapist GetByUserId(string Id)
        {
            return _context.Physiotherapists.SingleOrDefault(physiotherapist => physiotherapist.UserId == Id);
        }

        public IEnumerable<Physiotherapist> GetAllHeadByUserId(string Id)
        {
            int? id = null;

            id = _context.Patients
                .Where(m => m.UserId == Id)
                .Select(m => m.Id)
                .SingleOrDefault();

            if (id != null && id != 0)
            {
                return _context.Dossiers
                    .Include(dossier => dossier.Patient)
                    .Where(dossier => dossier.PatientId == id && (dossier.DateOfEndProcedure == null || dossier.DateOfEndProcedure > DateTime.Now))
                    .Select(dossier => dossier.PhysiotherapistHeadPractitioner);
            }
            else
                return null;
        }
    }
}
