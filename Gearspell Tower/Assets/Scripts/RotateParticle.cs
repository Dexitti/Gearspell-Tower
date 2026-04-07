using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RotateParticle : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void Update()
    {
        int count = ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            Vector3 velocity = particles[i].velocity;
            
            if (velocity.magnitude > 0.01f)
            {
                particles[i].rotation = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            }
        }
        
        ps.SetParticles(particles, count);
    }
}
