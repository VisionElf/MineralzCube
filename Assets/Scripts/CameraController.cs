using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    //UNITY PROPERTIES
    public float cameraSpeed;
    public int cameraScrollingRange;
    public bool cameraScrollActive;

    public int cameraZooming;
    public int cameraMaxZooming;

    //FUNCTIONS
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public Camera mainCamera
    {
        get { return GetComponent<Camera>(); }
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
        if (Other.gameStarted)
        {
            if (Input.GetKey(KeyCode.UpArrow) || (cameraScrollActive && Input.mousePosition.y >= Screen.height + cameraScrollingRange))
                transform.localPosition += new Vector3(1, 0, 1) * cameraSpeed * Time.deltaTime;
            else if (Input.GetKey(KeyCode.DownArrow) || (cameraScrollActive && Input.mousePosition.y <= cameraScrollingRange))
                transform.localPosition += new Vector3(-1, 0, -1) * cameraSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftArrow) || (cameraScrollActive && Input.mousePosition.x <= cameraScrollingRange))
                transform.localPosition += new Vector3(-1, 0, 1) * cameraSpeed * Time.deltaTime;
            else if (Input.GetKey(KeyCode.RightArrow) || (cameraScrollActive && Input.mousePosition.x >= Screen.width - cameraScrollingRange))
                transform.localPosition += new Vector3(1, 0, -1) * cameraSpeed * Time.deltaTime;


            if (Input.GetMouseButtonDown(2))
            {
                float temp = Mathf.Sign(10 - mainCamera.orthographicSize);
                while (mainCamera.orthographicSize != 10)
                {
                    mainCamera.orthographicSize += cameraZooming * temp;
                    transform.localPosition += new Vector3(-1, 1, -1) * cameraZooming * temp;
                }
            }
            else if (Input.mouseScrollDelta.y > 0 && mainCamera.orthographicSize > cameraZooming)
            {
                mainCamera.orthographicSize -= cameraZooming;
                transform.localPosition += new Vector3(1, -1, 1) * cameraZooming;
            }
            else if (Input.mouseScrollDelta.y < 0 && mainCamera.orthographicSize < cameraMaxZooming)
            {
                mainCamera.orthographicSize += cameraZooming;
                transform.localPosition += new Vector3(-1, 1, -1) * cameraZooming;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    void OnGUI()
    {
        if (!Other.gameStarted)
        {
            int width = 800;
            int height = 20;
            int border = 5;
            Rect fullRect = new Rect(Screen.width / 2 - width / 2, Screen.height - 2 * height, width, height);
            float percent = (float)Other.loading / Other.maxLoading;
            Rect rect = new Rect(fullRect.x + border, fullRect.y + border, percent * (fullRect.width - 2 * border), fullRect.height - 2 * border);

            GUIStyle style = new GUIStyle();
            style.fontSize = 30;
            style.font = Resources.Load("AGENCYB") as Font;
            
            GUIContent content = new GUIContent(Other.oldStep);
            Vector3 textSize = style.CalcSize(content);

            GUI.color = Color.white;
            GUI.Label(new Rect(fullRect.x + width / 2 - textSize.x / 2, fullRect.y - 50, width, 200), content, style);
            GUI.DrawTexture(fullRect, Static.basic_texture);
            GUI.color = Color.gray;
            GUI.DrawTexture(rect, Static.basic_texture);
        }
    }
}
