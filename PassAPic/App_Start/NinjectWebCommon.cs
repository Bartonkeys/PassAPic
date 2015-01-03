using PassAPic.Contracts;
using PassAPic.Contracts.EmailService;
using PassAPic.Core.CloudImage;
using PassAPic.Core.Email;
using PassAPic.Core.PushRegistration;
using PassAPic.Core.Repositories;
using PassAPic.Core.WordManager;
using PassAPic.WordManager;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(PassAPic.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(PassAPic.App_Start.NinjectWebCommon), "Stop")]

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
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IDataContext>().To<EfDataContext>();
            kernel.Bind<IPushProvider>().To<PushProviderYerma>();
            kernel.Bind<ICloudImageProvider>().To<CloudImageProviderAzureBlob>();
            kernel.Bind<IWordManager>().To<LocalWordManager>();
            kernel.Bind<IEmailService>().To<SendGridEmailService>();
        }        
    }
}
