using UnityEngine;

public class ShowIfBoolAttribute : PropertyAttribute
{
    public string boolName;

    public ShowIfBoolAttribute(string boolName)
    {
        this.boolName = boolName;
    }
}