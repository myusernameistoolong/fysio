using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Domain;
using Core.DomainServices;

namespace Core.Services
{
    public class DossierServices : IDossierServices
    {
        private readonly IDossierRepository _dossierRepository;
        private readonly IPhysiotherapistRepository _physiotherapistRepository;

        public DossierServices(IDossierRepository dossierRepository,
            IPhysiotherapistRepository physiotherapistRepository)
        {
            _dossierRepository = dossierRepository;
            _physiotherapistRepository = physiotherapistRepository;
        }
        public async Task AddDossier(Dossier dossier, ApplicationUser user)
        {
            Validation(dossier, user);
            await _dossierRepository.AddDossier(dossier);
        }

        public async Task EditDossier(Dossier dossier, ApplicationUser user)
        {
            Validation(dossier, user);
            await _dossierRepository.EditDossier(dossier);
        }

        //Note
        public async Task AddNote(Note note)
        {
            await _dossierRepository.AddNote(note);
        }
        public async Task EditNote(Note note)
        {
            await _dossierRepository.EditNote(note);
        }

        public void Validation(Dossier dossier, ApplicationUser user)
        {
            if (_dossierRepository.GetByPatientIdWithinDate(dossier.PatientId) != null)
                throw new InvalidOperationException("A dossier for this user already exists");
            if (_physiotherapistRepository.GetByUserId(user.Id).StudentNr != null && dossier.IntakeUnderSuperVisionBy == null && _physiotherapistRepository.GetById((int)dossier.IntakeUnderSuperVisionBy).BigNr != null)
                throw new InvalidOperationException("Students are required to enter their supervisor");
            if (dossier.IntakeUnderSuperVisionBy != null && _physiotherapistRepository.GetById((int)dossier.IntakeUnderSuperVisionBy).BigNr == null)
                throw new InvalidOperationException("The intake supervisor is a student");
            if (_physiotherapistRepository.GetById(dossier.HeadPractitioner).BigNr == null)
                throw new InvalidOperationException("The head practitioner is a student");
        }
    }
}
