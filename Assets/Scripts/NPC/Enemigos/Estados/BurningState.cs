using IA.FSM;
using System;

public class BurningState : State
{
    public Action OnBurning = delegate { };

    public override void Begin()
    {
        //Seteo la animación.
        //print($"{gameObject.name} está entrando a Burning");
        _anims.SetBool("Burning", true);

        OnBurning();
    }

    public override void Execute()
    {
#region TODO
        //    //Tengo un timing en el que estoy prendido fuego...
        //    //Por cada segundo que estoy prendido fuego, me aplico daño x acumulación.
        //    //List<int> indexesToDelete = new List<int>();

        //float fireDamage = 0;
        //for (int i = 0; i < DamageStack[DamageType.e_fire].Count; i++)
        //{
        //    var acum = DamageStack[DamageType.e_fire][i];
        //    fireDamage += acum.Ammount;

        //    float time = acum.TimeRemaining;
        //    time -= Time.deltaTime;
        //    if (time <= 0)
        //        indexesToDelete.Add(i);
        //}

        //foreach (var index in indexesToDelete)
        //{
        //    DamageStack[DamageType.e_fire].RemoveAt(index);
        //}

        //Health -= fireDamage;
#endregion

        //Esto estaría kul, pero por ahora solo me muero al terminar la animación.
        //Espero a que la animación termine.

        //Igual que en attack, reviso todo lo que sea relevante al ataque
        //Si no veo a un player y sigo vivo cuando termino de prenderme fuego.
        //Entro en rage.
    }

    public override void End()
    {
        //print($"{gameObject.name} está saliendo de Burning");
    }
}
