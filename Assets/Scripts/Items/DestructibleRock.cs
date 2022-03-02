using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DestructibleRock : Destroyable
{

#if UNITY_EDITOR
    [SerializeField] bool debugThisRock = false; 
#endif

    public override HitResult GetHit(Damage damage)
    {
        HitResult result = new HitResult(true);

        if (damage.type == DamageType.explotion)
        {
            result.exploded = true;
            result.fatalDamage = true;
            Explode(damage.explotionOrigin, damage.explotionForce);
            return result;
        }

        if (damage.type == DamageType.blunt)
        {
#if UNITY_EDITOR
            if (debugThisRock)
                print($"{gameObject.name} has recieved a Blunt Damage"); 
#endif

            getSmashed();
            //fatalDamageCount++;
        }

        //if (damage.type == DamageType.Fire)
        //{
        //    result.ignited = true;
        //    getBurned();
        //}

        return result;
    }

    protected void Explode(Vector3 explotionOrigin, float force)
    {
        //print($"{gameObject.name}: Recibió daño por Explosión.");
        if (_destroyedObject)
        {
            _normalObject.SetActive(false);
            _destroyedObject.SetActive(true);

            var transforms = _destroyedObject.transform.GetComponentsInChildren<Transform>();
            foreach (var transform in transforms)
            {
                if (transform == this.transform) continue;
                //print($"{transform.name} es un elmento hijo");

                var rb = transform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    //Les aplico fuerza.
                        //Teniendo el epicentro y mi posición calculo la dirección en opesta a donde aplico la fuerza.
                }
            }

            foreach (var node in AffectedNodes)
                node.ChangeNodeState(IA.PathFinding.NavigationArea.Navegable);
        }

        StartCoroutine(delayedDestroy(_timeToDestroy));
    }
    bool getBurned()
    {
        print($"{gameObject.name} recibió un golpecito de fuego.");
        return false;
    }
    bool getSmashed()
    {
        print($"{gameObject.name} fue Aplastado.");

        if (_destroyedObject)
        {
            _normalObject.SetActive(false);
            _destroyedObject.SetActive(true);

            //var transforms = _destroyedObject.transform.GetComponentsInChildren<Transform>();
            //foreach (var transform in transforms)
            //{
            //    if (transform == this.transform) continue;
            //    //print($"{transform.name} es un elmento hijo");

            //    //var rb = transform.GetComponent<Rigidbody>();
            //    //if (rb != null)
            //    //{
            //    //    //Les aplico fuerza.
            //    //    //Teniendo el epicentro y mi posición calculo la dirección en opesta a donde aplico la fuerza.
            //    //}
            //}

            foreach (var node in AffectedNodes)
                node.ChangeNodeState(IA.PathFinding.NavigationArea.Navegable);
        }

        StartCoroutine(delayedDestroy(_timeToDestroy));

        return false;
    }
}
