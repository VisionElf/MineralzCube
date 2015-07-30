using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    //UNITY PROPERTIES
    public float cameraSpeed;

    //FUNCTIONS

    public void PanCamera(Vector3 position)
    {
        transform.position = new Vector3(position.x, transform.position.y, position.z);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
            transform.position += cameraSpeed * Vector3.forward;
        else if (Input.GetKey(KeyCode.DownArrow))
            transform.position += cameraSpeed * Vector3.back;
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.position += cameraSpeed * Vector3.left;
        else if (Input.GetKey(KeyCode.RightArrow))
            transform.position += cameraSpeed * Vector3.right;
    }
}
