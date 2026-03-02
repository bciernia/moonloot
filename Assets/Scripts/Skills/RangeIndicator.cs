using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    [SerializeField] private ParticleSystem particle;

    public void SetRangeIndicatorRadius(float radius)
    {
        var shape = particle.shape;
        shape.radius = radius;        
        shape.donutRadius = 0.1f;
    }

    public void SetRangeIndicatorColor(Color minColor, Color maxColor)
    {
        var main = particle.main;
        
        minColor.a = 1f;
        maxColor.a = 1f;
        
        main.startColor = new ParticleSystem.MinMaxGradient(minColor, maxColor);
    }
}