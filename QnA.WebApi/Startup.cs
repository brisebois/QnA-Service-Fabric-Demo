using System.Web.Http;
using Microsoft.Owin.Cors;
using Owin;

namespace QnA.WebApi
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
           
            //config.Routes.MapHttpRoute(
            //    name: "API Default",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            config.MapHttpAttributeRoutes();
            
            appBuilder.UseCors(CorsOptions.AllowAll)
                      .UseWebApi(config);


            config.EnsureInitialized();
        }
    }
}