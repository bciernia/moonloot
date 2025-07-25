using UnityEngine;

[CreateAssetMenu]
public class Dash : Skill
{
    public float dashVelocity;

    public override void Activate()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var movement = player.GetComponent<PlayerMovement>();
        if (movement == null) return;

        movement.ApplyDash(dashVelocity, ActiveTime);
    }
}
