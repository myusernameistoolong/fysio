using Core.DomainServices;
using Core.Services;
using fysio.Controllers;
using fysio.Models;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Core.Domain.Tests
{
    public class BusinessRules
    {
        public BusinessRules()
        {
            Environment.SetEnvironmentVariable("API_URL", "https://fysio000api.azurewebsites.net/api/");
        }

        //Het maximaal aantal afspraken per week wordt niet overschreden bij het boeken van een afspraak.
        [Fact]
        public async Task MaxAppointmentsEachWeekCannotExceed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TreatmentController>>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();
            var physiotherapistMock = new Mock<IPhysiotherapistRepository>();
            var dossierMock = new Mock<IDossierRepository>();

            var treatmentMock = new Mock<ITreatmentRepository>();
            TreatmentServices treatmentServices = new TreatmentServices(treatmentMock.Object, dossierMock.Object, physiotherapistMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone")
            }, "mock"));

            TreatmentController treatmentController = new TreatmentController(loggerMock.Object, treatmentMock.Object, treatmentServices, applicationUserMock.Object, physiotherapistMock.Object, dossierMock.Object);
            treatmentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            treatmentMock.Setup(m => m.GetAllByDossierId(1)).Returns(new List<Treatment>()
            {
                new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1), //First treatment
                new Treatment("type", "Patient has a headache", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1) //Second treatment
            });

            physiotherapistMock.Setup(m => m.GetById(1)).Returns(new Physiotherapist("Simone", "Kerseboom", "simone@kerse.nl", "000", DateTime.Today.AddHours(8), DateTime.Today.AddHours(23), null, 100000));
            dossierMock.Setup(m => m.GetById(1)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, 1, DateTime.Now, null, DateTime.Today.AddHours(1), 2) { Patient = new Patient("Test", "Kees", "test@kees.nl", "000", "photo", DateTime.Now, "male", "1000000", null) { UserId = "10" } }); //Limit is 2
            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));

            // Act
            var newTreatmentModel = new NewTreatmentViewModel()
            {
                Type = "1",
                Desc = "Patient was absent for 2 days",
                Location = "Room 3",
                Specialities = "Abnormal body parts",
                PerformedBy = 1,
                StartDate = DateTime.Today.AddDays(1).AddHours(14),
                DossierId = 1
            };

            var result = await treatmentController.Create(newTreatmentModel);

            // Assert
            var viewResult = result as ViewResult;
            //var key = nameof(newTreatmentModel.Type);

            Assert.Equal("You cannot create more appointments in this week than is prescribed in the treatment plan", viewResult.ViewData.ModelState[""].Errors.First().ErrorMessage);
            treatmentMock.Verify(m => m.AddTreatment(It.IsAny<Treatment>()), Times.Never);
        }

        [Fact]
        public async Task AppointmentCanBeCreatedWithinLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TreatmentController>>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();
            var physiotherapistMock = new Mock<IPhysiotherapistRepository>();
            var dossierMock = new Mock<IDossierRepository>();

            var treatmentMock = new Mock<ITreatmentRepository>();
            TreatmentServices treatmentServices = new TreatmentServices(treatmentMock.Object, dossierMock.Object, physiotherapistMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone")
            }, "mock"));

            TreatmentController treatmentController = new TreatmentController(loggerMock.Object, treatmentMock.Object, treatmentServices, applicationUserMock.Object, physiotherapistMock.Object, dossierMock.Object);
            treatmentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            treatmentMock.Setup(m => m.GetAllByDossierId(1)).Returns(new List<Treatment>()
            {
                new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1), //First treatment
                new Treatment("type", "Patient has a headache", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1) //Second treatment
            });

            physiotherapistMock.Setup(m => m.GetById(1)).Returns(new Physiotherapist("Simone", "Kerseboom", "simone@kerse.nl", "000", DateTime.Today.AddHours(4), DateTime.Today.AddHours(20), 100000));
            dossierMock.Setup(m => m.GetById(1)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, 1, DateTime.Now, null, DateTime.Today.AddHours(1), 3) { Patient = new Patient("Test", "Kees", "test@kees.nl", "000", "photo", DateTime.Now, "male", "1000000", null) { UserId = "10" } }); //Limit is 3
            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));

            // Act
            var newTreatmentModel = new NewTreatmentViewModel()
            {
                Type = "1",
                Desc = "Patient was absent for 2 days",
                Location = "Room 3",
                Specialities = "Abnormal body parts",
                PerformedBy = 1,
                StartDate = DateTime.Today.AddDays(3).AddHours(14),
                DossierId = 1
            };

            var result = await treatmentController.Create(newTreatmentModel);

            // Assert
            treatmentMock.Verify(m => m.AddTreatment(It.IsAny<Treatment>()), Times.Once);
        }

        //Afspraken kunnen alleen worden gemaakt op beschikbare momenten van de hoofdbehandelaar.
        //Hierbij moet rekening gehouden worden met de algemene beschikbaarheid en de reeds gemaakte afspraken.
        [Fact]
        public async Task TreatmentsCanOnlyBeMadeAtAvailableHours()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TreatmentController>>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();
            var physiotherapistMock = new Mock<IPhysiotherapistRepository>();
            var dossierMock = new Mock<IDossierRepository>();

            var treatmentMock = new Mock<ITreatmentRepository>();
            TreatmentServices treatmentServices = new TreatmentServices(treatmentMock.Object, dossierMock.Object, physiotherapistMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone"),
                new Claim(ClaimTypes.Role, "FysioTherapist")
            }, "mock"));

            TreatmentController treatmentController = new TreatmentController(loggerMock.Object, treatmentMock.Object, treatmentServices, applicationUserMock.Object, physiotherapistMock.Object, dossierMock.Object);
            treatmentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            int headPractitioner = 1;
            DateTime now = DateTime.Today;
            DateTime startDate = now.AddDays(1); //1 day after today
            DateTime endDate = startDate.AddHours(2);

            Treatment existingTreatment = new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", headPractitioner, now.AddDays(1), now.AddDays(1).AddHours(2), 1); //1 day after today

            treatmentMock.Setup(m => m.GetAllByDossierId(1)).Returns(new List<Treatment>()
            {
                existingTreatment
            });

            physiotherapistMock.Setup(m => m.GetById(1)).Returns(new Physiotherapist("Simone", "Kerseboom", "simone@kerse.nl", "000", DateTime.Today.AddHours(6), DateTime.Today.AddHours(20), null, 100000));
            dossierMock.Setup(m => m.GetById(1)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, headPractitioner, now, null, DateTime.Today.AddHours(2), 2) { Patient = new Patient("Test", "Kees", "test@kees.nl", "000", "photo", now, "male", "1000000", null) { UserId = "10" } }); //Treatments take 2 hours
            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));
            treatmentMock.Setup(m => m.GetTreatmentsByPhysioTherapistId(headPractitioner, startDate, endDate, null)).Returns(new List<Treatment> { existingTreatment });

            // Act
            var newTreatmentModel = new NewTreatmentViewModel()
            {
                Type = "1",
                Desc = "Patient was absent for 2 days",
                Location = "Room 3",
                Specialities = "Abnormal body parts",
                PerformedBy = headPractitioner,
                StartDate = startDate,
                DossierId = 1
            };

            var result = await treatmentController.Create(newTreatmentModel);

            // Assert
            var viewResult = result as ViewResult;
            //var key = nameof(newTreatmentModel.StartDate);

            Assert.Equal("This physiotherapist is occupied during these times", viewResult.ViewData.ModelState[""].Errors.First().ErrorMessage);
        }

        [Fact]
        public async Task TreatmentsCanBeMadeAtAvailableHours()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TreatmentController>>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();
            var physiotherapistMock = new Mock<IPhysiotherapistRepository>();
            var dossierMock = new Mock<IDossierRepository>();

            var treatmentMock = new Mock<ITreatmentRepository>();
            TreatmentServices treatmentServices = new TreatmentServices(treatmentMock.Object, dossierMock.Object, physiotherapistMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone"),
                new Claim(ClaimTypes.Role, "FysioTherapist")
            }, "mock"));

            TreatmentController treatmentController = new TreatmentController(loggerMock.Object, treatmentMock.Object, treatmentServices, applicationUserMock.Object, physiotherapistMock.Object, dossierMock.Object);
            treatmentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            int headPractitioner = 1;
            DateTime now = DateTime.Today;
            DateTime startDate = now.AddDays(1); //1 day after today
            DateTime endDate = startDate.AddHours(2);

            Treatment existingTreatment = new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", headPractitioner, now.AddDays(1), now.AddDays(1).AddHours(2), 1); //1 day after today

            treatmentMock.Setup(m => m.GetAllByDossierId(1)).Returns(new List<Treatment>()
            {
                existingTreatment
            });

            physiotherapistMock.Setup(m => m.GetById(1)).Returns(new Physiotherapist("Simone", "Kerseboom", "simone@kerse.nl", "000", DateTime.Today.AddHours(1), DateTime.Today.AddHours(22), null, 100000));
            dossierMock.Setup(m => m.GetById(1)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, headPractitioner, now, null, DateTime.Today.AddHours(2), 3) { Patient = new Patient("Test", "Kees", "test@kees.nl", "000", "photo", now, "male", "1000000", null) { UserId = "10" } }); //Treatments take 2 hours
            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));
            treatmentMock.Setup(m => m.GetTreatmentsByPhysioTherapistId(headPractitioner, startDate, endDate, null)).Returns(new List<Treatment> { existingTreatment });

            // Act
            var newTreatmentModel = new NewTreatmentViewModel()
            {
                Type = "1",
                Desc = "Patient was absent for 2 days",
                Location = "Room 3",
                Specialities = "Abnormal body parts",
                PerformedBy = headPractitioner,
                StartDate = DateTime.Today.AddDays(1).AddHours(6),
                DossierId = 1
            };

            var result = await treatmentController.Create(newTreatmentModel);

            // Assert
            treatmentMock.Verify(m => m.AddTreatment(It.IsAny<Treatment>()), Times.Once);
        }

        //Een behandeling kan niet in worden gevoerd als de patiënt nog niet in de praktijk is geregistreerd of nadat de behandeling is beëindigd.
        [Fact]
        public async Task CannotCreateTreatmentWhenPatientIsNotRegisted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TreatmentController>>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();
            var physiotherapistMock = new Mock<IPhysiotherapistRepository>();
            var dossierMock = new Mock<IDossierRepository>();

            var treatmentMock = new Mock<ITreatmentRepository>();
            TreatmentServices treatmentServices = new TreatmentServices(treatmentMock.Object, dossierMock.Object, physiotherapistMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone")
            }, "mock"));

            TreatmentController treatmentController = new TreatmentController(loggerMock.Object, treatmentMock.Object, treatmentServices, applicationUserMock.Object, physiotherapistMock.Object, dossierMock.Object);
            treatmentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            treatmentMock.Setup(m => m.GetAllByDossierId(1)).Returns(new List<Treatment>()
            {
                new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1),
                new Treatment("type", "Patient has a headache", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1)
            });

            physiotherapistMock.Setup(m => m.GetById(1)).Returns(new Physiotherapist("Simone", "Kerseboom", "simone@kerse.nl", "000", DateTime.Today.AddHours(4), DateTime.Today.AddHours(20), null, 100000));
            dossierMock.Setup(m => m.GetById(1)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, 1, DateTime.Now, null, DateTime.Today.AddHours(1), 2) { Patient = new Patient("Test", "Kees", "test@kees.nl", "000", "photo", DateTime.Now, "male", "1000000", null) { UserId = null } }); //User id is null
            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));

            // Act
            var newTreatmentModel = new NewTreatmentViewModel()
            {
                Type = "1",
                Desc = "Patient was absent for 2 days",
                Location = "Room 3",
                Specialities = "Abnormal body parts",
                PerformedBy = 1,
                StartDate = DateTime.Today.AddDays(3).AddHours(14),
                DossierId = 1
            };

            var result = await treatmentController.Create(newTreatmentModel);

            // Assert
            var viewResult = result as ViewResult;
            //var key = nameof(newTreatmentModel.DossierId);

            Assert.Equal("This patient is not registed yet", viewResult.ViewData.ModelState[""].Errors.First().ErrorMessage);
        }

        [Fact]
        public async Task CanCreateTreatmentWhenPatientIsRegisted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TreatmentController>>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();
            var physiotherapistMock = new Mock<IPhysiotherapistRepository>();
            var dossierMock = new Mock<IDossierRepository>();

            var treatmentMock = new Mock<ITreatmentRepository>();
            TreatmentServices treatmentServices = new TreatmentServices(treatmentMock.Object, dossierMock.Object, physiotherapistMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone")
            }, "mock"));

            TreatmentController treatmentController = new TreatmentController(loggerMock.Object, treatmentMock.Object, treatmentServices, applicationUserMock.Object, physiotherapistMock.Object, dossierMock.Object);
            treatmentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            treatmentMock.Setup(m => m.GetAllByDossierId(1)).Returns(new List<Treatment>()
            {
                new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1)
            });

            physiotherapistMock.Setup(m => m.GetById(1)).Returns(new Physiotherapist("Simone", "Kerseboom", "simone@kerse.nl", "000", DateTime.Today.AddHours(4), DateTime.Today.AddHours(20), null, 100000));
            dossierMock.Setup(m => m.GetById(1)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, 1, DateTime.Now, null, DateTime.Today.AddHours(1), 2) { Patient = new Patient("Test", "Kees", "test@kees.nl", "000", "photo", DateTime.Now, "male", "1000000", null) { UserId = "10" } }); //User is linked to identity
            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));

            // Act
            var newTreatmentModel = new NewTreatmentViewModel()
            {
                Type = "1",
                Desc = "Patient was absent for 2 days",
                Location = "Room 3",
                Specialities = "Abnormal body parts",
                PerformedBy = 1,
                StartDate = DateTime.Today.AddDays(3).AddHours(14),
                DossierId = 1
            };

            var result = await treatmentController.Create(newTreatmentModel);

            // Assert
            treatmentMock.Verify(m => m.AddTreatment(It.IsAny<Treatment>()), Times.Once);
        }

        //Bij een aantal behandelingen is een toelichting verplicht.
        [Fact]
        public async Task DescriptionIsRequiredWithSomeTreatmentTypes()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TreatmentController>>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();
            var physiotherapistMock = new Mock<IPhysiotherapistRepository>();
            var dossierMock = new Mock<IDossierRepository>();

            var treatmentMock = new Mock<ITreatmentRepository>();
            TreatmentServices treatmentServices = new TreatmentServices(treatmentMock.Object, dossierMock.Object, physiotherapistMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone"),
                new Claim(ClaimTypes.Role, "FysioTherapist")
            }, "mock"));

            TreatmentController treatmentController = new TreatmentController(loggerMock.Object, treatmentMock.Object, treatmentServices, applicationUserMock.Object, physiotherapistMock.Object, dossierMock.Object);
            treatmentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            treatmentMock.Setup(m => m.GetAllByDossierId(1)).Returns(new List<Treatment>()
            {
                new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1)
            });

            physiotherapistMock.Setup(m => m.GetById(1)).Returns(new Physiotherapist("Simone", "Kerseboom", "simone@kerse.nl", "000", DateTime.Today.AddHours(4), DateTime.Today.AddHours(20), null, 100000));
            dossierMock.Setup(m => m.GetById(1)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, 1, DateTime.Now, null, DateTime.Today.AddHours(1), 2) { Patient = new Patient("Test", "Kees", "test@kees.nl", "000", "photo", DateTime.Now, "male", "1000000", null) { UserId = "10" } });
            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));

            // Act
            var newTreatmentModel = new NewTreatmentViewModel()
            {
                Type = "1", //Required treatment type
                Desc = "Patient was absent for 2 days",
                Location = "Room 3",
                Specialities = null, //Null
                PerformedBy = 1,
                StartDate = DateTime.Today.AddDays(3).AddHours(14),
                DossierId = 1
            };

            var result = await treatmentController.Create(newTreatmentModel);

            // Assert
            var viewResult = result as ViewResult;
            //var key = nameof(newTreatmentModel.Specialities);

            Assert.Equal("Specialities are required with this treatment type", viewResult.ViewData.ModelState[""].Errors.First().ErrorMessage);
        }

        [Fact]
        public async Task DescriptionIsNotRequiredWithSomeTreatmentTypes()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TreatmentController>>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();
            var physiotherapistMock = new Mock<IPhysiotherapistRepository>();
            var dossierMock = new Mock<IDossierRepository>();

            var treatmentMock = new Mock<ITreatmentRepository>();
            TreatmentServices treatmentServices = new TreatmentServices(treatmentMock.Object, dossierMock.Object, physiotherapistMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone"),
                new Claim(ClaimTypes.Role, "FysioTherapist")
            }, "mock"));

            TreatmentController treatmentController = new TreatmentController(loggerMock.Object, treatmentMock.Object, treatmentServices, applicationUserMock.Object, physiotherapistMock.Object, dossierMock.Object);
            treatmentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            treatmentMock.Setup(m => m.GetAllByDossierId(1)).Returns(new List<Treatment>()
            {
                new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1)
            });

            physiotherapistMock.Setup(m => m.GetById(1)).Returns(new Physiotherapist("Simone", "Kerseboom", "simone@kerse.nl", "000", DateTime.Today.AddHours(4), DateTime.Today.AddHours(20), null, 100000));
            dossierMock.Setup(m => m.GetById(1)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, 1, DateTime.Now, null, DateTime.Today.AddHours(1), 2) { Patient = new Patient("Test", "Kees", "test@kees.nl", "000", "photo", DateTime.Now, "male", "1000000", null) { UserId = "10" } });
            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));

            // Act
            var newTreatmentModel = new NewTreatmentViewModel()
            {
                Type = "6", //Not required treatment type
                Desc = "Patient was absent for 2 days",
                Location = "Room 3",
                Specialities = null, //Null
                PerformedBy = 1,
                StartDate = DateTime.Today.AddDays(3).AddHours(14),
                DossierId = 1
            };

            var result = await treatmentController.Create(newTreatmentModel);

            // Assert
            treatmentMock.Verify(m => m.AddTreatment(It.IsAny<Treatment>()), Times.Once);
        }

        //De leeftijd van een patiënt is >= 16.
        [Fact]
        public async Task CannotSetAgeOfPatientWhenYoungerThan16()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PatientController>>();
            var webhostMock = new Mock<IWebHostEnvironment>();
            var patientMock = new Mock<IPatientRepository>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();

            var patientRepMock = new Mock<IPatientRepository>();
            PatientServices patientServices = new PatientServices(patientRepMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone")
            }, "mock"));

            PatientController patientController = new PatientController(loggerMock.Object, webhostMock.Object, patientServices, patientMock.Object, applicationUserMock.Object);
            patientController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));

            // Act
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.txt");

            var newPatientModel = new NewPatientViewModel()
            {
                Name = "Simone",
                LastName = "Kerseboom",
                Email = "simone@kerse.nl",
                Phone = "000",
                Photo = file,
                Bday = DateTime.Now.AddYears(-15), //Age
                Sex = "Female",
                BigNr = null,
                StudentNr = "1000000"
            };

            var result = await patientController.Create(newPatientModel);

            // Assert
            var viewResult = result as ViewResult;
            //var key = nameof(newPatientModel.Bday);

            Assert.Equal("Patient needs to be at least 16 years of age", viewResult.ViewData.ModelState[""].Errors.First().ErrorMessage);
            patientRepMock.Verify(m => m.AddPatient(It.IsAny<Patient>()), Times.Never);
        }

        [Fact]
        public async Task CanSetAgeOfPatientWhenIsOlderThan15()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PatientController>>();
            var webhostMock = new Mock<IWebHostEnvironment>();
            var patientMock = new Mock<IPatientRepository>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();

            var patientRepMock = new Mock<IPatientRepository>();
            PatientServices patientServices = new PatientServices(patientRepMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone")
            }, "mock"));

            PatientController patientController = new PatientController(loggerMock.Object, webhostMock.Object, patientServices, patientMock.Object, applicationUserMock.Object);
            patientController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));

            // Act
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.txt");

            var newPatientModel = new NewPatientViewModel()
            {
                Name = "Simone",
                LastName = "Kerseboom",
                Email = "simone@kerse.nl",
                Phone = "000",
                Photo = file,
                Bday = DateTime.Now.AddYears(-16),
                Sex = "Female",
                BigNr = null,
                StudentNr = "1000000"
            };

            var result = await patientController.Create(newPatientModel);

            // Assert
            var viewResult = result as ViewResult;
            patientRepMock.Verify(m => m.AddPatient(It.IsAny<Patient>()), Times.Once);
        }

        //Een afspraak kan niet door een patiënt worden geannuleerd minder van 24 uur voorafgaand aan de afspraak.
        [Fact]
        public async Task CannotCancelTreatmentWithin24HoursAsync()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TreatmentController>>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();
            var physiotherapistMock = new Mock<IPhysiotherapistRepository>();
            var dossierMock = new Mock<IDossierRepository>();

            var treatmentMock = new Mock<ITreatmentRepository>();
            TreatmentServices treatmentServices = new TreatmentServices(treatmentMock.Object, dossierMock.Object, physiotherapistMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone"),
                new Claim(ClaimTypes.Role, "Patient")
            }, "mock"));

            TreatmentController treatmentController = new TreatmentController(loggerMock.Object, treatmentMock.Object, treatmentServices, applicationUserMock.Object, physiotherapistMock.Object, dossierMock.Object);
            treatmentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            treatmentMock.Setup(m => m.GetById(1)).Returns(new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", 1, DateTime.Now, DateTime.Now.AddHours(2), 1));
            treatmentMock.Setup(m => m.GetAllByDossierId(1)).Returns(new List<Treatment>()
            {
                new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1)
            });

            ApplicationUser mockUser = new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl");
            physiotherapistMock.Setup(m => m.GetById(1)).Returns(new Physiotherapist("Simone", "Kerseboom", "simone@kerse.nl", "000", DateTime.Today.AddHours(4), DateTime.Today.AddHours(20), null, 100000));
            dossierMock.Setup(m => m.GetById(1)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, 1, DateTime.Now, null, DateTime.Today.AddHours(1), 2) { Patient = new Patient("Test", "Kees", "test@kees.nl", "000", "photo", DateTime.Now, "male", "1000000", null) { UserId = "10" } });
            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl"));
            dossierMock.Setup(m => m.CheckIfDossierIsRelated(1, mockUser)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, 1, DateTime.Now, null, DateTime.Now, 2));

            // Act
            var result = await treatmentController.Cancel(1);

            // Assert
            treatmentMock.Verify(m => m.EditTreatment(It.IsAny<Treatment>()), Times.Never);
        }

        [Fact]
        public async Task CanCancelTreatmentAfter24HoursAsync()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TreatmentController>>();
            var applicationUserMock = new Mock<IUserStore<ApplicationUser>>();
            var physiotherapistMock = new Mock<IPhysiotherapistRepository>();
            var dossierMock = new Mock<IDossierRepository>();

            var treatmentMock = new Mock<ITreatmentRepository>();
            TreatmentServices treatmentServices = new TreatmentServices(treatmentMock.Object, dossierMock.Object, physiotherapistMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Simone"),
                new Claim(ClaimTypes.Role, "Patient")
            }, "mock"));

            TreatmentController treatmentController = new TreatmentController(loggerMock.Object, treatmentMock.Object, treatmentServices, applicationUserMock.Object, physiotherapistMock.Object, dossierMock.Object);
            treatmentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };

            treatmentMock.Setup(m => m.GetById(1)).Returns(new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", 1, DateTime.Today.AddDays(2), DateTime.Today.AddDays(2).AddHours(2), 1));
            treatmentMock.Setup(m => m.GetAllByDossierId(1)).Returns(new List<Treatment>()
            {
                new Treatment("type", "Patient was absent for 2 days", "Room 1", "None", 1, DateTime.Parse("10:00"), DateTime.Parse("12:00"), 1)
            });

            ApplicationUser mockUser = new ApplicationUser("Simone", "Kerseboom", "simone@kerse.nl");
            physiotherapistMock.Setup(m => m.GetById(1)).Returns(new Physiotherapist("Simone", "Kerseboom", "simone@kerse.nl", "000", DateTime.Today.AddHours(4), DateTime.Today.AddHours(20), null, 100000));
            dossierMock.Setup(m => m.GetById(1)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, 1, DateTime.Now, null, DateTime.Today.AddHours(1), 2) { Patient = new Patient("Test", "Kees", "test@kees.nl", "000", "photo", DateTime.Now, "male", "1000000", null) { UserId = "10" } });
            applicationUserMock.Setup(m => m.FindByNameAsync("Simone", CancellationToken.None)).ReturnsAsync(mockUser);
            dossierMock.Setup(m => m.CheckIfDossierIsRelated(1, mockUser)).Returns(new Dossier("Stressed", 1, 1, 1, 1, 1, 1, DateTime.Now, null, DateTime.Now, 2));

            // Act
            var result = await treatmentController.Cancel(1);

            // Assert
            treatmentMock.Verify(m => m.EditTreatment(It.IsAny<Treatment>()), Times.Once);
        }
    }
}
