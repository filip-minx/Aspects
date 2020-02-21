using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minx.Aspects
{
    public static class Aspects
    {
        private static bool initialized = false;

        private static ProxyGenerator ProxyGenerator = new ProxyGenerator();
        
        public static IDictionary<Type, IList<IInterceptor>> TypeAspects = new Dictionary<Type, IList<IInterceptor>>();

        static Aspects()
        {
            Init();
        }

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            var aspectTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly
                    .GetTypes()
                    .Where(type => type.GetCustomAttribute<AspectAttribute>() != null))
                .Aggregate((a, b) => a.Concat(b));

            foreach (var aspectType in aspectTypes)
            {
                var aspectTargetType = aspectType.GetCustomAttribute<AspectAttribute>().TargetType;

                var aspectInterceptor = Activator.CreateInstance(
                    type: typeof(AspectInterceptor<>).MakeGenericType(aspectTargetType),
                    args: new object[] { Activator.CreateInstance(aspectType) });

                if (!TypeAspects.TryGetValue(aspectTargetType, out var aspectsList))
                {
                    aspectsList = new List<IInterceptor>();
                    TypeAspects.Add(aspectTargetType, aspectsList);
                }

                aspectsList.Add(aspectInterceptor as IInterceptor);
            }
        }

        public static TTarget AddAspects<TTarget, TAspect>(TTarget target, params TAspect[] aspects) where TAspect : class, TTarget
        {
            var aspectInterceptors = aspects.Select(aspect =>
            {
                var interceptor = Activator.CreateInstance(typeof(AspectInterceptor<TAspect>), aspect);
                
                return interceptor as IInterceptor;
            }).ToList();

            target = GetUnproxiedInstance(target);

            var targetType = target.GetType();
            var interfaces = targetType.GetInterfaces();

            if (TypeAspects.TryGetValue(targetType, out var interceptors))
            {
                aspectInterceptors.AddRange(interceptors);
            }

            return (TTarget)ProxyGenerator.CreateInterfaceProxyWithTarget(
                interfaceToProxy: typeof(TTarget),
                additionalInterfacesToProxy: interfaces,
                target: target,
                interceptors: aspectInterceptors.ToArray());
        }

        public static TTarget AddAspects<TTarget>(TTarget target)
        {
            target = GetUnproxiedInstance(target);

            var targetType = target.GetType();
            var interfaces = targetType.GetInterfaces();
            
            if (!TypeAspects.TryGetValue(interfaces[0], out var aspectInterceptors))
            {
                aspectInterceptors = new IInterceptor[] { };
            }

            return (TTarget)ProxyGenerator.CreateInterfaceProxyWithTarget(
                interfaceToProxy: typeof(TTarget),
                additionalInterfacesToProxy: interfaces,
                target: target,
                interceptors: aspectInterceptors.ToArray());
        }

        private static T GetUnproxiedInstance<T>(T proxy)
        {
            return ProxyUtil.IsProxy(proxy)
                ? (T)(proxy as IProxyTargetAccessor).DynProxyGetTarget()
                : proxy;
        }
    }
}
