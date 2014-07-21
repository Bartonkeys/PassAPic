using System.Web.Http;
using JPassAPic.Core.Repositories.Helpers;
using PassAPic.Contracts;
using PassAPic.Contracts.EmailService;
using PassAPic.Core.Email;
using PassAPic.Core.PushRegistration;
using PassAPic.Core.CloudImage;
using PassAPic.Core.Repositories;
using PassAPic.Core.WordManager;
using PassAPic.Repositories.Helpers;
using PassAPic.WordManager;

[assembly: WebActivator.PreApplicationStartMethod(typeof(PassAPic.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(PassAPic.App_Start.NinjectWebCommon), "Stop")]

namespace PassAPic.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            // Install our Ninject-based IDependencyResolver into the Web API config
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);

            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<RepositoryFactories>().To<RepositoryFactories>().InSingletonScope();
            kernel.Bind<IRepositoryProvider>().To<RepositoryProvider>();
            kernel.Bind<IUnitOfWork>().To<EFUnitOfWork>();
            kernel.Bind<IPushProvider>().To<PushProviderUrbanAirship>();
            kernel.Bind<ICloudImageProvider>().To<CloudImageProviderAzureBlob>();
            kernel.Bind<IWordManager>().To<WordnikManager>();
            kernel.Bind<IEmailService>().To<PlaceHolderEmailService>();
        }        
    }
}
