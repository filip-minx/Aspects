using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minx.Aspects
{
    /// <summary>
    /// Class providing support for aspect oriented programming paradigm.
    /// </summary>
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
                .Select(assembly => assembly.GetTypes()
                    .Where(type => type.GetCustomAttribute<AspectAttribute>() != null))
                .Aggregate((a, b) => a.Concat(b));

            foreach (var aspectType in aspectTypes)
            {
                var aspectOfType = aspectType.GetCustomAttribute<AspectAttribute>().TargetType;

                var interceptorConstructor = typeof(AspectInterceptor<>).MakeGenericType(aspectOfType).GetConstructors()[0];

                var aspectInstance = aspectType.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());

                var interceptorInstance = interceptorConstructor.Invoke(new object[] { aspectInstance });

                interceptorInstance.GetType().GetProperty("AspectImplementation").GetSetMethod().Invoke(interceptorInstance, new object[] { aspectInstance });
                
                
                if (!TypeAspects.TryGetValue(aspectOfType, out var aspectsList))
                {
                    aspectsList = new List<IInterceptor>();
                    TypeAspects.Add(aspectOfType, aspectsList);
                }

                aspectsList.Add(interceptorInstance as IInterceptor);
            }
        }

        public static TTarget AddAspects<TTarget, TAspect>(TTarget target, params TAspect[] aspects) where TAspect : class, TTarget
        {
            var aspectInterceptors = aspects.Select(aspect =>
            {
                var interceptor = Activator.CreateInstance(typeof(AspectInterceptor<TAspect>), aspect);
                
                return interceptor as IInterceptor;
            }).ToList();

            if (ProxyUtil.IsProxy(target))
            {
                target = (TTarget)(target as IProxyTargetAccessor).DynProxyGetTarget();
            }

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
            if (ProxyUtil.IsProxy(target))
            {
                target = (TTarget)(target as IProxyTargetAccessor).DynProxyGetTarget();
            }

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
    }
}
