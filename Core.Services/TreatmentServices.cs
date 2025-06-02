using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Core.Domain;
using Core.DomainServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core.Services
{
    public class TreatmentServices : ITreatmentServices
    {
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IDossierRepository _dossierRepository;
        private readonly IPhysiotherapistRepository _physiotherapistRepository;
        private readonly string UriString = "https://fysio000api.azurewebsites.net/api/";

        public TreatmentServices(ITreatmentRepository treatmentRepository,
            IDossierRepository dossierRepository,
            IPhysiotherapistRepository physiotherapistRepository)
        {
            _treatmentRepository = treatmentRepository;
            _dossierRepository = dossierRepository;
            _physiotherapistRepository = physiotherapistRepository;
        }

        public async Task AddTreatment(Treatment treatment, ClaimsPrincipal userIdentity)
        {
            Validation(treatment, userIdentity);
            await _treatmentRepository.AddTreatment(treatment);
        }

        public async Task EditTreatment(Treatment treatment, ClaimsPrincipal userIdentity)
        {
            Validation(treatment, userIdentity, true);
            await _treatmentRepository.EditTreatment(treatment);
        }

        public async Task CancelTreatment(Treatment treatment, ApplicationUser user)
        {
            TimeSpan diff = treatment.StartDate - DateTime.Now;
            double hours = diff.TotalHours;

            if (_dossierRepository.CheckIfDossierIsRelated(treatment.DossierId, user) == null || hours < 24 || treatment.StartDate < DateTime.Now)
                throw new InvalidOperationException("Cannot cancel when the appointment starts within 24 hours");

            treatment.EndDate = DateTime.MinValue;
            await _treatmentRepository.EditTreatment(treatment);
        }

        public void Validation(Treatment treatment, ClaimsPrincipal userIdentity, bool isEdit = false)
        {
            TimeSpan diff = treatment.StartDate - DateTime.Now;
            double hours = diff.TotalHours;

            Dossier dossier = _dossierRepository.GetById(treatment.DossierId);
            DateTime endDate = treatment.StartDate.AddHours(dossier.LengthOfTreatments.Hour);

            if (dossier.Patient.UserId == null)
                throw new InvalidOperationException("This patient is not registed yet");

            if (!userIdentity.IsInRole("FysioTherapist") && dossier.HeadPractitioner != treatment.PerformedBy)
                throw new InvalidOperationException("This physiotherapist is unavailable");

            if (!isEdit)
            {
                if (!(treatment.StartDate.TimeOfDay < _physiotherapistRepository.GetById(treatment.PerformedBy).EndTime.TimeOfDay && endDate.TimeOfDay > _physiotherapistRepository.GetById(treatment.PerformedBy).StartTime.TimeOfDay) ||
                    _treatmentRepository.GetTreatmentsByPhysioTherapistId(dossier.HeadPractitioner, treatment.StartDate, endDate).ToList().Count >= 1)
                    throw new InvalidOperationException("This physiotherapist is occupied during these times");
            }
            else
            {
                if (!(treatment.StartDate.TimeOfDay < _physiotherapistRepository.GetById(treatment.PerformedBy).EndTime.TimeOfDay && endDate.TimeOfDay > _physiotherapistRepository.GetById(treatment.PerformedBy).StartTime.TimeOfDay) ||
                    _treatmentRepository.GetTreatmentsByPhysioTherapistId(dossier.HeadPractitioner, treatment.StartDate, endDate, treatment.Id).ToList().Count >= 1)
                    throw new InvalidOperationException("This physiotherapist is occupied during these times");
            }

            if (treatment.StartDate <= DateTime.Now)
                throw new InvalidOperationException("Only future time and dates are allowed");

            if (treatment.StartDate <= dossier.DateOfRegistration)
                throw new InvalidOperationException("Treatment date needs to take place *after* the patient's date of registration");

            if (treatment.StartDate >= dossier.DateOfEndProcedure)
                throw new InvalidOperationException("Treatment date needs to take place *before* the patient's date of end procedure");

            if (endDate < treatment.StartDate)
                throw new InvalidOperationException("End time & date needs to take place *before* the start time & date");

            if (treatment.Type != null)
            {
                var cal = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;

                if (!isEdit)
                {
                    if (_treatmentRepository.GetAllByDossierId(treatment.DossierId).Where(t => t.StartDate.Date.AddDays(-1 * (int)cal.GetDayOfWeek(t.StartDate)) == treatment.StartDate.Date.AddDays(-1 * (int)cal.GetDayOfWeek(treatment.StartDate))).ToList().Count >= dossier.AmountOfTreatments)
                        throw new InvalidOperationException("You cannot create more appointments in this week than is prescribed in the treatment plan");
                }
                else
                {
                    if (_treatmentRepository.GetAllByDossierId(treatment.DossierId).Where(t => t.Id != treatment.Id && t.StartDate.Date.AddDays(-1 * (int)cal.GetDayOfWeek(t.StartDate)) == treatment.StartDate.Date.AddDays(-1 * (int)cal.GetDayOfWeek(treatment.StartDate))).ToList().Count >= dossier.AmountOfTreatments)
                        throw new InvalidOperationException("You cannot create more appointments in this week than is prescribed in the treatment plan");
                }
            }

            //Set treatment time out
            if(treatment.TreatmentTimeOut == null || treatment.TreatmentTimeOut == DateTime.MinValue)
            {
                if(treatment.Type != null && treatment.Type != "None")
                    treatment.TreatmentTimeOut = DateTime.Now;
            }
            else
            {
                diff = DateTime.Now - (DateTime)treatment.TreatmentTimeOut;
                hours = diff.TotalHours;

                if (hours > 24)
                    throw new InvalidOperationException("You cannot adjust this appointment 24 hours after the treatment has been confirmed"); 
            }

            //Get treatment type desc
            if (treatment.Type != "None" && userIdentity.IsInRole("FysioTherapist"))
            {
                TreatmentType treatmentType = null;

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(UriString);

                    string detailedAddress = "TreatmentTypes/" + treatment.Type;
                    var responseTask = client.GetAsync(detailedAddress);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<TreatmentType>();
                        readTask.Wait();
                        treatmentType = readTask.Result;
                    }
                    else
                    {
                        treatmentType = null;
                        throw new InvalidOperationException("Server error. Please contact administrator.");
                    }
                }
                if (treatmentType == null)
                    throw new InvalidOperationException("Could not connect to API");

                treatment.Type = treatmentType.Code;
                treatment.Desc = treatmentType.Desc;

                if (treatmentType.Required == "Ja" && treatment.Specialities == null)
                    throw new InvalidOperationException("Specialities are required with this treatment type");
            }
            else
            {
                treatment.Type = "TBA";
                treatment.Desc = "TBA";
            }
        }
    }
}
