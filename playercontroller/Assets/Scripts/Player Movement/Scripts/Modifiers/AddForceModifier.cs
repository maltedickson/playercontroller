using UnityEngine;

public class AddForceModifier : MonoBehaviour, IMovementModifier
{
    Vector3 forceToAdd = Vector3.zero;

    public Vector3 Modify(ModifierInfo info, PlayerMoverConfig config)
    {
        Vector3 forceToAdd = this.forceToAdd;
        this.forceToAdd = Vector3.zero;
        return forceToAdd;
    }

    public void AddForce(Vector3 force)
    {
        forceToAdd += force;
    }
}