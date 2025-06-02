using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Services
{
    public interface ITreatmentServices
    {
        Task AddTreatment(Treatment treatment, ClaimsPrincipal userIdentity);
        Task EditTreatment(Treatment treatment, ClaimsPrincipal userIdentity);
        Task CancelTreatment(Treatment treatment, ApplicationUser user);
    }
}
