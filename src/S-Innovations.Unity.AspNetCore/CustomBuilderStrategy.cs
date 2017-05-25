using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
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
                var originalConstructor = _originalConstructorSelectorPolicy.SelectConstructor(context, resolverPolicyDestination);

                if (originalConstructor.Constructor.GetParameters().All(arg => _container.CanResolve(arg.ParameterType)))
                {
                    return originalConstructor;
                }
                else
                {
                    var newSelectedConstructor = FindNewCtor(originalConstructor);
                    if (newSelectedConstructor == null)
                        return originalConstructor;

                    foreach (var newParameterResolver in
                        originalConstructor.GetParameterResolvers().Take(newSelectedConstructor.Constructor.GetParameters().Length))
                    {
                        newSelectedConstructor.AddParameterResolver(newParameterResolver);
                    }

                    return newSelectedConstructor;
                }
            }

            private SelectedConstructor FindNewCtor(SelectedConstructor originalConstructor)
            {
                var implementingType = originalConstructor.Constructor.DeclaringType;
                var constructors = implementingType.GetTypeInfo()
                  .DeclaredConstructors
                  .Where(constructor => constructor.IsPublic && constructor != originalConstructor.Constructor)
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
