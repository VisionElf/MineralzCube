using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    //PUBLIC
    public float scrollingSpeed;
    public float scrollingRange;
    public bool allowMouseScrolling;
    public float zoomMax;
    public float zoomMin;

    void Start()
    {
        PanCamera(Map.instance.builder.transform.position);
    }

    public void PanCamera(Vector3 target)
    {
        transform.position = new Vector3(target.x, transform.position.y, target.z - (Mathf.Tan((90 - transform.rotation.eulerAngles.x) * Mathf.Deg2Rad) * transform.position.y));
    }

    void Update()
    {
        if (Input.GetButton("CameraY") && Input.GetAxisRaw("CameraY") > 0 || (allowMouseScrolling && Input.mousePosition.y > Screen.height - scrollingRange))
            transform.position += Vector3.forward * scrollingSpeed * Time.deltaTime;
        else if (Input.GetButton("CameraY") && Input.GetAxisRaw("CameraY") < 0 || (allowMouseScrolling && Input.mousePosition.y < scrollingRange))
            transform.position += Vector3.back * scrollingSpeed * Time.deltaTime;
        if (Input.GetButton("CameraX") && Input.GetAxisRaw("CameraX") > 0 || (allowMouseScrolling && Input.mousePosition.x > Screen.width - scrollingRange))
            transform.position += Vector3.right * scrollingSpeed * Time.deltaTime;
        else if (Input.GetButton("CameraX") && Input.GetAxisRaw("CameraX") < 0 || (allowMouseScrolling && Input.mousePosition.x < scrollingRange))
            transform.position += Vector3.left * scrollingSpeed * Time.deltaTime;

        if (Input.mouseScrollDelta.y > 0)
            transform.position += transform.TransformVector(Vector3.forward) * scrollingSpeed * Time.deltaTime;
        else if (Input.mouseScrollDelta.y < 0)
            transform.position += transform.TransformVector(Vector3.back) * scrollingSpeed * Time.deltaTime;
    }
}
