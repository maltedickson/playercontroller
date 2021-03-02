using UnityEngine;

public interface IMovementModifier
{
    Vector3 ModifyVelocity(Vector3 velocity, bool isGrounded);
}