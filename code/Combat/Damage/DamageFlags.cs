using System;

namespace Ironsim;

[Flags]
public enum DamageFlags
{
    None = 0,
    IgnoreResistance = 1,
    DamageOverTime = 2,
    NoFlinch = 4,
    Kick = 8,

    /// <summary>
    /// This damage contains an input vector, used for executions.
    /// </summary>
    HasVector = 16,
}
