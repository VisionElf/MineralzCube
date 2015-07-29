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

    public Case(int _x, int _y, ECaseType _caseType)
    {
        x = _x;
        y = _y;
        caseType = _caseType;
    }

    public Color GetColor()
    {
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
