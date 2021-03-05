using UnityEngine;

public class FrictionModifier : MonoBehaviour, IMovementModifier
{
    public Vector3 Modify(ModifierInfo info, PlayerMoverConfig config)
    {
        if (!info.IsGrounded || info.CurrentHorizontalVelocity.magnitude == 0) return Vector3.zero;

        float speed = info.CurrentHorizontalVelocity.magnitude;
        float drop = speed / config.friction * Time.fixedDeltaTime;

        Vector3 newHorVel = info.CurrentHorizontalVelocity * Mathf.Max(speed - drop, 0f) / speed;
        Vector3 value = new Vector3(newHorVel.x, 0, newHorVel.z) - info.CurrentHorizontalVelocity;

        return value;
    }
}