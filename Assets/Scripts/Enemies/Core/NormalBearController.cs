using Enemies.Types;

namespace Enemies.Core
{
    public class NormalBearController : BearController
    {
        public override BearType Type => BearType.Normal;
        
        protected override float CalculateDamage(float damage, DamageType damageType)
        {
            return damage; // Normal bears don't have elemental resistances
        }
    }
} 