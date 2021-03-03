using UnityEngine;

public class GravityModifier : MonoBehaviour, IMovementModifier
{
    public Vector3 Modify(ModifierInfo info, PlayerMoverConfig config)
    {
        if (!info.IsGrounded) return Vector3.up * config.gravity * Time.fixedDeltaTime;
        else return Vector3.zero;
    }
}