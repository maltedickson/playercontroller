using UnityEngine;

public class PlayerForce
{
    Vector3 currentForce = Vector3.zero;

    public Vector3 ApplyForce()
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