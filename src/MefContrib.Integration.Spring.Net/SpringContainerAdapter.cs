#region Header

#endregion

namespace MefContrib.Integration.Autofac
{
    using System;
    using System.Linq;
    using Containers;
    using global::Spring.Core;
    using global::Spring.Context;
    using global::Spring.Context.Events;

    /// <summary>
    ///   Represents an adapter for the <see cref = "IApplicationContext" /> container.
    /// </summary>
    public class SpringContainerAdapter : ContainerAdapterBase
    {
        #region Fields

        private readonly IApplicationContext _container;

        #endregion

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SpringContainerAdapter" /> class.
        /// </summary>
        /// <param name = "container"><see cref = "IApplicationContext" /> instance.</param>
        public SpringContainerAdapter(IApplicationContext container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            _container = container;
            OnRegisteringComponent(typeof(IApplicationContext), null);
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Method called by <see cref = "ContainerExportProvider" /> in order
        ///   to initialize the adapter.
        /// </summary>
        public override void Initialize()
        {
            foreach(var definition in _container.GetObjectDefinitionNames())
            {
                var definitionType = _container.GetType(definition);
                OnRegisteringComponent(definitionType, definition);
            }

            _container.ContextEvent += (t, e) =>
                {
                    var contextEvent = e as ContextEventArgs;
                    if (contextEvent != null && contextEvent.Event == ContextEventArgs.ContextEvent.Refreshed)
                    {
                        foreach (var definition in _container.GetObjectDefinitionNames())
                        {
                            var definitionType = _container.GetType(definition);
                            OnRegisteringComponent(definitionType, definition);
                        }
                    }
                };
        }
        
        /// <summary>
        ///   Method called by <see cref = "ContainerExportProvider" /> to retrieve
        ///   an instance of a given type.
        /// </summary>
        /// <param name = "type">Type of the instance to retrieve.</param>
        /// <param name = "name">Optional name.</param>
        /// <returns>An instance of a given type.</returns>
        public override object Resolve(Type type, string name)
        {
            if (name == null)
            {
                var definitions = _container.GetObjectsOfType(type);
                if (definitions.Values.Count == 0)
                {
                    throw new InvalidOperationException(string.Format("No definition found for type {0}", type));
                }
                else if (definitions.Values.Count > 0)
                {
                    throw new InvalidOperationException(string.Format("Multiple definitions found for type {0}", type));
                }
                else
                {
                    var enumerator = definitions.Values.GetEnumerator();
                    enumerator.MoveNext();
                    return enumerator.Current;
                }
            }
            else
            {
                return _container.GetObject(name, type);
            }
        }

        #endregion
    }
}