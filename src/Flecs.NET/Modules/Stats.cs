using System;
using static Flecs.NET.Bindings.flecs;

namespace Flecs.NET.Core;

public static unsafe partial class Ecs
{
    /// <summary>
    ///     Stats module.
    /// </summary>
    public struct Stats : IFlecsModule, IEquatable<Stats>
    {
        /// <summary>
        ///     Initializes stats module.
        /// </summary>
        /// <param name="world"></param>
        public readonly void InitModule(World world)
        {
            world.Import<Units>();
            ResetStatsIds();
            FlecsStatsImport(world);
        }

        private static void ResetStatsIds()
        {
            // Flecs addon component ids are process-global C externs. Reset them so
            // importing Stats into a new world does not reuse ids from a prior world.
            FLECS_IDFlecsStatsID_ = 0;
            FLECS_IDEcsWorldStatsID_ = 0;
            FLECS_IDEcsWorldSummaryID_ = 0;
            FLECS_IDEcsSystemStatsID_ = 0;
            FLECS_IDEcsPipelineStatsID_ = 0;
            FLECS_IDecs_entities_memory_tID_ = 0;
            FLECS_IDecs_component_index_memory_tID_ = 0;
            FLECS_IDecs_query_memory_tID_ = 0;
            FLECS_IDecs_component_memory_tID_ = 0;
            FLECS_IDecs_table_memory_tID_ = 0;
            FLECS_IDecs_misc_memory_tID_ = 0;
            FLECS_IDecs_table_histogram_tID_ = 0;
            FLECS_IDecs_allocator_memory_tID_ = 0;
            FLECS_IDEcsWorldMemoryID_ = 0;
        }

        /// <summary>
        ///     Checks if two <see cref="Stats"/> instances are equal.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Stats other)
        {
            return true;
        }

        /// <summary>
        ///     Checks if two <see cref="Stats"/> instances are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is Stats;
        }

        /// <summary>
        ///     Returns the hash code of the <see cref="Stats"/>.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 0;
        }

        /// <summary>
        ///     Checks if two <see cref="Stats"/> instances are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Stats left, Stats right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks if two <see cref="Stats"/> instances are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Stats left, Stats right)
        {
            return !(left == right);
        }
    }
}
