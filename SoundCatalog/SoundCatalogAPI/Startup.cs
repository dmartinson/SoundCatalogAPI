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
using Swashbuckle.AspNetCore.Swagger;
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

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "SoundCatalog API", Version = "v1" });
            });

            // services
            services.AddSingleton<IMessageServices, MessageServices>();
            services.AddTransient<UsersInitialize>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, UsersInitialize usersSeeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("PolicySoundCatalogUI");

            app.UseAuthentication();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SoundCatalog API V1");
            });

            app.UseMvc();

            // Initialize data
            usersSeeder.Seed().Wait();
        }
    }
}
