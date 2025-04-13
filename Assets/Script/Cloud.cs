using System;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    ParticleSystem cloudSystem;
    public Color color;
    public Color lining;
    public int numberOfParticles;
    public float minSpeed;
    public float maxSpeed;
    public float distance;

    Vector3 startPosition;
    float speed;
    bool painted;

    private void Start()
    {
        cloudSystem = GetComponent<ParticleSystem>();
        Spawn();
    }

    private void Spawn()
    {
        float xPos = UnityEngine.Random.Range(-0.5f, 0.5f);
        float yPos = UnityEngine.Random.Range(-0.5f, 0.5f);
        float zPos = UnityEngine.Random.Range(-0.5f, 0.5f);

        transform.localPosition = new Vector3(xPos, yPos, zPos);
        speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        startPosition = transform.position;
    }

    private void Paint() 
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[cloudSystem.particleCount];
        cloudSystem.GetParticles(particles);

        if(particles.Length > 0) 
        {
            for(int i = 0; i < particles.Length; i++) 
            {
                particles[i].startColor = Color.Lerp(lining, color, particles[i].position.y / cloudSystem.shape.scale.y);
            }
            painted = true;
            cloudSystem.SetParticles(particles, particles.Length);
        }
    }

    void Update()
    {
        if(painted is false) 
        {
            Paint();
        }

        transform.Translate(0, 0, speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, startPosition) > distance) 
        {
            Spawn();
        }
    }
}
