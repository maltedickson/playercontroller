using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLookConfig", menuName = "PlayerLookConfig")]
public class PlayerLookConfig : ScriptableObject
{

    [SerializeField] public float mouseSensitivity = 1f;

    [SerializeField] public float fieldOfView = 90f;

    [SerializeField] public float minVerticalRotation = -90f;
    [SerializeField] public float maxVerticalRotation = 90f;

}
