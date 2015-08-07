using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {

	//PUBLIC
    public string displayName;
    public string displayDescription;
    public Texture icon;
    public Model model;
    public float maxHealth;

    public float radius;

    public int nodeWidth;
    public int nodeHeight;


    public void LookAt(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        if (dir.sqrMagnitude > 0)
        { 
            Quaternion rotation = Quaternion.LookRotation(dir);
            model.transform.rotation = rotation;
        }
    }
}
