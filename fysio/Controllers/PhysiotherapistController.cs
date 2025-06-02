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

namespace fysio.Controllers
{
    public class PhysiotherapistController : Controller
    {
        private readonly ILogger<PhysiotherapistController> _logger;
        private readonly IPhysiotherapistRepository _physiotherapistRepository;

        public PhysiotherapistController(ILogger<PhysiotherapistController> logger,
            IPhysiotherapistRepository physiotherapistRepository)
        {
            _logger = logger;
            _physiotherapistRepository = physiotherapistRepository;
        }

        [Authorize(Roles = "FysioTherapist")]
        public IActionResult Index()
        {
            return View(_physiotherapistRepository.GetAll().ToList());
        }

        [Authorize(Roles = "FysioTherapist")]
        public IActionResult Details(int id)
        {
            Physiotherapist physiotherapist = _physiotherapistRepository.GetById(id);
            return View(physiotherapist);
        }

        [Authorize(Roles = "FysioTherapist")]
        [HttpGet]
        public IActionResult Create()
        {
            PrefillSelectOptions();
            return View();
        }

        private void PrefillSelectOptions()
        {
            ViewBag.Sexes = new SelectList(Enum.GetValues(typeof(Sex)), Sex.Male);
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(NewPhysiotherapistViewModel newPhysiotherapist)
        {
            if(ModelState.IsValid)
            {
                var physiotherapist = new Physiotherapist(newPhysiotherapist.Name, newPhysiotherapist.LastName, newPhysiotherapist.Email, newPhysiotherapist.Phone, DateTime.Now.AddHours(12), DateTime.Now.AddHours(16), newPhysiotherapist.StudentNr, newPhysiotherapist.BigNr);
                await _physiotherapistRepository.AddPhysiotherapist(physiotherapist);

                return RedirectToAction("Index");
            }
            else
                return View("Create");
        }

        [Authorize(Roles = "FysioTherapist")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Physiotherapist physiotherapist = _physiotherapistRepository.GetById(id);
            ViewBag.Physiotherapist = physiotherapist;
            PrefillSelectOptions();

            return View();
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(EditPhysiotherapistViewModel newPhysiotherapist)
        {
            if (ModelState.IsValid)
            {
                Physiotherapist currentPhysiotherapist = _physiotherapistRepository.GetById(newPhysiotherapist.Id);

                if (currentPhysiotherapist != null)
                {
                    currentPhysiotherapist.Name         = newPhysiotherapist.Name;
                    currentPhysiotherapist.LastName     = newPhysiotherapist.LastName;
                    currentPhysiotherapist.Email        = newPhysiotherapist.Email;
                    currentPhysiotherapist.Phone        = newPhysiotherapist.Phone;
                    currentPhysiotherapist.StudentNr    = newPhysiotherapist.StudentNr;
                    currentPhysiotherapist.BigNr        = newPhysiotherapist.BigNr;

                    await _physiotherapistRepository.EditPhysiotherapist(currentPhysiotherapist);

                    return RedirectToAction("Index");
                }
                else
                    return View("Edit");
            }
            else
                return View("Edit");
        }

        [Authorize(Roles = "FysioTherapist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            Physiotherapist physiotherapist = _physiotherapistRepository.GetById(id);

            if (physiotherapist != null)
            {
                await _physiotherapistRepository.DeletePhysiotherapist(physiotherapist);
                return RedirectToAction("Index");
            }
            else
                return View();
        }
    }
}
