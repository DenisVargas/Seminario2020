using UnityEngine;

[System.Serializable]
public struct Damage
{
    public DamageType type;
    public float Ammount;
    public float criticalMultiplier;
    public bool instaKill;
    public int KillAnimationType;

    public Vector3 explotionOrigin;
    public float explotionForce;

    public static Damage defaultDamage()
    {
        return new Damage() { Ammount = 1, criticalMultiplier = 1, instaKill = false, KillAnimationType = 0, type = DamageType.hit, explotionForce = 0f, explotionOrigin = Vector3.zero };
    }
}
