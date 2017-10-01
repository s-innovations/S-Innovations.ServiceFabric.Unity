using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.ObjectBuilder;

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
