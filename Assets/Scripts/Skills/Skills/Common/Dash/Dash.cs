using UnityEngine;

[CreateAssetMenu(fileName = "Dash_", menuName = "Skill/Dash")]
public class Dash : Skill
{
    [Header("Dash configuration")]
    public float dashVelocity;

    public override bool Activate(GameObject user)
    { 
        if (user == null) return false;
        
        var movement = user.GetComponent<PlayerMovement>();
        if (movement == null) return false;

        movement.ApplyDash(dashVelocity, ActiveTime);

        return true;
    }

    public override string GetDescription()
    {
        return $"Slip between heartbeats with a sudden burst of speed.\n\nDash in your current movement direction, gaining +{dashVelocity:0.#} movement speed for {ActiveTime:0.##} seconds. You must be moving to use this skill.";
    }
}
