using Castle.DynamicProxy;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Minx.Aspects
{
    internal class AspectInterceptor<T> : IInterceptor
    {
        private readonly Action<IInvocation> setInvocationDelegate = null;

        public object AspectImplementation { get; set; }

        public AspectInterceptor(T aspectImplementation)
        {
            AspectImplementation = aspectImplementation;

            var invocationProperties = AspectImplementation.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property => property.GetCustomAttribute<AspectInvocationAttribute>() != null);

            if (invocationProperties.Count() > 1)
            {
                throw new InvalidOperationException("Multiple attributes 'AspectInvocation' cannot be used in the same class.");
            }

            if (invocationProperties.Any())
            {
                var setInvocationMethodInfo = invocationProperties
                .First()
                .GetSetMethod(true);

                setInvocationDelegate = (Action<IInvocation>)
                    Delegate.CreateDelegate(typeof(Action<IInvocation>), AspectImplementation, setInvocationMethodInfo);
            }
        }

        [DebuggerHidden]
        public void Intercept(IInvocation invocation)
        {
            var proxy = new InvocationProxy(invocation);

            setInvocationDelegate?.Invoke(proxy);
            
            invocation.Method.Invoke(AspectImplementation, invocation.Arguments);

            if (!proxy.ProceedExecuted)
            {
                invocation.Proceed();
            }
        }
    }
}
