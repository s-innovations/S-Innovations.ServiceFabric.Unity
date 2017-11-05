using Unity.Builder;
using Unity.Extension;

namespace SInnovations.Unity.AspNetCore
{
    public sealed class CustomBuildExtension : UnityContainerExtension
    {
        //  public Func<Type, Type, String, MethodBase, IUnityContainer, Object> Constructor { get; set; }

        protected override void Initialize()
        {
            var strategy = new CustomBuilderStrategy(this);
            this.Context.Strategies.Add(strategy, UnityBuildStage.PreCreation);
        }
    }
}
