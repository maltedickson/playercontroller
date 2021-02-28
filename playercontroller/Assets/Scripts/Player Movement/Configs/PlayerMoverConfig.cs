using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMoverConfig", menuName = "PlayerMoverConfig")]
public class PlayerMoverConfig : ScriptableObject
{

    [Header("Collider")]
    [SerializeField] public float height = 2f;
    [SerializeField] public float radius = 0.5f;
    [SerializeField] public PhysicMaterial noFrictionPhysicMaterial = null;

    [Header("Slopes and Steps")]
    [SerializeField] public float slopeLimit = 50f;
    [SerializeField] public float maxStepUpHeight = 0.3f;
    [SerializeField] public float maxStepDownHeight = 0.4f;

}
