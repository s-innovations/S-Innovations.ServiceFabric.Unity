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

In Unity, you will register dependencies for scoped lifetime like this:
```
 container.RegisterType<From,To>(new HierarchicalLifetimeManager());
```

## TODO

- [ ] Remove dependency on unity
- [ ] Use CoreCLR dependency injection instead
- [ ] When dotnet core 1.1.0 is out, its possible to make services go from main.cs to startup.cs.