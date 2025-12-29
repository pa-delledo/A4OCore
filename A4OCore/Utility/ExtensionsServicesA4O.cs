

using A4OCore.BLCore;
using A4OCore.Cfg;
using A4OCore.Models;
using A4OCore.Store;
using A4OCore.Store.DB.SQLLite;
using Microsoft.Extensions.DependencyInjection;


namespace A4OCore.Utility
{
    public static class A4OExtensionsServices
    {
        public static IServiceCollection AddA4ODependencies(this IServiceCollection services)
        {
            var configA4O = ConfigurationA4O.Get("appsettings.json");
            services.AddSingleton(configA4O);

            services.AddScoped<IStoreA4O, DBBase>();
            services.AddSingleton<A4O_MapIdRepository>();
            services.AddSingleton<A4O_User>();
            services.AddSingleton<IA4O_CheckEnumRepository, A4O_CheckEnumRepository>();
            //A4OServiceRegister reg = new(services);
            //services.AddSingleton<IServiceRegistrer>(reg);

            RegisterAllElementBLA4O<ElementBLA4O>(configA4O, services);
            RegisterAllElementBLA4O<IViewBLA4O>(configA4O, services);
            RegisterAllElementBLA4O<IActionBLA4O>(configA4O, services);

            services.AddSingleton<IBLRepository, BLRepository>();

            return services;
        }
        private static void RegisterAllElementBLA4O<T>(ConfigurationA4O cfg, IServiceCollection services)
        {
            //Type serviceTypeIViewBLA4ODto = typeof();
            //Type serviceTypeIActionBLA4ODto = typeof();

            Type serviceTypeElementBLA4O = typeof(T);
            int i = 0;
            foreach (var prj in cfg.BLProject)
            {

                var allElementBLA4O = ReflectionUtility.GetAllChildrenClass<T>(prj);


                foreach (Type item in allElementBLA4O)
                {
                    services.AddKeyedScoped(serviceTypeElementBLA4O, i++, item);
                }
            }
        }
    }
    //public interface IServiceRegistrer
    //{
    //    void Register( Type serviceType, Type implementationType, ServiceLifetime lifetime);
    ////    T GetServiceByServiceType<T>();
    ////    IEnumerable<T> GetAllServiceByServiceType<T>();

    //}
    //public class A4OServiceRegister : IServiceRegistrer
    //{
    //    private readonly IServiceCollection _serviceCollection;

    //    public A4OServiceRegister(IServiceCollection ser)
    //    {
    //        _serviceCollection = ser;
    //    }
    //    public void Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    //    {
    //        _serviceCollection.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
    //    }
    //}
    //    public IEnumerable<T> GetAllServiceByServiceType<T>()   
    //    {
    //        Type serviceType = typeof(T);
    //        return _serviceCollection.OfType<T>() ;
    //    }

    //    public T GetServiceByServiceType<T>()
    //    {
    //        Type serviceType = typeof(T);
    //        return (T)_serviceCollection.First(x=> serviceType.IsAssignableTo(x.ServiceType)).ImplementationInstance;
    //    }

    //    //public void Register(Type serviceType, Type implementationType)
    //    //{
    //    //    using var scope = app.Services.CreateScope();
    //    //    _serviceCollection.Bind(serviceType).To(implementationType).InSingletonScope();
    //    //}
    //    public void Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    //    {
    //        _serviceCollection.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
    //    }


    //}
}
