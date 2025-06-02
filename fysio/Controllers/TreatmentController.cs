using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Core.Domain;
using Infrastructure;
using Core.DomainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using fysio.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Net.Http;
using Core.Services;

namespace fysio.Controllers
{
    public class TreatmentController : Controller
    {
        private readonly ILogger<TreatmentController> _logger;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly ITreatmentServices _treatmentServices;
        private readonly IUserStore<ApplicationUser> userManager;
        private readonly IPhysiotherapistRepository _physioTherapistRepository;
        private readonly IDossierRepository _dossierRepository;
        private readonly string UriString = "https://fysio000api.azurewebsites.net/api/";

        public TreatmentController(ILogger<TreatmentController> logger,
            ITreatmentRepository treatmentRepository,
            ITreatmentServices treatmentServices,
            IUserStore<ApplicationUser> userMgr,
            IPhysiotherapistRepository physioTherapistRepository,
            IDossierRepository dossierRepository)
        {
            _logger = logger;
            _treatmentRepository = treatmentRepository;
            _treatmentServices = treatmentServices;
            userManager = userMgr;
            _physioTherapistRepository = physioTherapistRepository;
            _dossierRepository = dossierRepository;
        }

        [Authorize(Roles = "FysioTherapist")]
        public IActionResult Index()
        {
            ViewBag.PageName = "Appointments";
            return View(_treatmentRepository.GetAll().OrderBy(Treatment => Treatment.StartDate).ToList());
        }

        [Authorize]
        public async Task<IActionResult> Appointments()
        {
            ViewBag.PageName = "Your Appointments";

            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name, CancellationToken.None);
            return View("Index", _treatmentRepository.GetAllByAssociatedUsers(user).OrderBy(Treatment => Treatment.StartDate).ToList());
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            Treatment treatment = _treatmentRepository.GetById(id);
            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name, CancellationToken.None);

            if (treatment != null && (User.IsInRole("FysioTherapist") || _dossierRepository.CheckIfDossierIsRelated(treatment.DossierId, user) != null))
                return View(treatment);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CreateAsync()
        {
            await PrefillSelectOptions();
            return View();
        }

        private async Task PrefillSelectOptions(int? physiotherapistsId = null, int? dossierId = null, string treatmentType = null)
        {
            IEnumerable<TreatmentType> treatmentTypes = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(UriString);

                var responseTask = client.GetAsync("TreatmentTypes");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<TreatmentType>>();
                    readTask.Wait();
                    treatmentTypes = readTask.Result;
                }
                else
                {
                    treatmentTypes = Enumerable.Empty<TreatmentType>();
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            if (treatmentTypes == null)
                ModelState.AddModelError(string.Empty, "Could not connect to API");

            List<SelectListItem> selectListItem = new List<SelectListItem>();
            selectListItem.AddRange(new SelectList(treatmentTypes, "Id", "Code"));

            var selected = selectListItem.Where(x => x.Text == treatmentType).FirstOrDefault();
            if(selected != null)
                selected.Selected = true;

            selectListItem.Insert(0, new SelectListItem { Text = "Select a Treatment", Value = "" });
            ViewBag.Treatments = selectListItem;

            //PhysioTherapists
            selectListItem = new List<SelectListItem>();
            IEnumerable<Physiotherapist> physiotherapists;
            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name, CancellationToken.None);

            if (User.IsInRole("Patient"))
                physiotherapists = _physioTherapistRepository.GetAllHeadByUserId(user.Id);
            else
                physiotherapists = _physioTherapistRepository.GetAll();

            selectListItem.AddRange(new SelectList(physiotherapists, "Id", "Name", physiotherapistsId));
            selectListItem.Insert(0, new SelectListItem { Text = "Select a Physiotherapist", Value = "" });

            ViewBag.PhysioTherapists = selectListItem;

            //Dossiers
            selectListItem = new List<SelectListItem>();
            var dossiers = _dossierRepository.GetAll();
            selectListItem.AddRange(new SelectList(dossiers, "Id", "Patient.Name", dossierId));
            selectListItem.Insert(0, new SelectListItem { Text = "Select a Patient", Value = "" });

            ViewBag.Dossiers = selectListItem;
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(NewTreatmentViewModel newTreatment)
        {
            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name, CancellationToken.None);

            if (User.IsInRole("Patient"))
            {
                newTreatment.Type = "None";
                newTreatment.Desc = null;
                newTreatment.Location = null;
                newTreatment.Specialities = null;

                Physiotherapist physiotherapist = _physioTherapistRepository.GetAllHeadByUserId(user.Id).FirstOrDefault();

                if (physiotherapist == null)
                {
                    ModelState.AddModelError("", "This account does not have a dossier with a valid head practitioner");
                    await PrefillSelectOptions();
                    return View();
                }

                newTreatment.PerformedBy = physiotherapist.Id;
                newTreatment.DossierId = _dossierRepository.GetByUserId(user.Id).Id;
            }

            if (ModelState.IsValid)
            {
                Dossier dossier = _dossierRepository.GetById(newTreatment.DossierId);
                DateTime endDate = newTreatment.StartDate.AddHours(dossier.LengthOfTreatments.Hour);

                var treatment = new Treatment(newTreatment.Type, newTreatment.Desc, newTreatment.Location, newTreatment.Specialities, newTreatment.PerformedBy, newTreatment.StartDate, endDate, newTreatment.DossierId);

                try
                {
                    await _treatmentServices.AddTreatment(treatment, User);
                }
                catch (InvalidOperationException e)
                {
                    ModelState.AddModelError("", e.Message);
                    await PrefillSelectOptions();
                    return View();
                }

                return RedirectToAction("Appointments");
            }

            await PrefillSelectOptions();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditAsync(int id)
        {
            Treatment treatment = _treatmentRepository.GetById(id);
            ViewBag.Treatment = treatment;

            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name, CancellationToken.None);
            if (!User.IsInRole("FysioTherapist") && _dossierRepository.CheckIfDossierIsRelated(treatment.DossierId, user) == null)
                return RedirectToAction("Appointments");

            await PrefillSelectOptions(treatment.PerformedBy, treatment.DossierId, treatment.Type);

            return View();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(EditTreatmentViewModel newTreatment)
        {
            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name, CancellationToken.None);
            Treatment currentTreatment = _treatmentRepository.GetById(newTreatment.Id);

            if (User.IsInRole("Patient"))
            {
                newTreatment.Type = currentTreatment.Type;
                newTreatment.Desc = currentTreatment.Desc;
                newTreatment.Location = currentTreatment.Location;
                newTreatment.Specialities = currentTreatment.Specialities;
                newTreatment.PerformedBy = _physioTherapistRepository.GetAllHeadByUserId(user.Id).FirstOrDefault().Id;
                newTreatment.DossierId = _dossierRepository.GetByUserId(user.Id).Id;
            }

            if (ModelState.IsValid)
            {
                if (!User.IsInRole("FysioTherapist") && _dossierRepository.CheckIfDossierIsRelated(currentTreatment.DossierId, user) == null)
                    return RedirectToAction("Appointments");

                Dossier dossier = _dossierRepository.GetById(newTreatment.DossierId);
                DateTime endDate = newTreatment.StartDate.AddHours(dossier.LengthOfTreatments.Hour);

                if (currentTreatment != null)
                {
                    currentTreatment.Type           = newTreatment.Type;
                    currentTreatment.Desc           = newTreatment.Desc;
                    currentTreatment.Location       = newTreatment.Location;
                    currentTreatment.Specialities   = newTreatment.Specialities;
                    currentTreatment.PerformedBy    = newTreatment.PerformedBy;
                    currentTreatment.StartDate      = newTreatment.StartDate;
                    currentTreatment.EndDate        = endDate;
                    currentTreatment.DossierId      = newTreatment.DossierId;

                    try
                    {
                        await _treatmentServices.EditTreatment(currentTreatment, User);
                    }
                    catch (InvalidOperationException e)
                    {
                        ModelState.AddModelError("", e.Message);
                        await PrefillSelectOptions(newTreatment.PerformedBy, newTreatment.DossierId, newTreatment.Type);
                        return await EditAsync(newTreatment.Id);
                    }

                    return RedirectToAction("Appointments");
                }
            }

            return await EditAsync(newTreatment.Id);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            Treatment treatment = _treatmentRepository.GetById(id);

            if (treatment != null)
            {
                ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name, CancellationToken.None);

                try
                {
                    await _treatmentServices.CancelTreatment(treatment, user);
                }
                catch (InvalidOperationException e)
                {
                    ModelState.AddModelError("", e.Message);
                }
            }
            return await Appointments();
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            Treatment treatment = _treatmentRepository.GetById(id);

            if (treatment != null)
            {
                await _treatmentRepository.DeleteTreatment(treatment);
                return RedirectToAction("Appointments");
            }
            else
                return View();
        }
    }
}
