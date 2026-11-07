using System;

namespace Rebalance.MiscUtils;

//TODO: Apply this attribute to other places where DamageEnemies damage multipliers have to be adjusted
[AttributeUsage(AttributeTargets.Method)]
public class DoDamageEnemiesHookAttribute : Attribute;