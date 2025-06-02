using Core.Domain;
using Core.DomainServices;
using Core.Services;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fysio
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            /*services.AddDbContext<ClientDbContext>(options => options.UseSqlServer(
                @"Server=tcp:localhost,1433\MSSQLSERVER;Initial Catalog=fysio;Trusted_Connection=True;User ID=sa;Password=wachtwoord;Connection Timeout=30;"));
            services.AddDbContext<SecurityDbContext>(options => options.UseSqlServer(
                @"Server=tcp:localhost,1433\MSSQLSERVER;Initial Catalog=fysio_identity;Trusted_Connection=True;User ID=sa;Password=wachtwoord;Connection Timeout=30;"));*/

            //Environment.SetEnvironmentVariable("API_URL", "https://localhost:44334/api/");
            Environment.SetEnvironmentVariable("API_URL", "https://fysio000api.azurewebsites.net/api/");

            services.AddDbContext<ClientDbContext>(options => options.UseSqlServer(
                @"Server=tcp:fysio000.database.windows.net,1433;Initial Catalog=fysio;Persist Security Info=False;User ID=kaas;Password=Oo4XjNHCwHlWIV1byu47;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));
            services.AddDbContext<SecurityDbContext>(options => options.UseSqlServer(
                @"Server=tcp:fysio000.database.windows.net,1433;Initial Catalog=fysio_identity;Persist Security Info=False;User ID=kaas;Password=Oo4XjNHCwHlWIV1byu47;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            }).AddEntityFrameworkStores<SecurityDbContext>().AddDefaultTokenProviders();

            var mailKitOptions = Configuration.GetSection("Email").Get<MailKitOptions>();
            services.AddMailKit(config => { config.UseMailKit(mailKitOptions); });

            //Patient
            services.AddScoped<IPatientServices, PatientServices>();
            services.AddScoped<IPatientRepository, PatientRepository>();

            //Physiotherapist
            services.AddScoped<IPhysiotherapistServices, PhysiotherapistServices>();
            services.AddScoped<IPhysiotherapistRepository, PhysiotherapistRepository>();

            //Dossier
            services.AddScoped<IDossierServices, DossierServices>();
            services.AddScoped<IDossierRepository, DossierRepository>();

            //Treatment
            services.AddScoped<ITreatmentServices, TreatmentServices>();
            services.AddScoped<ITreatmentRepository, TreatmentRepository>();

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/home";
            });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
