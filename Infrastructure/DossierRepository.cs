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
    public class DossierRepository : IDossierRepository
    {
        private readonly ClientDbContext _context;

        public DossierRepository(ClientDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Dossier> GetAll()
        {
            return _context.Dossiers
                .Include(treatment => treatment.Patient)
                .Include(treatment => treatment.Physiotherapist)
                .Include(treatment => treatment.PhysiotherapistIntakeDoneBy)
                .Include(treatment => treatment.PhysiotherapistIntakeUnderSuperVisionBy)
                .Include(treatment => treatment.PhysiotherapistHeadPractitioner);
        }

        public Dossier GetById(int id)
        {
            return _context.Dossiers
                .Include(treatment => treatment.Patient)
                .Include(treatment => treatment.Physiotherapist)
                .Include(treatment => treatment.PhysiotherapistIntakeDoneBy)
                .Include(treatment => treatment.PhysiotherapistIntakeUnderSuperVisionBy)
                .Include(treatment => treatment.PhysiotherapistHeadPractitioner)
                .SingleOrDefault(dossier => dossier.Id == id);
        }

        public Dossier GetByUserId(string id)
        {
            return _context.Dossiers
                .Include(treatment => treatment.Patient)
                .Include(treatment => treatment.Physiotherapist)
                .Include(treatment => treatment.PhysiotherapistIntakeDoneBy)
                .Include(treatment => treatment.PhysiotherapistIntakeUnderSuperVisionBy)
                .Include(treatment => treatment.PhysiotherapistHeadPractitioner)
                .SingleOrDefault(dossier => dossier.Patient.UserId == id);
        }

        public async Task AddDossier(Dossier dossier)
        {
            _context.Dossiers.Add(dossier);
            await _context.SaveChangesAsync();
        }

        public async Task EditDossier(Dossier dossier)
        {
            _context.Dossiers.Update(dossier);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDossier(Dossier dossier)
        {
            _context.Dossiers.Remove(dossier);
            _context.Notes.RemoveRange(_context.Notes.Where(note => note.DossierId == dossier.Id));
            _context.Treatments.RemoveRange(_context.Treatments.Where(treatment => treatment.DossierId == dossier.Id));
            await _context.SaveChangesAsync();
        }

        //Note
        public Note GetNoteById(int id)
        {
            return _context.Notes.SingleOrDefault(note => note.Id == id);
        }
        public List<Note> GetAllAssociatedNotesById(int id, ClaimsPrincipal user = null)
        {
            if (user == null)
            {
                return _context.Notes
                    .Include(note => note.Physiotherapist)
                    .Where(note => note.DossierId == id).ToList();
            }
            else
            {
                if(user.IsInRole("FysioTherapist"))
                    return _context.Notes
                        .Include(note => note.Physiotherapist)
                        .Where(note => note.DossierId == id).ToList();
                else
                    return _context.Notes
                        .Include(note => note.Physiotherapist)
                        .Where(note => note.DossierId == id && note.VisibleForPatient == true).ToList();
            }
        }
        public async Task AddNote(Note note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }
        public async Task EditNote(Note note)
        {
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNote(Note note)
        {
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
        }
        public List<Treatment> GetAllAssociatedTreatmentsById(int id)
        {
            return _context.Treatments.Where(treatment => treatment.DossierId == id).ToList();
        }

        public Dossier CheckIfDossierIsRelated(int id, ApplicationUser user)
        {
            return _context.Dossiers
                .Include(treatment => treatment.Patient)
                .Include(treatment => treatment.Physiotherapist)
                .Include(treatment => treatment.PhysiotherapistIntakeDoneBy)
                .Include(treatment => treatment.PhysiotherapistIntakeUnderSuperVisionBy)
                .Include(treatment => treatment.PhysiotherapistHeadPractitioner)
                .FirstOrDefault(dossier => dossier.Patient.UserId == user.Id && dossier.Id == id);
        }

        public Dossier GetByPatientIdWithinDate(int id)
        {
            return _context.Dossiers.Where(dossier => dossier.PatientId == id && dossier.DateOfEndProcedure <= DateTime.Now).FirstOrDefault();
        }
    }
}
