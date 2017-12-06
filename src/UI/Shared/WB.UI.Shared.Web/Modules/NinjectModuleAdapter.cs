using System;
using System.Linq;
using System.Web.Http.Filters;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Ninject.Web.Mvc.FilterBindingSyntax;
using Ninject.Web.WebApi.FilterBindingSyntax;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public  class NinjectModuleAdapter<TModule> : NinjectModuleAdapter
        where TModule : IModule
    {
        private readonly TModule module;

        public NinjectModuleAdapter(TModule module)
        {
            this.module = module;
        }

        public override void Load()
        {
            this.module.Load(this);
        }
    }


    public abstract class NinjectModuleAdapter : NinjectModule, IIocRegistry
    {
        void IIocRegistry.Bind<TInterface, TImplementation>()
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>();
        }

        void IIocRegistry.Bind<TInterface1, TInterface2, TImplementation>() 
        {
            this.Kernel.Bind<TInterface1, TInterface2>().To<TImplementation>();
        }

        void IIocRegistry.Bind<TInterface, TImplementation>(params ConstructorArgument[] constructorArguments)
        {
            var syntax = this.Kernel.Bind<TInterface>().To<TImplementation>();

            foreach (var constructorArgument in constructorArguments)
            {
                object Callback(IContext context) => constructorArgument.Value.Invoke(new NinjectModuleContext(context));
                syntax.WithConstructorArgument(constructorArgument.Name, (Func<IContext, object>) Callback);
            }
        }

        void IIocRegistry.Bind<TImplementation>()
        {
            this.Kernel.Bind<TImplementation>().To<TImplementation>();
        }

        void IIocRegistry.BindAsSingleton<TInterface, TImplementation>()
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope();
        }

        void IIocRegistry.BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue)
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope()
                .WithConstructorArgument(argumentName, argumentValue);
        }

        void IIocRegistry.BindToRegisteredInterface<TInterface, TRegisteredInterface>()
        {
            this.Kernel.Bind<TInterface>().ToMethod<TInterface>(context => context.Kernel.Get<TRegisteredInterface>());
        }

        public void BindToMethod<T>(Func<T> func)
        {
            this.Kernel.Bind<T>().ToMethod(ctx => func());
        }

        public void BindToConstant<T>(Func<T> func)
        {
            this.Kernel.Bind<T>().ToMethod(ctx => func()).InSingletonScope();
        }

        public void BindAsSingleton(Type @interface, Type implementation)
        {
            this.Kernel.Bind(@interface).To(implementation).InSingletonScope();
        }

        public void BindGeneric(Type implementation)
        {
            this.Kernel.Bind(implementation);
        }

        public void RegisterDenormalizer<T>() where T : IEventHandler
        {
            this.RegisterDenormalizer<T>(this.Kernel);
        }

        internal void BindMvcFilter<T>(System.Web.Mvc.FilterScope filterScope, int? order)
        {
            this.Kernel.BindFilter<T>(filterScope, order);
        }

        internal void BindMvcFilterWhenActionMethodHasNoAttribute<T, TAttribute>(System.Web.Mvc.FilterScope filterScope, int? order)
        {
            this.Kernel.BindFilter<T>(filterScope, order).WhenActionMethodHasNo<TAttribute>();
        }

        internal void BindHttpFilter<T>(System.Web.Http.Filters.FilterScope filterScope, int? order) where T : IFilter
        {
            this.Kernel.BindHttpFilter<T>(filterScope);
        }

        internal void BindHttpFilterWhenActionMethodHasNoAttribute<T>(System.Web.Http.Filters.FilterScope filterScope, int? order, Type attributeType) where T : IFilter
        {
            this.Kernel.BindHttpFilter<T>(filterScope)
                .When((controllerContext, actionDescriptor) => !actionDescriptor.GetCustomAttributes(attributeType).Any());
        }

        public void RegisterDenormalizer<T>(IKernel kernel) where T : IEventHandler
        {
            Type[] eventHandlerTypes = { typeof(IEventHandler), typeof(IEventHandler<>) };
            var denormalizerType = typeof(T);

            foreach (var interfaceType in eventHandlerTypes)
            {
                kernel.Bind(interfaceType).To(denormalizerType);

                if (!interfaceType.IsGenericType) continue;

                GetValue(kernel, denormalizerType, interfaceType);
            }
        }

        private static void GetValue(IKernel kernel, Type factoryType, Type interfaceType)
        {
            var interfaceImplementations = factoryType.GetInterfaces()
                .Where(t => t.IsGenericType)
                .Where(t => t.GetGenericTypeDefinition() == interfaceType);

            var genericInterfaceTypes =
                interfaceImplementations.Select(
                    interfaceImplementation => interfaceType.MakeGenericType(interfaceImplementation.GetGenericArguments()));

            foreach (var genericInterfaceType in genericInterfaceTypes)
            {
                kernel.Bind(genericInterfaceType).To(factoryType);
            }
        }
    }
}