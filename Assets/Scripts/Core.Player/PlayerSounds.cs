using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    // Start is called before the first frame update
    public void PlaySteps()
    {
        AudioManger.instance.Play("playerSteps");
    }
    public void PlayDeathSound()
    {
        AudioManger.instance.Play("playerDeath");
    }
}
