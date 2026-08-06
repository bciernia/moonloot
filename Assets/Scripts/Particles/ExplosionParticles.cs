using UnityEngine;

public class ExplosionParticles : MonoBehaviour
{
    public ParticleSystem ps;

    public void SetRadius(float radius)
    {
        var main = ps.main;

        // speed zostaje np. 6
        float speed = main.startSpeed.constant;

        main.startLifetime = radius / speed;
    }
}