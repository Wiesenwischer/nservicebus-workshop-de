using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace ITOps.HealthMonitor
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
			services.AddHealthChecks()
				.AddCheck("self", () => HealthCheckResult.Healthy());

			services
				.AddHealthChecksUI()
                .AddInMemoryStorage();

			services.AddControllers();

			services.AddMvc();
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
			}

			app.UseRouting();

			app.UseStaticFiles();



			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
				{
					Predicate = _ => true,
					ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
				});
				endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
				{
					Predicate = r => r.Name.Contains("self")
				});

				endpoints.MapHealthChecksUI(setup =>
				{
					setup.AddCustomStylesheet("corporate.css");
				});

				endpoints.MapDefaultControllerRoute();
			});
		}
	}
}
