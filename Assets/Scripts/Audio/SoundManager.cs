using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Utility.ObjectPools.Generic;

/*
    La función del SoundManager es de generar un pool dinámico de Sonidos.
    Esto nos permite añadir funcionalidad extra a los sonidos que usemos.
*/

public class SoundManager : MonoBehaviour
{
    public AudioMixerGroup defaultAudioMixerGroup;

    private static SoundManager _instance;
    public static SoundManager Main
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SoundManager>();
                if (_instance == null)
                {
                    _instance = new GameObject("SoundManager").AddComponent<SoundManager>();
                    DontDestroyOnLoad(_instance);
                }
            }

            return _instance;
        }
    }

    //Evita que haya multiples sonidos y se sumen.
    public Dictionary<SoundLibrary, List<AudioSource>> activeSources = new Dictionary<SoundLibrary, List<AudioSource>>();

    //Pool de AudioSources.
    GenericPool<AudioSource> _aviableSources = new GenericPool<AudioSource>();

    /// <summary>
    /// Reproduce el sonido encapsulado en una librería de sonidos utilizando sus settings.
    /// </summary>
    /// <param name="soundLib">Librería de Sonidos con sus respectivos settings.</param>
    /// <param name="allowMultiple"> ¿Está permitido la reproducción de multiples instancias en simultaneo? </param>
    public static void PlaySound(SoundLibrary soundLib, bool allowMultiple = true)
    {
        var m = SoundManager.Main;
        if (!m.activeSources.ContainsKey(soundLib))
            m.activeSources.Add(soundLib, new List<AudioSource>());

        if(m.activeSources[soundLib].Count == 0 || allowMultiple)
        {
            var source = m.GetNewSource(soundLib);
            m.activeSources[soundLib].Add(source);
            source.Play();
            source.gameObject.name = $"Playing sound: {soundLib.name}";
        }
    }
    public static void StopSound(SoundLibrary soundLib)
    {
        var m = SoundManager.Main;
        if (m.activeSources.ContainsKey(soundLib))
        {
            foreach (var source in m.activeSources[soundLib])
            {
                source.Stop();
                m._aviableSources.DisablePoolObject(source); //Retorno al Pool
            }

            //ISSUE: Esto bloquea la reproducción de todas las sources reproduciéndose en paralelo.
            m.activeSources[soundLib].Clear();
        }
    }
    public static void StopAllSounds()
    {
        var m = SoundManager.Main;
        foreach (var kvp in m.activeSources)
        {
            foreach (AudioSource source in kvp.Value)
            {
                source.Stop();
                m._aviableSources.DisablePoolObject(source);
            }
        }
        m.activeSources.Clear();
    }

    AudioSource SourceFactory()
    {
        var source = new GameObject("[Aviable AudioSource]").AddComponent<AudioSource>();
        source.transform.SetParent(transform);
        return source;
    }
    AudioSource GetNewSource(SoundLibrary soundLib)
    {
        AudioSource source = _aviableSources.GetObjectFromPool();
        AssignSoundToSource(soundLib, source);
        return source;
    }
    void AssignSoundToSource(SoundLibrary soundLib, AudioSource source)
    {
        source.clip = soundLib.GetRandomClip();
        source.volume = soundLib.Volume;
        source.pitch = soundLib.Pitch;
        source.loop = soundLib.Loop;
        source.outputAudioMixerGroup = soundLib.mixerGroup ? soundLib.mixerGroup : defaultAudioMixerGroup;
    }

    //================================ MonobeHaviour Funcs ===========================================

    public void Awake()
    {
        activeSources = new Dictionary<SoundLibrary, List<AudioSource>>();
        _aviableSources = new GenericPool<AudioSource>
            (
              5,
              SourceFactory,
              (source) => {/*Inicializador: No modifico el objeto en sí.*/},
              (source) =>
              {
                  source.gameObject.name = "[Aviable AudioSource]";
              },
              true
            );
    }
    public void Update()
    {
        var m = SoundManager.Main;
        foreach (var kvp in m.activeSources)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                var source = kvp.Value[i];
                if (!source.isPlaying)
                {
                    kvp.Value.RemoveAt(i);
                    _aviableSources.DisablePoolObject(source);
                }
            }
        }
    }
}