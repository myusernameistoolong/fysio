using System;
using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ClientDbContext : DbContext
    {
        public ClientDbContext(DbContextOptions<ClientDbContext> contextOptions) : base(contextOptions)
        {

        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Physiotherapist> Physiotherapists { get; set; }
        public DbSet<Dossier> Dossiers { get; set; }
        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<Note> Notes { get; set; }
    }
}
