﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using NConfig;
using NLog;
using Owin;
using Quartz;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Monitoring;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.DataAnnotations;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters
{
    public class Startup
    {
        internal static void SetupNConfig()
        {
            NConfigurator.RegisterSectionMerger(new DeepMerger<HqSecuritySection>());
            NConfigurator.UsingFiles(@"~\Configuration\Headquarters.Web.config").SetAsSystemDefault();
        }

        static Startup()
        {
            SetupNConfig();
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(@"en-US");
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            //HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
            //NpgsqlLogManager.Provider = new NLogNpgsqlLoggingProvider();
            //NpgsqlLogManager.IsParameterLoggingEnabled = true;

        }

        public void Configuration(IAppBuilder app)
        {
            EnsureJsonStorageForErrorsExists();
            app.Use(RemoveServerNameFromHeaders);

            var autofacKernel = AutofacConfig.CreateKernel();

            autofacKernel.ContainerBuilder.RegisterHubs(Assembly.GetAssembly(typeof(WebInterviewHub)));
            autofacKernel.ContainerBuilder.RegisterControllers(typeof(Startup).Assembly);
            autofacKernel.ContainerBuilder.RegisterApiControllers(typeof(Startup).Assembly);

            //autofacKernel.ContainerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            autofacKernel.ContainerBuilder.RegisterType<Autofac.Integration.SignalR.AutofacDependencyResolver>()
                .As<Microsoft.AspNet.SignalR.IDependencyResolver>().SingleInstance();
            
            //todo:af remove
            //temp logging
            autofacKernel.ContainerBuilder.RegisterModule<LogRequestModule>();
            
            autofacKernel.ContainerBuilder
                .RegisterType<CustomMVCDependencyResolver>()
                .As<System.Web.Mvc.IDependencyResolver>()
                .SingleInstance();

            autofacKernel.ContainerBuilder
                .RegisterType<AutofacServiceLocatorAdapter>()
                .As<IServiceLocator>()
                .InstancePerLifetimeScope();

            //no scope involved activity should be used
            autofacKernel.Init().Wait();

            var container = autofacKernel.Container;

            var config = new HttpConfiguration();
            var resolver = new CustomWebApiDependencyResolver(container);

            config.DependencyResolver = resolver;
            GlobalConfiguration.Configuration.DependencyResolver = resolver;

            HubConfiguration hubConfig = new HubConfiguration();

            hubConfig.Resolver = container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>();
            GlobalHost.DependencyResolver = container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>();

            var resolv = container.Resolve<System.Web.Mvc.IDependencyResolver>();
            //resolver.

            DependencyResolver.SetResolver(resolv);
            ModelBinders.Binders.DefaultBinder = new AutofacBinderResolver(container);

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));

            //TODO:AF resolve
            /*var perRequestModule = new OnePerRequestHttpModule();
            // onPerRequest scope implementation. Collecting all perRequest instances after all requests
            app.Use(async (ctx, next) =>
            {
                try
                {
                    if (ctx.Request.CallCancelled.IsCancellationRequested) return;

                    await next();
                }
                finally
                {
                    perRequestModule.DeactivateInstancesForCurrentHttpRequest();
                }
            });
            kernel.Inject(perRequestModule); // will keep reference to perRequestModule in Kernel instance
            */

            //GlobalFilters.Filters.Add(new TransactionFilter());
            //config.Filters.Add(new ApiTransactionFilter());

            app.UseAutofacMiddleware(container);
            app.UseWebApi(config);

            var logger = ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<Startup>();
            logger.Info($@"Starting Headquarters {ServiceLocator.Current.GetInstance<IProductVersion>()}");

            ConfigureAuth(app);
            InitializeAppShutdown(app);
            InitializeMVC();
            ConfigureWebApi(app);

            using (var scope = container.BeginLifetimeScope())
            {
                scope.Resolve<IProductVersionHistory>().RegisterCurrentVersion();
            }

            Exceptional.Settings.ExceptionActions.AddHandler<TargetInvocationException>((error, exception) =>
            {
                void AddAllSqlData(Exception e)
                {
                    if (e is Npgsql.PostgresException pe)
                    {
                        error.AddCommand(new Command(@"NpgSql", pe.Statement.SQL));
                    }

                    if (e.InnerException != null)
                    {
                        AddAllSqlData(e.InnerException);
                    }
                }

                AddAllSqlData(exception);
            });
        }


        private void ConfigureWebApi(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.Formatters.Add(new FormMultipartEncodedMediaTypeFormatter());

            GlobalConfiguration.Configure(WebApiConfig.Register);
            WebApiConfig.Register(config);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            WebInterviewModule.Configure(app, HqWebInterviewModule.HubPipelineModules);
            app.Use(SetSessionStateBehavior).UseStageMarker(PipelineStage.MapHandler);

            app.UseAutofacWebApi(config);
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            var applicationSecuritySection = NConfigurator.Default.GetSection<HqSecuritySection>(@"applicationSecurity");

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString(@"/Account/LogOn"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator
                        .OnValidateIdentity<HqUserManager, HqUser, Guid>(
                            validateInterval: TimeSpan.FromMinutes(30),
                            regenerateIdentityCallback: (manager, user) => manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie),
                            getUserIdCallback: (id) => Guid.Parse(id.GetUserId())),

                    OnApplyRedirect = ctx =>
                    {
                        if (!IsAjaxRequest(ctx.Request) && !IsApiRequest(ctx.Request) && !IsBasicAuthApiUnAuthRequest(ctx.Response))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    }
                },
                ExpireTimeSpan = TimeSpan.FromHours(applicationSecuritySection.CookieSettings.ExpirationTime),
                SlidingExpiration = applicationSecuritySection.CookieSettings.SlidingExpiration,
                CookieName = applicationSecuritySection.CookieSettings.Name,
                CookieHttpOnly = applicationSecuritySection.CookieSettings.HttpOnly
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        }

        private static bool IsApiRequest(IOwinRequest request)
        {
            var userAgent = request.Headers[@"User-Agent"];
            return (userAgent?.ToLowerInvariant().Contains(@"org.worldbank.solutions.") ?? false) || (userAgent?.Contains(@"okhttp/") ?? false);
        }

        private static bool IsBasicAuthApiUnAuthRequest(IOwinResponse response)
        {
            return response.Headers[ApiBasicAuthAttribute.AuthHeader] != null;
        }

        private static bool IsAjaxRequest(IOwinRequest request)
        {
            IReadableStringCollection query = request.Query;
            if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
            {
                return true;
            }
            IHeaderDictionary headers = request.Headers;
            return ((headers != null) && (headers["X-Requested-With"] == "XMLHttpRequest"));
        }

        private static Task SetSessionStateBehavior(IOwinContext context, Func<Task> next)
        {
            // Depending on the handler the request gets mapped to, session might not be enabled. Force it on.
            HttpContextBase httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
            httpContext.SetSessionStateBehavior(SessionStateBehavior.Required);
            return next();
        }

        private static Task RemoveServerNameFromHeaders(IOwinContext context, Func<Task> next)
        {
            context.Response.Headers.Remove(@"Server");
            return next.Invoke();
        }

        private static void InitializeAppShutdown(IAppBuilder app)
        {
            var properties = new AppProperties(app.Properties);
            CancellationToken token = properties.OnAppDisposing;
            if (token != CancellationToken.None)
                token.Register(OnShutdown);
        }

        private static void OnShutdown()
        {
            CommonMetrics.StateFullInterviewsCount.Set(0);

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info(@"Ending application.");
            logger.Info(@"ShutdownReason: " + HostingEnvironment.ShutdownReason);

            ServiceLocator.Current.GetInstance<IScheduler>()?.Shutdown();

            if (HostingEnvironment.ShutdownReason != ApplicationShutdownReason.HostingEnvironment) return;

            var httpRuntimeType = typeof(HttpRuntime);
            var httpRuntime = httpRuntimeType.InvokeMember(
                "_theRuntime",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                null, null, null) as HttpRuntime;

            var shutDownMessage = httpRuntimeType.InvokeMember(
                "_shutDownMessage",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, httpRuntime, null) as string;

            string shutDownStack = httpRuntimeType.InvokeMember("_shutDownStack",
                BindingFlags.NonPublic
                | BindingFlags.Instance
                | BindingFlags.GetField,
                null,
                httpRuntime,
                null) as string;

            logger.Info(@"ShutDownMessage: " + shutDownMessage);
            logger.Info(@"ShutDownStack: " + shutDownStack);
        }

        private static void InitializeMVC()
        {
            MvcHandler.DisableMvcResponseHeader = true;

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterHttpFilters(GlobalConfiguration.Configuration.Filters);
            RegisterWebApiFilters(GlobalConfiguration.Configuration.Filters);

            DataAnnotationsConfig.RegisterAdapters();

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new NlogExceptionFilter());
            filters.Add(new RequireSecureConnectionAttribute());
            filters.Add(new NoCacheAttribute());
            filters.Add(new InstallationAttribute(), 100);
        }

        public static void RegisterHttpFilters(HttpFilterCollection filters)
        {
            filters.Add(new ErrorLoggerFilter());
        }

        public static void RegisterWebApiFilters(HttpFilterCollection filters)
        {
        }

        private void EnsureJsonStorageForErrorsExists()
        {
            if (StackExchange.Exceptional.Exceptional.Settings.DefaultStore is JSONErrorStore exceptionalConfig)
            {
                var jsonStorePath = exceptionalConfig.Settings.Path;
                var jsonStorePathAbsolute = HostingEnvironment.MapPath(jsonStorePath);

                if (!Directory.Exists(jsonStorePathAbsolute))
                {
                    Directory.CreateDirectory(jsonStorePathAbsolute);
                }
            }
        }
    }

    public class CustomMVCDependencyResolver : Autofac.Integration.Mvc.AutofacDependencyResolver
    {
        //todo:af
        //try to extract to bindings
        //private IServiceLocator locator;

        public CustomMVCDependencyResolver(ILifetimeScope container) : base(container)
        {
        }

        public CustomMVCDependencyResolver(ILifetimeScope container, Action<ContainerBuilder> configurationAction) : base(container, configurationAction)
        {
        }

        public CustomMVCDependencyResolver(ILifetimeScope container, ILifetimeScopeProvider lifetimeScopeProvider) : base(container, lifetimeScopeProvider)
        {
        }

        public CustomMVCDependencyResolver(ILifetimeScope container, ILifetimeScopeProvider lifetimeScopeProvider, Action<ContainerBuilder> configurationAction) : base(container, lifetimeScopeProvider, configurationAction)
        {
        }

        public override object GetService(Type serviceType)
        {
            //to preserve scope
            var serviceLocator = this.RequestLifetimeScope.Resolve<IServiceLocator>(new NamedParameter("kernel", this.RequestLifetimeScope));
            return base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            var serviceLocator = this.RequestLifetimeScope.Resolve<IServiceLocator>(new NamedParameter("kernel", this.RequestLifetimeScope));
            return base.GetServices(serviceType);
        }
    }


    /// <summary>
    /// Autofac implementation of the <see cref="T:System.Web.Http.Dependencies.IDependencyResolver" /> interface.
    /// </summary>
    public class CustomWebApiDependencyResolver : AutofacWebApiDependencyResolver
    {
        /// <summary>Try to get a service of the given type.</summary>
        /// <param name="serviceType">Type of service to request.</param>
        /// <returns>An instance of the service, or null if the service is not found.</returns>
        public new object GetService(Type serviceType)
        {
            var scope = (BeginScope() as AutofacWebApiDependencyScope).LifetimeScope;
            var serviceLocator = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));

            return base.GetService(serviceType);
        }

        public new IEnumerable<object> GetServices(Type serviceType)
        {
            var scope = (BeginScope() as AutofacWebApiDependencyScope).LifetimeScope;
            var serviceLocator = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));

            return base.GetServices(serviceType);
        }

        public CustomWebApiDependencyResolver(ILifetimeScope container) : base(container)
        {
        }
    }

    /*public class CustomWebApiDependencyResolver : AutofacWebApiDependencyResolver
    {
        public CustomWebApiDependencyResolver(ILifetimeScope container) : base(container)
        {
        }
        public override object GetService(Type serviceType)
        {
            var serviceLocator = this.RequestLifetimeScope.Resolve<IServiceLocator>(new NamedParameter("kernel", this.RequestLifetimeScope));
            /*if (serviceType == typeof(IServiceLocator))
            {
                return locator ?? (locator = new MvcAutofacDependencyResolverServiceLocatorAdaptor(this));
            }#1#

            return base.GetService(serviceType);
        }
    }*/

    public class LogRequestModule : Autofac.Module
    {
        public int depth = 0;

        private string trace;

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry,
            IComponentRegistration registration)
        {
            registration.Preparing += RegistrationOnPreparing;
            registration.Activating += RegistrationOnActivating;
            base.AttachToComponentRegistration(componentRegistry, registration);
        }

        private string GetPrefix()
        {
            return new string('-', /*depth **/ 2);
        }

        private void RegistrationOnPreparing(object sender, PreparingEventArgs preparingEventArgs)
        {
            trace += ($"{GetPrefix()}Resolving  {preparingEventArgs.Component.Activator.LimitType} \r\n");
            depth++;
        }

        private void RegistrationOnActivating(object sender, ActivatingEventArgs<object> activatingEventArgs)
        {
            depth--;
            trace += ($"{GetPrefix()}Activating {activatingEventArgs.Component.Activator.LimitType}\r\n");
        }
    }
}
