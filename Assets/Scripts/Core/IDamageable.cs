using UnityEngine;

namespace SurvivalMoba.Core
{
    /// <summary>
    /// Anything that can receive damage (player, enemies, boss).
    /// Projectiles and abilities resolve damage through this interface so they
    /// never need to know the concrete type they hit.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>Faction of this target, used to filter friendly fire.</summary>
        Faction Faction { get; }

        /// <summary>True while the target is alive and can still take damage.</summary>
        bool IsAlive { get; }

        /// <summary>World position of the target (used for aiming and VFX).</summary>
        Vector3 Position { get; }

        /// <summary>Apply <paramref name="amount"/> damage to the target.</summary>
        void TakeDamage(float amount);

        /// <summary>Apply a temporary movement slow (0..1 fraction) for a duration in seconds.</summary>
        void ApplySlow(float slowFraction, float duration);
    }
}
