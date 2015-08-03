using UnityEngine;
using System.Collections;

public class Dummy : MonoBehaviour {

    //UNITY PROPERTIES
    public Material defaultMaterial;
    public Vector3 defaultPosition { get; set; }
    public Vector3 defaultRotation;
    public Vector3 defaultScale { get; set; }

    float decalageY;
    bool visible;

    //PROPERTIES
    void Awake()
    {
        defaultPosition = transform.localPosition;
        defaultScale = transform.localScale;
        decalageY = defaultPosition.y - (defaultScale.y / 2);
        defaultPosition -= new Vector3(0, decalageY, 0);
        visible = true;
    }

    public void ScaleY(float percent)
    {
        if (percent <= 0)
            Hide();
        else
        {
            Show();
            transform.localScale = new Vector3(transform.localScale.x, percent * defaultScale.y, transform.localScale.z);
            transform.localPosition = new Vector3(transform.localPosition.x, decalageY + percent * defaultPosition.y, transform.localPosition.z);
        }
    }

    public Color color
    {
        set {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.material.color = value;
        }
    }
    public Material material
    {
        set
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.material = value;
        }
    }

    public void SetColor(float r, float g, float b)
    {
        if (defaultMaterial != null)
            color = new Color(r, g, b, defaultMaterial.color.a);
        else
            color = new Color(r, g, b, 1f);
    }
    public void SetColor(float r, float g, float b, float a)
    {
        color = new Color(r, g, b, a);
    }

    public void ResetColor()
    {
        color = defaultMaterial.color;
    }

    public void Hide()
    {
        if (visible)
        {
            visible = false;
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = visible;
        }
    }
    public void Show()
    {
        if (!visible)
        {
            visible = true;
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = visible;
        }
    }

    public void DestroyDummy()
    {
        GameObject.Destroy(gameObject);
    }
}
