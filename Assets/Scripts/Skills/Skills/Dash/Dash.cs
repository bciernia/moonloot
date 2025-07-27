using UnityEngine;

[CreateAssetMenu(fileName = "Dash_", menuName = "Skill/Dash")]
public class Dash : Skill
{
    [Header("Dash configuration")]
    public float dashVelocity;

    public override void Activate(GameObject user)
    {
        var movement = user.GetComponent<PlayerMovement>();
        if (movement == null) return;

        movement.ApplyDash(dashVelocity, ActiveTime);
    }
}
