using UnityEngine;
using System.Collections;

public enum ECaseType
{
    Empty, Rock, Mineral
}

public class Case {

    public ECaseType caseType;
    public int x;
    public int y;
    public Vector3 position;
    public Color color;

    public Case(int _x, int _y, ECaseType _caseType, Vector3 _position)
    {
        x = _x;
        y = _y;
        caseType = _caseType;
        position = _position;
        color = new Color(0, 0, 0, 0);
    }

    public Color GetColor()
    {
        if (color.a > 0)
            return color;
        switch (caseType)
        {
            case ECaseType.Empty:
                return Color.white;
            case ECaseType.Rock:
                return Color.black;
            case ECaseType.Mineral:
                return Color.blue;
            default:
                return Color.white;
        }
    }

    static public ECaseType GetCaseType(int id)
    {
        switch (id)
        {
            case 1:
                return ECaseType.Rock;
            case 2:
                return ECaseType.Mineral;
            default:
                return ECaseType.Empty;
        }
    }
}
