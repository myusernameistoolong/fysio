using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using Core.DomainServices;
using Core.Services;
using fysio.Models;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace management.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IPatientRepository _patientRepository;
        private readonly IPhysiotherapistRepository _physiotherapistRepository;
        private readonly IPhysiotherapistServices _physiotherapistServices;

        public AccountController(UserManager<ApplicationUser> userMgr,
            SignInManager<ApplicationUser> signInMgr,
            IPatientRepository patientRepository,
            IPhysiotherapistRepository physiotherapistRepository,
            IPhysiotherapistServices physiotherapistServices)
        {
            userManager = userMgr;
            signInManager = signInMgr;
            _patientRepository = patientRepository;
            _physiotherapistRepository = physiotherapistRepository;
            _physiotherapistServices = physiotherapistServices;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    await signInManager.SignOutAsync();
                    if ((await signInManager.PasswordSignInAsync(user, model.Password, false, false)).Succeeded)
                    {
                        return Redirect(model?.ReturnUrl ?? "/");
                    }
                }
            }

            ModelState.AddModelError("", "Invalid name or password");
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            PrefillSelectOptions();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Patient existingEmail = _patientRepository.GetByEmail(model.Email);

                if (existingEmail != null)
                {
                    var user = new ApplicationUser(model.Name, model.LastName, model.Email);
                    user.UserName = model.Email;

                    //var physiotherapist = new Physiotherapist(model.Name, model.LastName, model.Email, "000", null, 100);
                    //await _physiotherapistRepository.AddPhysiotherapist(physiotherapist);

                    var result = await userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        existingEmail.UserId = user.Id;
                        //physiotherapist.UserId = user.Id;
                        await _patientRepository.EditPatient(existingEmail);

                        await userManager.AddToRoleAsync(user, "Patient");
                        await signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                    AddErrors(result);
                }
                else
                {
                    ModelState.AddModelError("", "Email does not exist in patient register");
                }
            }

            PrefillSelectOptions();
            return View();
        }

        [Authorize(Roles = "FysioTherapist")]
        [HttpGet]
        public async Task<ActionResult> ChangeAvailability()
        {
            ApplicationUser user = await userManager.GetUserAsync(User);
            Physiotherapist physiotherapist = _physiotherapistRepository.GetByUserId(user.Id);

            ViewBag.Physiotherapist = physiotherapist;
            return View();
        }

        [Authorize(Roles = "FysioTherapist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeAvailability(EditAvailabilityViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.GetUserAsync(User);
                Physiotherapist physiotherapist = _physiotherapistRepository.GetByUserId(user.Id);

                physiotherapist.StartTime = model.StartTime;
                physiotherapist.EndTime = model.EndTime;

                try
                {
                    await _physiotherapistServices.EditPhysiotherapist(physiotherapist);
                }
                catch (InvalidOperationException e)
                {
                    ModelState.AddModelError("", e.Message);
                    return await ChangeAvailability();
                }

                return RedirectToAction("Index", "Home");
            }
            return await ChangeAvailability();
        }

        public async Task<RedirectResult> Logout(string returnUrl = "/")
        {
            await signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        private void PrefillSelectOptions()
        {
            ViewBag.Sexes = new SelectList(Enum.GetValues(typeof(Sex)), Sex.Male);
        }
    }
}
