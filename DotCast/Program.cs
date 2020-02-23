using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Microsoft.Owin.Hosting;

namespace DotCast
{
    class Program
    {
        private static WindsorContainer _container;

        static void Main(string[] args)
        {


            string url = "http://*:9783/";
            using (WebApp.Start(url))
            {
                Console.WriteLine("Podcast server started");
                Console.ReadKey();
            }
        }
    }
}
