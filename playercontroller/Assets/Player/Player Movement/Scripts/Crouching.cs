using UnityEngine;

public class Crouching
{
    float normalHeight;
    float crouchingHeight;

    float transitionTime;
    float currentVelocity;

    public float currentHeight { get; private set; }
    public bool isCrouching { get; private set; }

    public Crouching(float normalHeight, float crouchingHeight, float transitionTime)
    {
        this.normalHeight = normalHeight;
        this.crouchingHeight = crouchingHeight;

        this.transitionTime = transitionTime;

        currentHeight = normalHeight;
    }

    public void SetIsCrouching(bool wantsToCrouch, Transform transform, float radius)
    {
        isCrouching = wantsToCrouch;

        float margin = 0.05f;

        RaycastHit hit;
        bool hasToCrouch = Physics.SphereCast(
            transform.position + Vector3.up * (radius - margin),
            radius - margin,
            Vector3.up,
            out hit,
            normalHeight - radius * 2f + margin * 2f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (hasToCrouch) isCrouching = true;
    }

    public void UpdateHeight()
    {
        float targetHeight = isCrouching ? crouchingHeight : normalHeight;
        currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref currentVelocity, transitionTime);
    }
}