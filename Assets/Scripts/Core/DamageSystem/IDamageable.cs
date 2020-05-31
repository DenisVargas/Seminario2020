using UnityEngine;

namespace Core.DamageSystem
{
    public interface IDamageable<Input, output>
    {
        bool IsAlive { get;}

        GameObject gameObject { get; }
        output GetHit(Input damage);
        void FeedDamageResult(output result);
        Input GetDamageStats();
    }
}
