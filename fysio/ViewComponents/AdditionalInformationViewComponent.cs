using Core.Domain;
using Core.DomainServices;
using fysio.Controllers;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fysio.ViewComponents
{
    public class AdditionalInformationViewComponent : ViewComponent
    {
        private readonly ILogger<TreatmentController> _logger;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IPhysiotherapistRepository _physioTherapistRepository;
        private readonly IUserStore<ApplicationUser> userManager;

        public AdditionalInformationViewComponent(ILogger<TreatmentController> logger,
            ITreatmentRepository treatmentRepository,
            IPhysiotherapistRepository physioTherapistRepository,
            IUserStore<ApplicationUser> userMgr)
        {
            _logger = logger;
            _treatmentRepository = treatmentRepository;
            _physioTherapistRepository = physioTherapistRepository;
            userManager = userMgr;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int amountOfTreatments = 0;
            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name, CancellationToken.None);
            amountOfTreatments = _treatmentRepository.GetAllByAssociatedUsers(user).Where(t => t.StartDate.Date == DateTime.Today && t.EndDate != DateTime.MinValue).ToList().Count;

            ViewBag.AmountOfTreatments = amountOfTreatments;
            Physiotherapist physiotherapist = _physioTherapistRepository.GetByUserId(user.Id);
            ViewBag.StartTime = physiotherapist.StartTime;
            ViewBag.EndTime = physiotherapist.EndTime;
            return View("AdditionalInformation");
        }
    }
}
