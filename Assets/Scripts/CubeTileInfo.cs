﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTileInfo : MonoBehaviour
{
    private int ID = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetID(int ID)
    {
        this.ID = ID;
    }

    public int GetID()
    {
        return ID;
    }
}
