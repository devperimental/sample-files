using Microsoft.AspNetCore.Builder;
using Helper.Startup.Types;

namespace Helper.Startup.Middleware
{
    public static class ApiMiddleware
    {
        public static void UseApi(this IApplicationBuilder app, BootstrapConfiguration bootstrapConfiguration, Action customMiddleware)
        {
            app.UseForwardedHeaders();

            if(bootstrapConfiguration.EnvironmentName != EnvironmentName.Production)
            {
                app.UseSwagger(c =>
                {
                    c.RouteTemplate = $"swagger/{{documentName}}/swagger.json";
                });

                app.UseSwaggerUI(c =>
                {
                    c.InjectJavascript("https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/4.17.0/swagger-ui-bundle.js");
                    c.RoutePrefix= "swagger";
                    c.SwaggerEndpoint("v1/swagger.json", "v1");
                });
            }

            if (bootstrapConfiguration.EnvironmentName == EnvironmentName.Production)
            {
                app.UseHttpsRedirection();
            }

            if (!string.IsNullOrEmpty(bootstrapConfiguration.CorsOrigins))
            {
                app.UseCors();
            }

            customMiddleware();
        }
    }
}