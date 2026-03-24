using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform targetToFollow;

    private Camera selfCamera;


    private void Start()
    {
        selfCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        transform.position = new Vector3(targetToFollow.position.x, targetToFollow.position.y, transform.position.z);
    }

}
