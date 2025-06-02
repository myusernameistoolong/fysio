using Core.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public static class DbInitializer
    {
        public static void Initialize(ApiDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.TreatmentTypes.Any() && context.Diagnoses.Any())
                return;   // DB has been seeded

            if (!context.TreatmentTypes.Any())
            {
                SeedActivities(context);
            }

            if (!context.Diagnoses.Any())
            {
                SeedDiagnoses(context);
            }
        }

        public static void SeedActivities(ApiDbContext context)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Data", "Vektis lijst verrichtingen.csv");
            var readcsv = File.ReadAllLines(path);

            foreach (var row in readcsv)
            {
                if (!string.IsNullOrEmpty(row)){
                    var cells = row.Split(',');
                    if (!cells[0].Equals("Waarde"))
                    {
                        var activity = new TreatmentType { Code = cells[0], Desc = cells[1], Required = cells[2] };
                        context.TreatmentTypes.Add(activity);
                        context.SaveChanges();
                    }
                }
            }
        }

        public static void SeedDiagnoses(ApiDbContext context)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Data", "Vektis lijst diagnoses gecorrigeerd.csv");
            var readcsv = File.ReadAllLines(path);

            foreach (var row in readcsv)
            {
                if (!string.IsNullOrEmpty(row))
                {
                    var cells = row.Split(',');
                    if (!cells[0].Equals("Code"))
                    {
                        var diagnosis = new Diagnosis { Code = cells[0], BodyArea = cells[1], Pathology = cells[2] };
                        context.Diagnoses.Add(diagnosis);
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
