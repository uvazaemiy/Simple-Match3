using System;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] allParticles;
    [SerializeField] private float lifeTime = 1;

    
    
    
    
    private void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
        
        Destroy(gameObject, lifeTime);
    }





    public void Play()
    {
        foreach (ParticleSystem ps in allParticles)
        {
            ps.Stop();
            ps.Play();
        }
    }
}