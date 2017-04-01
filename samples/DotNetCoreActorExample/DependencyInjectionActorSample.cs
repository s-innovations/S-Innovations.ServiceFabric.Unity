using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;

namespace DependencyInjectionActorSample
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    public class DependencyInjectionActorSample : Actor, IDependencyInjectionActorSample
    {
        /// <summary>
        /// Initializes a new instance of DependencyInjectionActorSample
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public DependencyInjectionActorSample(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        
        protected override Task OnActivateAsync() 
            => this.StateManager.TryAddStateAsync("count", 0);        

        Task<int> IDependencyInjectionActorSample.GetCountAsync() 
            => this.StateManager.GetStateAsync<int>("count");
        
        Task IDependencyInjectionActorSample.SetCountAsync(int count)
            => this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value);
        
    }



    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    public class DependencyInjectionActorSample1 : Actor, IDependencyInjectionActorSample1
    {
        /// <summary>
        /// Initializes a new instance of DependencyInjectionActorSample
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public DependencyInjectionActorSample1(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }


        protected override Task OnActivateAsync()
            => this.StateManager.TryAddStateAsync("count", 0);

        Task<int> IDependencyInjectionActorSample1.GetCountAsync()
            => this.StateManager.GetStateAsync<int>("count");

        Task IDependencyInjectionActorSample1.SetCountAsync(int count)
            => this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value);

    }
}
