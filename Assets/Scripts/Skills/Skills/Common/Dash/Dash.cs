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
}
