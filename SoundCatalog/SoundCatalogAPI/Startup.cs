using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SoundCatalog.Configuration;
using SoundCatalog.Data;
using SoundCatalog.Models;
using SoundCatalog.Services;
using System;
using System.Threading.Tasks;

namespace SoundCatalogAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Configure Cors
            services.AddCors(options => {
                options.AddPolicy("PolicySoundCatalogUI",
                builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            });

            services.AddMvc();

            // context configuration
            services.AddDbContext<SoundCatalogContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

            // jwt configuration
            var jwtOptions = Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
   
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = jwtOptions.Key,
                    };
                });

            services
                .AddIdentity<ApplicationUser, IdentityRole>(config =>
                {
                    config.SignIn.RequireConfirmedEmail = true;
                })
                .AddEntityFrameworkStores<SoundCatalogContext>()
                .AddDefaultTokenProviders();

            //loggin
            services.AddLogging();

            // configuration
            services.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));
            services.Configure<EmailOptions>(Configuration.GetSection(nameof(EmailOptions)));
            services.Configure<ClientOptions>(Configuration.GetSection(nameof(ClientOptions)));

            // services
            services.AddSingleton<IMessageServices, MessageServices>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("PolicySoundCatalogUI");

            app.UseAuthentication();

            app.UseMvc();

            
        }
    }
}
