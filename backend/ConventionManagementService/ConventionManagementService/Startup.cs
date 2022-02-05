using ConventionManagementService.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;

namespace ConventionManagementService
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
            IConfigurationSection cosmosDbConfig = Configuration.GetSection("CosmosDb");
            services.Configure<CosmosDbConfig>(cosmosDbConfig);
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://{Configuration["Auth0:Domain"]}/";
                    options.Audience = Configuration["Auth0:Audience"];
                    // If the access token does not have a `sub` claim, `User.Identity.Name` will be `null`. Map it to a different claim by setting the NameClaimType below.
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CRUD", policy =>
                                  policy.RequireClaim("permissions", "crud:conventions"));
            });
            services.AddSingleton<IConventionManager, DatabaseConventionManager>();

            if (EnableSwagger())
            {
                // Add Swagger Documentation
                services.AddSwaggerGen();
            }
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                        builder =>
                        {
                            //builder.AllowAnyOrigin();
                            builder.WithOrigins("https://happy-sky-0a342490f.1.azurestaticapps.net/",
                                                              "http://locahlost")
                                                              .AllowAnyHeader()
                                                              .AllowAnyMethod();
                        });
            });

            services.AddApplicationInsightsTelemetry();

            services.AddControllers()
                    .AddMvcOptions(options =>
                    {
                        options.Filters.Add(new HttpResponseExceptionFilter());
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            if (EnableSwagger())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(options => options.SetIsOriginAllowed(IsOriginAllowed).AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseExceptionHandler();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private bool IsOriginAllowed(string origin)
        {
            string[] allowedOrigins = new[] {"happy-sky-0a342490f.1.azurestaticapps.net", "locahlost" };
            return allowedOrigins.Any(item => origin.Contains(item));
        }

        private bool EnableSwagger()
        {
            string enableSwagger = Configuration["EnableSwagger"];
            return enableSwagger != null && bool.Parse(enableSwagger);
        }
    }
}
