using Enemies.Types;

namespace Enemies.Core
{
    public class FireBearController : ElementalBearController
    {
        public override BearType Type => BearType.Fire;

        protected override bool IsWeakAgainst(DamageType damageType)
            => damageType == DamageType.Ice;

        protected override bool IsResistantTo(DamageType damageType)
            => damageType == DamageType.Fire;
    }
} 