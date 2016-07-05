using System.Collections.Generic;
using System.Web.Http;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
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
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            
            var fileServerOptions = new FileServerOptions
            {
                DefaultFilesOptions =
                {
                    DefaultFileNames = new List<string> {"index.htm"}
                },
                FileSystem = new PhysicalFileSystem("Client"),
                EnableDefaultFiles = true,
                EnableDirectoryBrowsing = false
            };

            appBuilder.UseCors(CorsOptions.AllowAll)
                      .UseWebApi(config)
                      .UseFileServer(fileServerOptions)
                      .UseStaticFiles(new StaticFileOptions
                      {
                          ServeUnknownFileTypes = true
                      });
            
            

            config.EnsureInitialized();
        }
    }
}