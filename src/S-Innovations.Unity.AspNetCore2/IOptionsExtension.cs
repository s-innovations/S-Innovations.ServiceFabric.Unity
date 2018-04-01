using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Builder;
using Unity.Extension;
using Unity.Lifetime;
using Unity.ObjectBuilder.BuildPlan.DynamicMethod;
using Unity.Policy;

namespace SInnovations.Unity.AspNetCore
{
    public class IOptionsExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {

            Context.Policies.Set(typeof(OptionsFactory<>), string.Empty, typeof(IBuildPlanCreatorPolicy), new OptionsFactoryBuilderPlanCreatorPolicy(Context.Policies));



        }
        public ILifetimeContainer Lifetime => Context.Lifetime;

        public class OptionsFactoryBuilderPlanCreatorPolicy : IBuildPlanCreatorPolicy
        {
            private readonly IPolicyList _policies;

            private readonly MethodInfo _factoryMethod =
                typeof(OptionsFactoryBuilderPlanCreatorPolicy).GetTypeInfo().GetDeclaredMethod(nameof(FactoryMethod));

            /// <summary>
            /// Factory plan to build [I]Foo type
            /// </summary>
            /// <param name="policies">Container policy list to store created plans</param>
            public OptionsFactoryBuilderPlanCreatorPolicy(IPolicyList policies)
            {
                _policies = policies;
            }

            public IBuildPlanPolicy CreatePlan(IBuilderContext context, INamedType buildKey)
            {
                // Make generic factory method for the type
                var typeToBuild = buildKey.Type.GetTypeInfo().GenericTypeArguments;
                var factoryMethod =
                    _factoryMethod.MakeGenericMethod(typeToBuild)
                                  .CreateDelegate(typeof(DynamicBuildPlanMethod));
                // Create policy
                var creatorPlan = new DynamicMethodBuildPlan((DynamicBuildPlanMethod)factoryMethod);

                // Register BuildPlan policy with the container to optimize performance
                _policies.Set(buildKey.Type, string.Empty, typeof(IBuildPlanPolicy), creatorPlan);

                //var t1 = typeof(IConfigureOptions<>).MakeGenericType(typeToBuild);
                //CreateChildPlan(t1);
                //var t2 = typeof(IPostConfigureOptions<>).MakeGenericType(typeToBuild);
                //CreateChildPlan(t2);

                return creatorPlan;
            }

            private void CreateChildPlan(Type t1)
            {
                var et1 = typeof(IEnumerable<>).MakeGenericType(t1);
                var p1 = new DynamicMethodBuildPlan((DynamicBuildPlanMethod)
                           _resolveMethod.MakeGenericMethod(t1)
                                         .CreateDelegate(typeof(DynamicBuildPlanMethod)));

                _policies.Set(et1, string.Empty, typeof(IBuildPlanPolicy), p1);
            }

            private static void FactoryMethod<TResult>(IBuilderContext context) where TResult : class, new()
            {


                var p1 = new DynamicMethodBuildPlan((DynamicBuildPlanMethod)
                             _resolveMethod.MakeGenericMethod(typeof(IConfigureOptions<TResult>))
                                           .CreateDelegate(typeof(DynamicBuildPlanMethod)));

                var a1 = p1.ExecuteBuildUp(context);

                var p2 = new DynamicMethodBuildPlan((DynamicBuildPlanMethod)
                           _resolveMethod.MakeGenericMethod(typeof(IPostConfigureOptions<TResult>))
                                         .CreateDelegate(typeof(DynamicBuildPlanMethod)));

                var a2 = p2.ExecuteBuildUp(context);



                context.Existing = new OptionsFactory<TResult>(a1 as IEnumerable<IConfigureOptions<TResult>>, a2 as IEnumerable<IPostConfigureOptions<TResult>>);
            }

            

            private static MethodInfo _resolveMethod = typeof(UnityContainer).GetTypeInfo().GetDeclaredMethod("ResolveEnumerable");
        }
    }
}
