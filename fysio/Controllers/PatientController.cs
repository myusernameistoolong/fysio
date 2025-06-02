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
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using Core.Services;

namespace fysio.Controllers
{
    public class PatientController : Controller
    {
        private readonly ILogger<PatientController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPatientServices _patientServices;
        private readonly IPatientRepository _patientRepository;
        private readonly IUserStore<ApplicationUser> userManager;

        public PatientController(ILogger<PatientController> logger,
            IWebHostEnvironment webHostEnvironment,
            IPatientServices patientServices,
            IPatientRepository patientRepository,
             IUserStore<ApplicationUser> userMgr)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _patientServices = patientServices;
            _patientRepository = patientRepository;
            userManager = userMgr;
        }

        [Authorize(Roles = "FysioTherapist")]
        public IActionResult Index()
        {
            return View(_patientRepository.GetAll().ToList());
        }

        [Authorize(Roles = "FysioTherapist")]
        public IActionResult Details(int id)
        {
            Patient patient = _patientRepository.GetById(id);
            return View(patient);
        }

        [Authorize(Roles = "FysioTherapist")]
        [HttpGet]
        public IActionResult Create()
        {
            PrefillSelectOptions();
            return View();
        }

        private void PrefillSelectOptions(Patient patient = null)
        {
            if (patient == null)
            {
                ViewBag.Sexes = new SelectList(Enum.GetValues(typeof(Sex)), Sex.Male);
            }
            else
            {
                if(Enum.TryParse(patient.Sex, out Sex sex))
                    ViewBag.Sexes = new SelectList(Enum.GetValues(typeof(Sex)), sex);
            }
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(NewPatientViewModel newPatient)
        {
            if (ModelState.IsValid)
            {
                //Photo
                string uniqueFileName = null;

                if (!string.IsNullOrWhiteSpace(_webHostEnvironment.WebRootPath))
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "profile");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + newPatient.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        newPatient.Photo.CopyTo(fileStream);
                    }
                }
                else
                    uniqueFileName = "";

                var patient = new Patient(newPatient.Name, newPatient.LastName, newPatient.Email, newPatient.Phone, uniqueFileName, newPatient.Bday, newPatient.Sex, newPatient.BigNr, newPatient.StudentNr);
                
                try
                {
                    await _patientServices.AddPatient(patient);
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
            Patient patient = _patientRepository.GetById(id);
            ViewBag.Patient = patient;

            PrefillSelectOptions(patient);

            return View();
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(EditPatientViewModel newPatient)
        {
            if (ModelState.IsValid)
            {
                Patient currentPatient = _patientRepository.GetById(newPatient.Id);

                //Photo
                string uniqueFileName = null;

                if (!string.IsNullOrWhiteSpace(_webHostEnvironment.WebRootPath))
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "profile");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + newPatient.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        newPatient.Photo.CopyTo(fileStream);
                    }
                }
                else
                    uniqueFileName = "";

                if (currentPatient != null)
                {
                    currentPatient.Name         = newPatient.Name;
                    currentPatient.LastName     = newPatient.LastName;
                    currentPatient.Phone        = newPatient.Phone;
                    currentPatient.Photo        = uniqueFileName;
                    currentPatient.Bday         = newPatient.Bday;
                    currentPatient.Sex          = newPatient.Sex;
                    currentPatient.StudentNr    = newPatient.StudentNr;
                    currentPatient.BigNr        = newPatient.BigNr;
                    
                    try
                    {
                        await _patientServices.EditPatient(currentPatient);
                    } 
                    catch (InvalidOperationException e)
                    {
                        ModelState.AddModelError("", e.Message);
                        PrefillSelectOptions(currentPatient);
                        return Edit(newPatient.Id);
                    }

                    return RedirectToAction("Index");
                }
            }

            return Edit(newPatient.Id);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<ActionResult> EditEmail()
        {
            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name, CancellationToken.None);
            Patient patient = _patientRepository.GetByUserId(user.Id);

            ViewBag.Patient = patient;
            return View();
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditEmail(EditEmail model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name, CancellationToken.None);
                Patient patient = _patientRepository.GetByUserId(user.Id);

                patient.Email = model.Email;
                user.UserName = model.Email;
                user.Email = model.Email;
                user.NormalizedUserName = model.Email.ToUpper();
                user.NormalizedEmail = model.Email.ToUpper();

                await _patientRepository.EditPatient(patient);
                await userManager.UpdateAsync(user, CancellationToken.None);

                return RedirectToAction("Index", "Home");
            }
            return await EditEmail();
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            Patient patient = _patientRepository.GetById(id);

            if (patient != null)
            {
                await _patientRepository.DeletePatient(patient);
                return RedirectToAction("Index");
            }
            else
                return View();
        }
    }
}
