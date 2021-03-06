using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLookConfig", menuName = "PlayerLookConfig")]
public class PlayerLookConfig : ScriptableObject
{

    [SerializeField] public float mouseSensitivity = 0.05f;

    [SerializeField] public float fieldOfView = 90f;

    [SerializeField] public float minVerticalRotation = -90f;
    [SerializeField] public float maxVerticalRotation = 90f;

    [SerializeField] public float cameraOffset = 1.5f;

}
