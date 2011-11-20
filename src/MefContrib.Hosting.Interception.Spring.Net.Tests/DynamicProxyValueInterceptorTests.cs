namespace MefContrib.Hosting.Interception.Spring.Net.Tests
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using MefContrib.Hosting.Interception.Configuration;
    using MefContrib.Hosting.Interception.Spring.Net;
    using MefContrib.Tests;
    using NUnit.Framework;
    using AopAlliance.Intercept;
    using AopAlliance.Aop;
    using global::Spring.Aop;
    using global::Spring.Aop.Support;

    [TestFixture]
    public class DynamicProxyValueInterceptorTests
    {
        private CompositionContainer container;

        [SetUp]
        public void TestSetUp()
        {
            var innerCatalog = new TypeCatalog(typeof(Customer));
            var interceptor = new FreezableInterceptor();
            interceptor.Freeze();

            var valueInterceptor = new DynamicProxyInterceptor(new IAdvice[] { interceptor });
            var cfg = new InterceptionConfiguration()
                .AddInterceptor(valueInterceptor);

            var catalog = new InterceptingCatalog(innerCatalog, cfg);
            container = new CompositionContainer(catalog);
        }

        [Test]
        public void When_setting_name_on_the_customer_it_should_error()
        {
            var customer = container.GetExportedValue<ICustomer>();
            typeof(InvalidOperationException).ShouldBeThrownBy(() => { customer.Name = "John Doe"; });
        }
    }

    [Export(typeof(ICustomer))]
    public class Customer : ICustomer
    {
        public string Name { get; set; }
    }

    public interface ICustomer
    {
        string Name { get; set; }
    }

    // Freezable interceptor taken from Krzysztof Kozmic
    internal class FreezableInterceptor : IMethodInterceptor, IFreezable
    {
        private bool _isFrozen;

        public void Freeze()
        {
            _isFrozen = true;
        }

        public bool IsFrozen
        {
            get { return _isFrozen; }
        }


        public object Invoke(IMethodInvocation invocation)
        {
            var targetMethod = invocation.Method;
            if (_isFrozen && targetMethod.Name.StartsWith("set_", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException();
            }

            return invocation.Method.Invoke(invocation.Target, invocation.Arguments);
        }
    }

    internal interface IFreezable { bool IsFrozen { get; } void Freeze();}
}