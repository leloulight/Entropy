using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Mvc.ActionConstraints;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ModuleFramework
{
    public class ModuleActionDescriptorProvider : IActionDescriptorProvider
    {
        private readonly IAssemblyProvider _assemblyProvider;
        private readonly IServiceProvider _services;

        public ModuleActionDescriptorProvider(
            IAssemblyProvider assemblyProvider,
            IServiceProvider services)
        {
            _assemblyProvider = assemblyProvider;
            _services = services;
        }

        public int Order { get { return 0; } }

        public void OnProvidersExecuting(ActionDescriptorProviderContext context)
        {
            foreach (var assembly in _assemblyProvider.CandidateAssemblies)
            {
                foreach (var type in assembly.GetExportedTypes())
                {
                    var typeInfo = type.GetTypeInfo();
                    if (typeInfo.IsClass &&
                        !typeInfo.IsAbstract &&
                        !typeInfo.ContainsGenericParameters &&
                        typeof(MvcModule).IsAssignableFrom(type) &&
                        type != typeof(MvcModule))
                    {
                        foreach (var action in GetActions(type))
                        {
                            context.Results.Add(action);
                        }
                    }
                }
            }
        }

        public void OnProvidersExecuted(ActionDescriptorProviderContext context)
        {
        }

        private IEnumerable<ActionDescriptor> GetActions(Type type)
        {
            var prototype = (MvcModule)ActivatorUtilities.CreateInstance(_services, type);

            int i = 0;
            foreach (var action in prototype.Actions)
            {
                var filters = type.GetTypeInfo()
                    .GetCustomAttributes(inherit: true)
                    .OfType<IFilterMetadata>()
                    .Select(filter => new FilterDescriptor(filter, FilterScope.Controller))
                    .OrderBy(d => d, FilterDescriptorOrderComparer.Comparer)
                    .ToList();

                RouteDataActionConstraint pathConstraint;
                if (action.Path == "/")
                {
                    pathConstraint = new RouteDataActionConstraint("modulepath", string.Empty);
                }
                else
                {
                    pathConstraint = new RouteDataActionConstraint("modulepath", action.Path.Substring(1));
                }

                yield return new ModuleActionDescriptor()
                {
                    FilterDescriptors = filters,
                    Index = i++,
                    ActionConstraints = new List<IActionConstraintMetadata>()
                    {
                        new HttpMethodConstraint(new [] { action.Verb }),
                    },
                    ModuleType = type,
                    Parameters = new List<ParameterDescriptor>(), // No Parameter support in this sample
                    RouteConstraints = new List<RouteDataActionConstraint>()
                    {
                        new RouteDataActionConstraint("module", "true"), // only match a 'module' route
                        pathConstraint,
                    }
                };
            }
        }
    }
}