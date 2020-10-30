using UnityEngine;
using Core.DamageSystem;

public class ExplosivePot : MonoBehaviour, IDamageable<Damage, HitResult>
{
    [SerializeField] ParticleSystem Explotion = null;
    [SerializeField] LayerMask explotionAffects = ~0;
    [SerializeField] float ExplotionForce = 10;
    [SerializeField] float ExplotionRadius = 4;

    public bool IsAlive { get; private set; } = true;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, ExplotionRadius);
    } 
#endif

    public void Explode()
    {
        //Exploto y destruyo cosas a mi alrededor.
        print("EXPLOTO A LA CHUCHA");
        var explotionParticle = Instantiate(Explotion, transform.position, Quaternion.identity);
        explotionParticle.gameObject.SetActive(true);
        explotionParticle.Play();

        //Creo un Damage con la explosión.
        var toApplyDamage = Damage.defaultDamage();
        toApplyDamage.Ammount = 100f;
        toApplyDamage.instaKill = true;
        toApplyDamage.KillAnimationType = 0;
        toApplyDamage.type = DamageType.explotion;
        toApplyDamage.explotionForce = ExplotionForce;
        toApplyDamage.explotionOrigin = transform.position;

        //Acá tengo tengo que buscar todos los objetivos en un radio y transmitirles daño de explosión.
        var hits = Physics.OverlapSphere(transform.position, ExplotionRadius, explotionAffects);
        if (hits.Length > 0)
        {
            foreach (var collider in hits)
            {
                //Chequeo si tiene un componente damageable.
                var damageable = collider.GetComponent<IDamageable<Damage, HitResult>>();
                if (damageable != null)
                    damageable.GetHit(toApplyDamage);
            }
        }

        IsAlive = false;
        Destroy(gameObject);
    }

    public void FeedDamageResult(HitResult result) { }
    public Damage GetDamageStats()
    {
        //Retorno mis datos de Daño.
        return Damage.defaultDamage();
    }
    public void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod) { /*acá no pasa nada xD*/ }
    public HitResult GetHit(Damage damage)
    {
        //Recivo daño de una fuente externa.
        var result = new HitResult(true);

        if (damage.type == DamageType.Fire)
        {
            result.exploded = true;
            Explode();
        }

        return result;
    }
}
