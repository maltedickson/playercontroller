using UnityEngine;

public class ForceModifier : MonoBehaviour, IMovementModifier
{
    Vector3 currentForce = Vector3.zero;

    public Vector3 Modify(ModifierInfo info, PlayerMoverConfig config)
    {
        Vector3 currentForce = this.currentForce;
        this.currentForce = Vector3.zero;

        return currentForce;
    }

    public void AddForce(Vector3 force)
    {
        currentForce += force;
    }
}