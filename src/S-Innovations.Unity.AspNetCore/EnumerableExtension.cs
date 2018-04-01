using Unity.Builder;
using Unity.Extension;

namespace SInnovations.Unity.AspNetCore
{



    //public class EnumerableExtension<T> : UnityContainerExtension
    //{
    //    protected override void Initialize()
    //    {
    //            //Lets use a stategy instead
    //        Context.Strategies.Add(new EnumerableExtensionBuilderStategy<T>(),
    //         UnityBuildStage.Setup);

    //    }
    //}
    ////
    //public class EnumerableExtensionBuilderStategy<T> : BuilderStrategy 
    //{
    //    delegate void ResolveGenericEnumerable(IBuilderContext context, Type type);
    //    private MethodInfo _resolveGenericMethod = typeof(UnityContainer).GetTypeInfo().GetDeclaredMethod("ResolveGenericEnumerable");
    //    private MethodInfo _resolveMethod = typeof(UnityContainer).GetTypeInfo().GetDeclaredMethod("ResolveEnumerable");




    //    public override void PreBuildUp(IBuilderContext context)
    //    {
    //        //  Guard.ArgumentNotNull(context, "context");


    //        var plan = context.Registration.Get<IBuildPlanPolicy>();
    //        if (plan == null)
    //        {
    //            var typeArgument = context.BuildKey.Type.GetTypeInfo().GenericTypeArguments.First();



    //            plan = new DynamicMethodBuildPlan((DynamicBuildPlanMethod)
    //                     _resolveMethod.MakeGenericMethod(typeArgument)
    //                                   .CreateDelegate(typeof(DynamicBuildPlanMethod)));

    //            context.Registration.Set(typeof(IBuildPlanPolicy), plan);
    //        }

    //        plan.BuildUp(context);
    //        context.BuildComplete = true;

    //    }

    //    public override bool RequiredToBuildType(IUnityContainer container, INamedType namedType, params InjectionMember[] injectionMembers)
    //    {
    //        return namedType is InternalRegistration registration &&
    //               registration.Type.GetTypeInfo().IsGenericType &&
    //               typeof(IEnumerable<>) == registration.Type.GetGenericTypeDefinition()
    //               && registration.Type.GetGenericArguments()[0] == typeof(T);
    //    }



    //}

    public class EnumerableExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            // Enumerable strategy
            Context.Strategies.Add(new EnumerableResolutionStrategy(),
                UnityBuildStage.Setup);

        }
    }
}
