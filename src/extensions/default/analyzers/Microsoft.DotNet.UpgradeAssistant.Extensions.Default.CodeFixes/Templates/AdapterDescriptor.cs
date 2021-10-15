using System;
using System.Diagnostics;
#if NET || NETCOREAPP
using Microsoft.AspNetCore.Http;
#else
using System.Web;
#endif
using Microsoft.CodeAnalysis.Refactoring;

[assembly: AdapterDescriptor(typeof(/*{{DEPRECATED_TYPE}}*/))]

namespace Microsoft.CodeAnalysis.Refactoring
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal sealed class AdapterDescriptorAttribute : Attribute
    {
        public AdapterDescriptorAttribute(Type interfaceType, Type original)
        {
        }

        public AdapterDescriptorAttribute(Type original)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal sealed class AdapterFactoryDescriptorAttribute : Attribute
    {
        public AdapterFactoryDescriptorAttribute(Type factoryType, string factoryMethod)
        {
        }
    }
}
