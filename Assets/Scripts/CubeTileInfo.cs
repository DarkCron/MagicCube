using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTileInfo : MonoBehaviour
{
    private int ID = -1;

    public void SetID(int ID)
    {
        this.ID = ID;
    }

    public int GetID()
    {
        return ID;
    }
}
