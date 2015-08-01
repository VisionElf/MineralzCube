using UnityEngine;
using System.Collections;

public class Dummy : MonoBehaviour {

    //UNITY PROPERTIES
    public Material defaultMaterial;
    public Vector3 defaultPosition { get; set; }
    public Vector3 defaultRotation;
    public Vector3 defaultScale { get; set; }

    float decalageY;

    //PROPERTIES
    void Awake()
    {
        defaultPosition = transform.localPosition;
        defaultScale = transform.localScale;
        decalageY = defaultPosition.y - (defaultScale.y / 2);
        defaultPosition -= new Vector3(0, decalageY, 0);
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

    public Renderer dummyRenderer
    {
        get { return GetComponent<Renderer>(); }
    }
    public Color color
    {
        get { return dummyRenderer.material.color; }
        set { dummyRenderer.material.color = value; }
    }
    public void ResetColor()
    {
        color = defaultMaterial.color;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
}
