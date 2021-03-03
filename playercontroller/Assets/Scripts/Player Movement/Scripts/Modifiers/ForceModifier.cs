using UnityEngine;

public class ForceModifier : MonoBehaviour, IMovementModifier
{
    Vector3 currentForce = Vector3.zero;
    Vector3 currentRawForce = Vector3.zero;

    public Vector3 Modify(ModifierInfo info, PlayerMoverConfig config)
    {
        currentForce = Vector3.Lerp(currentForce, Vector3.zero, 5 * Time.deltaTime);
        if (currentForce.magnitude > config.maxForce) currentForce = currentForce.normalized * config.maxForce;

        Vector3 currentRawForce = this.currentRawForce;
        this.currentRawForce = Vector3.zero;

        return currentForce + currentRawForce;
    }

    public void AddForce(Vector3 force)
    {
        currentForce += force;
    }
    public void AddRawForce(Vector3 force)
    {
        currentRawForce += force;
    }
}