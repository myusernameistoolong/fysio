using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Services
{
    public interface IDossierServices
    {
        Task AddDossier(Dossier dossier, ApplicationUser user);
        Task EditDossier(Dossier dossier, ApplicationUser user);
        Task AddNote(Note note);
        Task EditNote(Note note);
    }
}
