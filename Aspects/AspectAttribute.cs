using System;

namespace Minx.Aspects
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AspectAttribute : Attribute
    {
        public AspectAttribute(Type targetType)
        {
            TargetType = targetType;
        }

        public Type TargetType { get; }
    }
}
