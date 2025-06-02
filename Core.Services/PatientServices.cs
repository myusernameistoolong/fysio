using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Core.Domain;
using Core.DomainServices;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class PatientServices : IPatientServices
    {
        private readonly IPatientRepository _patientRepository;
        private int minAge = 16;

        public PatientServices(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }
        public async Task AddPatient(Patient patient)
        {
            Validation(patient);
            await _patientRepository.AddPatient(patient);
        }

        public async Task EditPatient(Patient patient)
        {
            Validation(patient);
            await _patientRepository.EditPatient(patient);
        }

        public void Validation(Patient patient)
        {
            if (GetAge(patient.Bday) < minAge)
                throw new InvalidOperationException("Patient needs to be at least 16 years of age");
            if (patient.StudentNr == null && patient.BigNr == null)
                throw new InvalidOperationException("Student number nor big number have been filled in");
            if (patient.StudentNr != null && patient.BigNr != null)
                throw new InvalidOperationException("Either fill in student number or big number");
        }

        public static int GetAge(DateTime dateOfBirth)
        {
            int age = DateTime.Now.Year - dateOfBirth.Year;
            if (dateOfBirth.AddYears(age) > DateTime.Now)
            {
                age = age - 1;
            }

            return age;
        }
    }
}
