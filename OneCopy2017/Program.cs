using System;
using System.Linq;
using OneCopy2017.DataObjects;
using OneCopy2017.Services;
using OneCopy2017.TinyIoc;

namespace OneCopy2017
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = RegisterServices();
            
            container.Resolve<ConfigService>().Load();
            container.Resolve<App>().Run();

            Console.ReadLine();
        }

        private static TinyIoCContainer RegisterServices()
        {
            var container = TinyIoCContainer.Current;

            var typesToRegister =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.FullName.StartsWith("OneCopy2017") && t.FullName.EndsWith("Service"))
                    .ToList();

            foreach (var type in typesToRegister)
                container.Register(type).AsSingleton();

            container.Register<App>().AsSingleton();

            return container;
        }
    }
}