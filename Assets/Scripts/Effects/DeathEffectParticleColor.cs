using UnityEngine;

public class DeathEffectParticleColor : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] _particles;

    private void Awake()
    {
        if (_particles == null || _particles.Length == 0)
            _particles = GetComponentsInChildren<ParticleSystem>(true);
    }
    
    public void SetColors(Color min, Color max)
    {
        foreach (var ps in _particles)
        {
            var main = ps.main;

            main.startColor = new ParticleSystem.MinMaxGradient(
                min,
                max);
        }
    }
}