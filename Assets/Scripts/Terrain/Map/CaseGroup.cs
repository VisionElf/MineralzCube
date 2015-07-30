using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CaseGroup {

    public List<Case> cases;

    public CaseGroup()
    {
        cases = new List<Case>();
    }

    public void Add(Case c)
    {
        cases.Add(c);
    }
    public bool Contains(Case c)
    {
        return cases.Contains(c);
    }

    float GetAverageDistance(Case center)
    {
        float dist = 0;
        foreach (Case c in cases)
            if (c != center)
                dist += Vector3.Distance(center.position, c.position);

        return dist / (cases.Count - 1);
    }

    public Case GetCenterCase()
    {
        Case temp = cases[0];
        float avgDist = GetAverageDistance(temp);

        for (int i = 1; i < cases.Count; i++)
        {
            float dist = GetAverageDistance(cases[i]);
            if (avgDist > dist)
            {
                temp = cases[i];
                avgDist = dist;
            }
        }
        return temp;
    }
}
