using UnityEngine;

public static class DeathEffectColors
{
    public static (Color min, Color max) GetColors(DeathEffectType type)
    {
        return type switch
        {
            DeathEffectType.Explosion =>
                (new Color(1f, 0.35f, 0f),
                    new Color(1f, 0.8f, 0.2f)),

            DeathEffectType.IceNova =>
                (new Color(0.2f, 0.7f, 1f),
                    new Color(0.8f, 1f, 1f)),

            DeathEffectType.PoisonCloud =>
                (new Color(0.15f, 0.7f, 0.15f),
                    new Color(0.8f, 1f, 0.2f)),

            _ =>
                (Color.white, Color.white)
        };
    }
}