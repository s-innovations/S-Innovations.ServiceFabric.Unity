# Dependency Injection Sample for Servicefabric

The current sample uses Unity, but the goal is to move away from a specific container and use the CoreCLR dependency injection abstractions.

## What will happen in the sample?


1. A `MyTestActor(id="MyCoolActor")` will be created, activated and 10secs later it will ask `MySecondTestActor` to do some work.
2. The MyTestActor(id="MyCoolActor") will be garbage collected after 120 seconds, see the registration in program.cs
3. The MyTestActor(id="MyCoolActor") will be recreated, activated again each 3min and do the same as in 1.
4. The MySecondTestActor(id="MyCoolActor") will be garbage collected after 30sec and recreated every 1 min due to its own reminder or every 3min due to the dowork call in 1.
5. The MySecondTestActor(id="MyCoolActor") will every 1 min ask DependencyInjectionActorSample for its count, increase it by one and set it again.


## Dependencies

Every new actor or service created will have a scoped container, such all scoped dependency registrations will be disposed if implementing IDisposable when the actor is garbage collected.

### Scoped Lifetime

Extension methods to the unity container has been made such the same methods that are in dotnet core also exists on IUnityContainer - this way it should be straint forward to use.

## TODO

- [] Verify that the above example of what happens is still accurate after all the refactoring.
- [x] Remove dependency on unity. Note: Will not happen, as IOC of dotnet core do not support child containers. Therefore we must bring our own container. I been using Unity for all the time I remember, so I will be sticking with this. Consider abstractin it out so you also can bring your own. For now there is no arguments to change this.
- [x] Use CoreCLR dependency injection instead. Note: Its been integrated such IServiceProviderFactory is used. Meaning that aspnet core apps can use the container and registrations to their apps also.
- [x] When dotnet core 1.1.0 is out, its possible to make services go from main.cs to startup.cs. All servics registered in main can now be used in nested services, like dotnet core apps.