using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.Unity.AspNetCore
{
    //http://stackoverflow.com/questions/39173345/unity-with-asp-net-core-and-mvc6-core
    public sealed class CustomBuilderStrategy : BuilderStrategy
    {
        private readonly CustomBuildExtension extension;

        public CustomBuilderStrategy(CustomBuildExtension extension)
        {
            this.extension = extension;
        }

        private IUnityContainer GetUnityFromBuildContext(IBuilderContext context)
        {
            var lifetime = context.Policies.Get<ILifetimePolicy>(NamedTypeBuildKey.Make<IUnityContainer>());
            return lifetime.GetValue() as IUnityContainer;
        }



        private class DerivedTypeConstructorSelectorPolicy : IConstructorSelectorPolicy
        {

            private readonly IConstructorSelectorPolicy _originalConstructorSelectorPolicy;
            private readonly IUnityContainer _container;

            public DerivedTypeConstructorSelectorPolicy(IUnityContainer container, IConstructorSelectorPolicy originalSelectorPolicy)
            {
                this._originalConstructorSelectorPolicy = originalSelectorPolicy;
                this._container = container;
            }

            public SelectedConstructor SelectConstructor(IBuilderContext context, IPolicyList resolverPolicyDestination)
            {
                try
                {
                    var name = context.BuildKey.Type.Name;
                    var originalConstructor = _originalConstructorSelectorPolicy.SelectConstructor(context, resolverPolicyDestination);


                    if (originalConstructor.Constructor.GetParameters().All(arg => _container.CanResolve(arg.ParameterType)))
                    {
                        return originalConstructor;
                    }
                    else
                    {
                        var newSelectedConstructor = FindNewCtor(originalConstructor);
                        var tryNulls = newSelectedConstructor == null;
                        if (tryNulls)
                            newSelectedConstructor = new SelectedConstructor(originalConstructor.Constructor);

                        foreach (var parameter in newSelectedConstructor.Constructor.GetParameters())
                        {
                            if (tryNulls && !_container.CanResolve(parameter.ParameterType) && parameter.HasDefaultValue)
                            {
                                
                                newSelectedConstructor.AddParameterResolver(new LiteralValueDependencyResolverPolicy(null));
                            }
                            else
                            {
                                newSelectedConstructor.AddParameterResolver(parameterResolver.GetResolver(parameter));
                            }
                        }
                        //    foreach (var newParameterResolver in
                        //    originalConstructor.GetParameterResolvers().Take(newSelectedConstructor.Constructor.GetParameters().Length))
                        //{
                        //    newSelectedConstructor.AddParameterResolver(newParameterResolver);
                        //}

                        return newSelectedConstructor;
                    }
                }
                catch (Exception ex)
                {
                    var a = context.BuildKey.Type;

                    var b = GetConstructorForType(a);
                    foreach (var parameter in b.Constructor.GetParameters())
                    {
                        b.AddParameterResolver(parameterResolver.GetResolver(parameter));
                    }

                    return b;
                    throw;
                }
            }

            /// <summary>
            /// Exposes part of Unity's default policy which is marked protected.
            /// </summary>
            class ParameterResolver : DefaultUnityConstructorSelectorPolicy
            {
                public IDependencyResolverPolicy GetResolver(ParameterInfo parameterInfo)
                {
                  
                    return CreateResolver(parameterInfo);
                }
            }

            readonly ParameterResolver parameterResolver = new ParameterResolver();

            private SelectedConstructor FindNewCtor(SelectedConstructor originalConstructor)
            {
                var implementingType = originalConstructor.Constructor.DeclaringType;
                return GetConstructorForType(implementingType, originalConstructor);
            }

            private SelectedConstructor GetConstructorForType(Type implementingType, SelectedConstructor originalConstructor = null)
            {
                var constructors = implementingType.GetTypeInfo()
                  .DeclaredConstructors
                  .Where(constructor => constructor.IsPublic && constructor != originalConstructor?.Constructor)
                  .ToArray();
                if (constructors.Length == 0) return null;

                if (constructors.Length == 1)
                    return new SelectedConstructor(constructors[0]);

                Array.Sort(constructors,
                   (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));

                var newCtor = constructors.FirstOrDefault(c => c.GetParameters()
                    .All(arg => _container.CanResolve(arg.ParameterType)));

                if (newCtor == null)
                    return null;

                return new SelectedConstructor(newCtor);
            }
        }
        public override void PreBuildUp(IBuilderContext context)
        {
            if (context.Existing != null)
            {
                return;
            }



            var originalSelectorPolicy = context.Policies.Get<IConstructorSelectorPolicy>(context.BuildKey,
                out IPolicyList selectorPolicyDestination);

            selectorPolicyDestination.Set<IConstructorSelectorPolicy>(
                new DerivedTypeConstructorSelectorPolicy(
                    GetUnityFromBuildContext(context), originalSelectorPolicy),
                context.BuildKey);

        }

    }
}
