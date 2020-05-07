using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.IISIntegration;
using System.Text;

namespace LogFile
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            //configuration du cors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var conf = new Cloudbizz.Panel.Config();
            var key = conf.GetVariable("Cloudbizz.Api.Keys.Log");
            var correctKey = $"Bearer {key}";
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();

            }      
                      
           

          /* app.Use(async (context, next) =>
            {
                var authHeader = context.Request.Headers["Authorization"];
               

                //if not autheader ok
                if (!correctKey.Equals(authHeader))
                {
                    context.Response.StatusCode = 401;
                    byte[] data = Encoding.UTF8.GetBytes("Not authorized");
                    await context.Response.Body.WriteAsync(data, 0, data.Length);
                }  
                else
                {
                    await next();
                }
            });*/

            //app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseCors("CorsPolicy");
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
        }
    }
}
