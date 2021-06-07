using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using Elastic.Apm.AspNetCore;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.TraceListener;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Web;

namespace ExampleApi
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
            #region Localization

            services.AddLocalization(o => { o.ResourcesPath = "Resources"; });
            var supportedCultures = new CultureInfo[] {
                new CultureInfo("es-MX"),
                new CultureInfo("en-US"),
                new CultureInfo("pt-BR"),
            };

            services.Configure<RequestLocalizationOptions>(s =>
            {
                s.DefaultRequestCulture = new RequestCulture(culture: "es-MX", uiCulture: "es-MX");
                s.SupportedCultures = supportedCultures;
                s.SupportedUICultures = supportedCultures;
            });

            #endregion Localization

            services.AddControllers();

            #region ApplicationInsights

            services.AddSingleton(typeof(ITelemetryChannel), new InMemoryChannel() { MaxTelemetryBufferCapacity = 19898 });

            services.AddApplicationInsightsTelemetry(Configuration);

            #endregion ApplicationInsights

            #region Swagger

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Example.Api",
                    Version = "v1",
                    Contact = new OpenApiContact()
                    {
                        Name = "Company Name",
                    },
                    Description = "Api que sirve de ejemplo para cargar los elementos de Configuración Inicial"
                });
            });

            #endregion Swagger

            //services.TryAddSingleton<IServices, Services>();

            services.AddHttpContextAccessor();

            services.AddMvc().AddViewLocalization().AddDataAnnotationsLocalization().AddXmlSerializerFormatters().AddXmlDataContractSerializerFormatters();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region Localization
            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            #endregion Localization

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Trace.Listeners.Add(new ApplicationInsightsTraceListener());

            #region Swagger

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                //Para Debug en Kestrel
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API V1");

                //Para Integración en IIS
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "Example.Api");

            });

            #endregion Swagger

            //para usar un SVC del servicio
            //app.UseSoapEndpoint<IServiceScope>("/Services.svc", new BasicHttpBinding(), SoapSerializar.XmlSerializer);

            app.UseElasticApm(Configuration);

            //Ayuda a Crear elemen tos en memoria por sesión (Equivalente en .Net Framework)
            //HttpContext.Configure(app.ApplicationServices.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>());

            #region Logger
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseLocalLog"]))
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microdoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        ConfigurationManager.AppSettings["LogPath"],
                        fileSizeLimitBytes: 1_000_000,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1))
                    .CreateLogger();

            }
            else {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.WithElasticApmCorrelationInfo()
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(ConfigurationManager.AppSettings["ElasticSearchUrl"]))
                    { 
                        AutoRegisterTemplate = true,
                        /*ModifyConnectionSettings = x =>
                            x.BasicAuthentication(
                                ConfigurationManager.AppSettings["ElasticSearchUserName"],
                                ConfigurationManager.AppSettings["ElasticSearchPassword"]),*/
                        IndexFormat = ConfigurationManager.AppSettings["EllasticSearchIndex"],
                        CustomFormatter = new EcsTextFormatter()
                    }).CreateLogger();


            }
            #endregion Logger

            app.UseSession();

            app.UseStaticFiles();
        }
    }
}
