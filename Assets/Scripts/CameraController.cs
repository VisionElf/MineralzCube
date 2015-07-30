using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    //UNITY PROPERTIES
    public float cameraSpeed;
    public int cameraScrollingRange;

    //FUNCTIONS
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void PanCamera(Vector3 position)
    {
        Vector3 vec = new Vector3(position.x, transform.position.y, position.z);
        vec.x -= 21;
        vec.z -= 21;
        transform.position = vec;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y <= cameraScrollingRange)
            transform.localPosition += cameraSpeed * transform.TransformVector(Vector3.up);
        else if (Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y >= Screen.height - cameraScrollingRange)
            transform.localPosition += cameraSpeed * transform.TransformVector(Vector3.down);
        if (Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= cameraScrollingRange)
            transform.localPosition += cameraSpeed * transform.TransformVector(Vector3.left);
        else if (Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - cameraScrollingRange)
            transform.localPosition += cameraSpeed * transform.TransformVector(Vector3.right);
    }
}
