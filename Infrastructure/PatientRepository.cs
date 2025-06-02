using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Core.Domain;
using Core.DomainServices;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ClientDbContext _context;

        public PatientRepository(ClientDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Patient> GetAll()
        {
            return _context.Patients;
        }

        public Patient GetById(int id)
        {
            return _context.Patients
                .Include(patient => patient.Dossiers)
                .SingleOrDefault(patient => patient.Id == id);
        }

        public Patient GetByUserId(string id)
        {
            return _context.Patients
                .Include(patient => patient.Dossiers)
                .SingleOrDefault(patient => patient.UserId == id);
        }

        public async Task AddPatient(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
        }

        public async Task EditPatient(Patient patient)
        {
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePatient(Patient patient)
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
        }

        public Patient GetByEmail(string email)
        {
            return _context.Patients.SingleOrDefault(patient => patient.Email == email);
        }

        public IEnumerable<Patient> GetAllWithoutDossierWithinDate()
        {
            return _context.Patients
                .Include(patient => patient.Dossiers)
                .Where(patient => patient.Dossiers.FirstOrDefault() == null);
        }
    }
}
