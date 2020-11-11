using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimePatch : MonoBehaviour
{
    [Header("Burning State Components")]
    [SerializeField] ParticleSystem _burnParticle = null;
    [Header("Options")]
    [SerializeField] bool _isBurning = false;
    public bool DestroyOnBurning = false;

    public bool isBurning
    {
        get => _isBurning;
        set
        {
            _isBurning = value;
            if (_isBurning)
            {
                _burnParticle.gameObject.SetActive(true);
                _burnParticle.Play();
            }
            else
            {
                _burnParticle.Stop();
                _burnParticle.gameObject.SetActive(false);
            }
        }
    }

    private void Awake()
    {
        isBurning = _isBurning;
    }
}
