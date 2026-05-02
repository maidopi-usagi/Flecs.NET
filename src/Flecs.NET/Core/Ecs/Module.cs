using static Flecs.NET.Bindings.flecs;

namespace Flecs.NET.Core;

public static unsafe partial class Ecs
{
    // Flecs C addons store module/component ids in process-global externs.
    // Serialize world initialization and imports so concurrent worlds do not reuse stale ids.
    internal static readonly object ImportLock = new();

    /// <summary>
    ///     Imports a module.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <typeparam name="T">The module type.</typeparam>
    /// <returns>The module entity.</returns>
    public static Entity Import<T>(World world) where T : IFlecsModule, new()
    {
        lock (ImportLock)
        {
            if (Type<T>.IsRegistered(world, out Entity module) && module.Has(EcsModule))
                return module;

            return new Entity(world, DoImport<T>(world));
        }
    }

    private static ulong DoImport<T>(World world) where T : IFlecsModule, new()
    {
        Entity module = world.Entity(Type<T>.Id(world)).Add(EcsModule);
        ulong prevScope = world.SetScope(module);

        T moduleInstance = new T();
        moduleInstance.InitModule(world);

        world.SetScope(prevScope);

        if (!Type<T>.IsTag)
        {
            world.Set(in moduleInstance);
        }

        return module;
    }
}
