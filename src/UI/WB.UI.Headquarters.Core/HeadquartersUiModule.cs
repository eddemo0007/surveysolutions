using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Services;
using WB.UI.Headquarters.Services.Impl;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters
{
    public class HeadquartersUiModule : IModule
    {
        private readonly IConfiguration configuration;

        public HeadquartersUiModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Load(IIocRegistry registry)
        {
            var services = registry;

            services.AddScoped<ServiceApiKeyAuthorization>();

            registry.Bind<IInterviewerProfileFactory, InterviewerProfileFactory>();
            registry.Bind<IExportServiceApiFactory, ExportServiceApiFactory>();
            registry.Bind<IImageProcessingService, ImageProcessingService>();
            registry.Bind<IAudioProcessingService, AudioProcessingService>();
            services.Bind<ICaptchaService, WebCacheBasedCaptchaService>();
            registry.Bind<IWebInterviewInterviewEntityFactory, WebInterviewInterviewEntityFactory>();
            registry.Bind<IWebNavigationService, WebNavigationService>();
            registry.Bind<IReviewAllowedService, ReviewAllowedService>();
            registry.Bind<IQuestionnaireAssemblyAccessor, QuestionnaireAssemblyAccessor>();
            registry.BindAsSingleton<IWebInterviewNotificationService, WebInterviewNotificationService>();
            registry.BindToMethodInSingletonScope(context => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
            }).CreateMapper());
            var captchaSection = this.configuration.CaptchaOptionsSection();

            ConfigureEventBus(registry);

            registry.Bind<IDesignerApiFactory, DesignerApiFactory>();
            registry.BindToMethod(ctx => ctx.Resolve<IDesignerApiFactory>().Get());

            var config = captchaSection.Get<CaptchaConfig>() ?? new CaptchaConfig();
            var provider = config.CaptchaType;

            switch (provider)
            {
                case CaptchaProviderType.Recaptcha:
                    services.AddTransient<IRecaptchaService, RecaptchaService>();
                    services.AddTransient<ICaptchaProvider, RecaptchaProvider>();
                    break;
                case CaptchaProviderType.Hosted:
                    services.AddTransient<ICaptchaProvider, HostedCaptchaProvider>();
                    services.AddTransient<IHostedCaptcha, HostedCaptchaProvider>();
                    break;
                default:
                    services.AddTransient<ICaptchaProvider, NoCaptchaProvider>();
                    break;
            }

            registry.Bind<ILoggerProvider, SerilogLoggerProvider>();

            registry.BindToMethod<ILogger>(context =>
            {
                if (context.MemberDeclaringType != null)
                    return new SerilogLogger(context.MemberDeclaringType);

                return new SerilogLogger();
            });
        }

        private void ConfigureEventBus(IIocRegistry registry)
        {
            EventHandlersConfig eventBusConfig = configuration.GetSection("EventHandlers").Get<EventHandlersConfig>();

            var eventBusSettings =  new EventBusSettings
            {
                DisabledEventHandlerTypes =
                    eventBusConfig.Disabled
                        .Select(Type.GetType)
                        .Where(x => x != null)
                        .ToArray(),

                EventHandlerTypesWithIgnoredExceptions =
                    eventBusConfig.IgnoredException
                        .Select(Type.GetType)
                        .Where(x => x != null)
                        .ToArray(),
            };

            registry.BindToConstant(() => eventBusSettings);
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}