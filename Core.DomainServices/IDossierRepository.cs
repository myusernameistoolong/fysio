using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.DomainServices
{
    public interface IDossierRepository
    {
        IEnumerable<Dossier> GetAll();
        Dossier GetById(int Id);
        Dossier GetByUserId(string Id);
        Task AddDossier(Dossier dossier);
        Task EditDossier(Dossier dossier);
        Task DeleteDossier(Dossier dossier);
        Note GetNoteById(int Id);
        List<Note> GetAllAssociatedNotesById(int Id, ClaimsPrincipal user = null);
        Task AddNote(Note note);
        Task EditNote(Note note);
        Task DeleteNote(Note note);
        List<Treatment> GetAllAssociatedTreatmentsById(int Id);
        Dossier CheckIfDossierIsRelated(int id, ApplicationUser user);
        Dossier GetByPatientIdWithinDate(int id);
    }
}
