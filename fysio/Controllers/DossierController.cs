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
using System.IO;
using System.Net.Http;
using Core.Services;

namespace fysio.Controllers
{
    public class DossierController : Controller
    {
        private readonly ILogger<DossierController> _logger;
        private readonly IDossierRepository _dossierRepository;
        private readonly IDossierServices _dossierServices;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPatientRepository _patientRepository;
        private readonly IPhysiotherapistRepository _physioTherapistRepository;
        private readonly string UriString = "https://fysio000api.azurewebsites.net/api/";

        public DossierController(ILogger<DossierController> logger,
            IDossierRepository dossierRepository,
            IDossierServices dossierServices,
            IPatientRepository patientRepository,
            IPhysiotherapistRepository physioTherapistRepository,
            UserManager<ApplicationUser> userMgr)
        {
            _logger = logger;
            _dossierRepository = dossierRepository;
            _dossierServices = dossierServices;
            _patientRepository = patientRepository;
            _physioTherapistRepository = physioTherapistRepository;
            userManager = userMgr;
        }

        [Authorize(Roles = "FysioTherapist")]
        [Authorize]
        public IActionResult Index()
        {
            return View(_dossierRepository.GetAll().ToList());
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            Dossier dossier = _dossierRepository.GetById(id);
            ApplicationUser user = await userManager.GetUserAsync(User);

            if (dossier == null || (!User.IsInRole("FysioTherapist") && _dossierRepository.CheckIfDossierIsRelated(dossier.Id, user) == null))
                return RedirectToAction("Index", "Home");

            ViewBag.Notes = _dossierRepository.GetAllAssociatedNotesById(id, User);
            ViewBag.Treatments = _dossierRepository.GetAllAssociatedTreatmentsById(id);
            ViewBag.Age = PatientServices.GetAge(dossier.Patient.Bday);

            //Diagnosis
            Diagnosis diagnosis = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(UriString);

                string detailedAddress = "Diagnosis/" + dossier.DiagnosisCode;
                var responseTask = client.GetAsync(detailedAddress);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<Diagnosis>();
                    readTask.Wait();

                    diagnosis = readTask.Result;
                }
                else
                {
                    diagnosis = null;

                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            if (diagnosis == null)
                ModelState.AddModelError(string.Empty, "Could not connect to API");

            ViewBag.Diagnosis = diagnosis;
            return View(dossier);
        }

        [Authorize(Roles = "FysioTherapist")]
        [HttpGet]
        public IActionResult Create()
        {
            PrefillSelectOptions();
            return View();
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(NewDossierViewModel newDossier)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.GetUserAsync(User);
                var dossier = new Dossier(newDossier.Desc, newDossier.DiagnosisCode, newDossier.PatientId, newDossier.PhysioTherapistId, newDossier.IntakeDoneBy, newDossier.IntakeUnderSuperVisionBy, newDossier.HeadPractitioner, newDossier.DateOfRegistration, null, newDossier.LengthOfTreatments, newDossier.AmountOfTreatments);

                try
                {
                    await _dossierServices.AddDossier(dossier, user);
                }
                catch (InvalidOperationException e)
                {
                    ModelState.AddModelError("", e.Message);
                    PrefillSelectOptions();
                    return View();
                }

                return RedirectToAction("Index");
            }

            PrefillSelectOptions();
            return View();
        }

        [Authorize(Roles = "FysioTherapist")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Dossier dossier = _dossierRepository.GetById(id);
            ViewBag.Dossier = dossier;

            PrefillSelectOptions(null, dossier.PhysioTherapistId, dossier.IntakeDoneBy, dossier.IntakeUnderSuperVisionBy, dossier.HeadPractitioner, dossier.DiagnosisCode);
            return View();
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(EditDossierViewModel newDossier)
        {
            if (ModelState.IsValid)
            {
                Dossier currentDossier = _dossierRepository.GetById(newDossier.Id);

                if (currentDossier != null)
                {
                    currentDossier.Desc                 = newDossier.Desc;
                    currentDossier.DiagnosisCode        = newDossier.DiagnosisCode;
                    currentDossier.PhysioTherapistId    = newDossier.PhysioTherapistId;
                    currentDossier.IntakeDoneBy         = newDossier.IntakeDoneBy;
                    currentDossier.IntakeUnderSuperVisionBy = newDossier.IntakeUnderSuperVisionBy;
                    currentDossier.HeadPractitioner     = newDossier.HeadPractitioner;
                    currentDossier.DateOfRegistration   = newDossier.DateOfRegistration;
                    currentDossier.DateOfEndProcedure   = newDossier.DateOfEndProcedure;
                    currentDossier.LengthOfTreatments   = newDossier.LengthOfTreatments;
                    currentDossier.AmountOfTreatments   = newDossier.AmountOfTreatments;

                    ApplicationUser user = await userManager.GetUserAsync(User);

                    try
                    {
                        await _dossierServices.EditDossier(currentDossier, user);
                    }
                    catch (InvalidOperationException e)
                    {
                        ModelState.AddModelError("", e.Message);
                        PrefillSelectOptions(null, newDossier.PhysioTherapistId, newDossier.IntakeDoneBy, newDossier.IntakeUnderSuperVisionBy, newDossier.HeadPractitioner, newDossier.DiagnosisCode);
                        return Edit(newDossier.Id);
                    }

                    return RedirectToAction("Index");
                };
            }

            PrefillSelectOptions(null, newDossier.PhysioTherapistId, newDossier.IntakeDoneBy, newDossier.IntakeUnderSuperVisionBy, newDossier.HeadPractitioner, newDossier.DiagnosisCode);
            return Edit(newDossier.Id);
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            Dossier dossier = _dossierRepository.GetById(id);

            if (dossier != null)
            {
                await _dossierRepository.DeleteDossier(dossier);
                return RedirectToAction("Index");
            }
            else
                return View();
        }

        private void PrefillSelectOptions(int? patientId = null, int? physiotherapistId = null, int? IntakeDoneById = null, int? IntakeUnderSuperVisionById = null, int? HeadPractitionerId = null, int? diagnosisCode = null)
        {
            //Diagnosis
            IEnumerable<Diagnosis> diagnosis = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(UriString);

                var responseTask = client.GetAsync("Diagnosis");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<Diagnosis>>();
                    readTask.Wait();
                    diagnosis = readTask.Result;
                }
                else
                {
                    diagnosis = Enumerable.Empty<Diagnosis>();
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            if (diagnosis == null)
                ModelState.AddModelError(string.Empty, "Could not connect to API");

            List<SelectListItem> selectListItem = new List<SelectListItem>();
            selectListItem.AddRange(new SelectList(diagnosis, "Id", "Code", diagnosisCode));
            selectListItem.Insert(0, new SelectListItem { Text = "Select a diagnosis", Value = "" });
            ViewBag.DiagnosisCodes = selectListItem;

            //Patients
            selectListItem = new List<SelectListItem>();
            var patients = _patientRepository.GetAllWithoutDossierWithinDate();

            if (patientId != null)
            {
                Patient currentPatient = _patientRepository.GetById((int)patientId);
                patients = patients.Append(currentPatient);
            }

            selectListItem.AddRange(new SelectList(patients, "Id", "Name", patientId));
            selectListItem.Insert(0, new SelectListItem { Text = "Select a Patient", Value = "" });

            ViewBag.Patients = selectListItem;

            //PhysioTherapists
            selectListItem = new List<SelectListItem>();
            var physiotherapists = _physioTherapistRepository.GetAll();
            selectListItem.AddRange(new SelectList(physiotherapists, "Id", "Name", physiotherapistId));
            selectListItem.Insert(0, new SelectListItem { Text = "Select a Physiotherapist", Value = "" });

            ViewBag.PhysioTherapists = selectListItem;

            selectListItem = new List<SelectListItem>();
            selectListItem.AddRange(new SelectList(physiotherapists, "Id", "Name", IntakeDoneById));
            selectListItem.Insert(0, new SelectListItem { Text = "Select a Physiotherapist", Value = "" });
            ViewBag.PhysioTherapistsIntake = selectListItem;

            selectListItem = new List<SelectListItem>();
            selectListItem.AddRange(new SelectList(physiotherapists.Where(p => p.BigNr != null), "Id", "Name", IntakeUnderSuperVisionById));
            selectListItem.Insert(0, new SelectListItem { Text = "Select a Physiotherapist", Value = "" });
            ViewBag.PhysioTherapistsSupervisor = selectListItem;

            selectListItem = new List<SelectListItem>();
            selectListItem.AddRange(new SelectList(physiotherapists.Where(p => p.BigNr != null), "Id", "Name", HeadPractitionerId));
            selectListItem.Insert(0, new SelectListItem { Text = "Select a Physiotherapist", Value = "" });
            ViewBag.PhysioTherapistsHead = selectListItem;
        }

        //Note
        [Authorize(Roles = "FysioTherapist")]
        [HttpGet]
        public IActionResult AddNote(int id)
        {
            ViewBag.DossierId = id;
            return View();
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddNote(NewNoteViewModel newNote)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.GetUserAsync(User);
                newNote.CreatedBy = _physioTherapistRepository.GetByUserId(user.Id).Id;
                newNote.Date = DateTime.Now; 

                var note = new Note(
                    newNote.Content,
                    newNote.Date,
                    newNote.Id,
                    newNote.CreatedBy,
                    newNote.VisibleForPatient
                    );

                await _dossierRepository.AddNote(note);

                return RedirectToAction("Details", new { id = newNote.Id });
            }
            else
                return View(newNote);
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> DeleteNote(DeleteNoteViewModel deleteNoteViewModel)
        {
            Note note = _dossierRepository.GetNoteById(deleteNoteViewModel.Id);

            if (note != null)
            {
                await _dossierRepository.DeleteNote(note);
                return RedirectToAction("Details", new { id = deleteNoteViewModel.DossierId });
            }
            else
                return View("Index");
        }
    }
}
