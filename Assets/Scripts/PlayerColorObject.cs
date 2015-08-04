using UnityEngine;
using System.Collections;

public class PlayerColorObject : MonoBehaviour {

    public void SetColor(Color newColor)
    {
        Renderer renderer = GetComponent<Renderer>();
        Color color = renderer.material.color;
        renderer.material.color = new Color(Mathf.Min(newColor.r, color.r), Mathf.Min(newColor.g, color.g), Mathf.Min(newColor.b, color.b), Mathf.Min(newColor.a, color.a));
    }
}
