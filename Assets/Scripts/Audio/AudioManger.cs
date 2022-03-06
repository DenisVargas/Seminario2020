using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManger : MonoBehaviour
{
    // Start is called before the first frame update


      public Sound[] AllSounds;

    public static AudioManger instance;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance)
            Destroy(gameObject);
        instance = this;
       

        foreach( Sound s in AllSounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

        }
        

    }
    private void Start()
    {
        Play("BackgroundMusic");
    }

    public void Play(String name)
    {

        Sound s = Array.Find(AllSounds, Sound => Sound.name == name);

        if (s != null)
            s.source.Play();

    }

    public void PlayAt(String name,float time)
    {

        Sound s = Array.Find(AllSounds, Sound => Sound.name == name);

        if (s != null)
            s.source.PlayScheduled(time);

    }





}
