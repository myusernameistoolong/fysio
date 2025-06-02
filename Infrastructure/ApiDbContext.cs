using System;
using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> contextOptions) : base(contextOptions)
        {

        }

        public DbSet<TreatmentType> TreatmentTypes { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TreatmentType>().ToTable("TreatmentTypes");
            modelBuilder.Entity<Diagnosis>().ToTable("Diagnoses");
        }
    }
}
