using Enemies.Types;

namespace Enemies.Core
{
    public class IceBearController : ElementalBearController
    {
        public override BearType Type => BearType.Ice;

        protected override bool IsWeakAgainst(DamageType damageType)
            => damageType == DamageType.Fire;

        protected override bool IsResistantTo(DamageType damageType)
            => damageType == DamageType.Ice;
    }
} 