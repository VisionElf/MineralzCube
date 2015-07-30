using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    //UNITY PROPERTIES
    public float cameraSpeed;

    //FUNCTIONS

    public void PanCamera(Vector3 position)
    {
        Vector3 vec = new Vector3(position.x, transform.position.y, position.z);
        vec.x -= 21;
        vec.z -= 21;
        transform.position = vec;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
            transform.localPosition += cameraSpeed * transform.TransformVector(Vector3.up);
        else if (Input.GetKey(KeyCode.DownArrow))
            transform.localPosition += cameraSpeed * transform.TransformVector(Vector3.down);
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.localPosition += cameraSpeed * transform.TransformVector(Vector3.left);
        else if (Input.GetKey(KeyCode.RightArrow))
            transform.localPosition += cameraSpeed * transform.TransformVector(Vector3.right);
    }
}
