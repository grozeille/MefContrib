namespace MefContrib.Hosting.Interception.Spring.Net
{
    using System.Linq;
    using global::Spring.Aop.Framework;
    using global::Spring.Aop;
    using AopAlliance.Aop;
    using System.Diagnostics;
    using System;

    /// <summary>
    /// Defines an interceptor which creates proxies using the Spring.Net library.
    /// </summary>
    public class DynamicProxyInterceptor : IExportedValueInterceptor
    {
        private readonly IAdvice[] advices;

        private readonly IAdvisor[] advisors;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProxyInterceptor"/> class.
        /// </summary>
        /// <param name="advisors"></param>
        public DynamicProxyInterceptor(IAdvice[] advices)
        {
            this.advices = advices;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProxyInterceptor"/> class.
        /// </summary>
        /// <param name="advisors"></param>
        public DynamicProxyInterceptor(IAdvisor[] advisors)
        {
            this.advisors = advisors;
        }

        /// <summary>
        /// Intercepts an exported value.
        /// </summary>
        /// <param name="value">The value to be intercepted.</param>
        /// <returns>Intercepted value.</returns>
        public object Intercept(object value)
        {
            var interfaces = value.GetType().GetInterfaces();

            ProxyFactory factory = new ProxyFactory(value);
           
            foreach (var intf in interfaces)
            {
                factory.AddInterface(intf);
            }

            if (this.advices != null)
            {
                foreach (var advice in this.advices)
                {
                    factory.AddAdvice(advice);
                }
            }
            else
            {
                foreach (var advisor in this.advisors)
                {
                    factory.AddAdvisor(advisor);
                }
            }

            return factory.GetProxy();
        }
    }
}
