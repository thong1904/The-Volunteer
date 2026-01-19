using UnityEngine;

public class MenuCameraOrbit : MonoBehaviour
{
    public Transform target;
    public float rotateSpeed = 8f;
    public Vector3 lookOffset = new Vector3(0, 1.4f, 0);

    void Update()
    {
        transform.RotateAround(
            target.position,
            Vector3.up,
            rotateSpeed * Time.deltaTime
        );

        transform.LookAt(target.position + lookOffset);
    }
}
