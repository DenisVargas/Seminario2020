using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New Sound Library", menuName = "SoundManager/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    public List<AudioClip> Clips = new List<AudioClip>();
    [Range(0f,1f)]
    public float Volume = 1f;
    [SerializeField, Range(-3,3)]
    public float Pitch = 1f;
    public AudioMixerGroup mixerGroup = null;
    public bool Loop = false;

    //TODO: Laburos adiconales que pueden ayudar:
    //Slider de pesos para cada clip --> porcentaje de probabilidad.
    //Slider de rangos para aleatoriedad. (Para pitch y Volume)

    public AudioClip GetRandomClip()
    {
        //Selecciono aleatoriamente una source dentro de mi lista de sources.
        if(Clips.Count > 0)
        {
            return Clips[UnityEngine.Random.Range(0, Clips.Count)];
        }

        return null;
    }
}