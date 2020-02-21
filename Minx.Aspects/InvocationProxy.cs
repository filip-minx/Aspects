using Castle.DynamicProxy;
using System;
using System.Reflection;

namespace Minx.Aspects
{
    class InvocationProxy : IInvocation
    {
        public bool ProceedExecuted { get; private set; }

        private IInvocation invocation;

        public InvocationProxy(IInvocation invocation)
        {
            this.invocation = invocation;
        }

        public object[] Arguments => invocation.Arguments;

        public Type[] GenericArguments => invocation.GenericArguments;

        public object InvocationTarget => invocation.InvocationTarget;

        public MethodInfo Method => invocation.Method;

        public MethodInfo MethodInvocationTarget => invocation.MethodInvocationTarget;

        public object Proxy => invocation.Proxy;

        public object ReturnValue { get => invocation.ReturnValue; set => invocation.ReturnValue = value; }

        public Type TargetType => invocation.TargetType;

        public IInvocationProceedInfo CaptureProceedInfo()
        {
            return invocation.CaptureProceedInfo();
        }

        public object GetArgumentValue(int index)
        {
            return invocation.GetArgumentValue(index);
        }

        public MethodInfo GetConcreteMethod()
        {
            return invocation.GetConcreteMethod();
        }

        public MethodInfo GetConcreteMethodInvocationTarget()
        {
            return invocation.GetConcreteMethodInvocationTarget();
        }

        public void Proceed()
        {
            ProceedExecuted = true;
            invocation.Proceed();
        }

        public void SetArgumentValue(int index, object value)
        {
            invocation.SetArgumentValue(index, value);
        }
    }
}
