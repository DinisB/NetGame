using UnityEngine;

public class CameraZ : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
}
