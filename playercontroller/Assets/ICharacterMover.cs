using UnityEngine;

public interface ICharacterMover
{
    Vector3 Velocity { get; }
    bool IsGrounded { get; }
    void Move(Vector3 wishVel);
}
