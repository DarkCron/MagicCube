﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MagicCubeSaveData
{
    public int magicCubeSize = 3;
    public int time = 0;
    public List<Vector3Serializable> positions = new List<Vector3Serializable>();
    public List<QuaternionSerializable> rotations = new List<QuaternionSerializable>();
    public List<ActionSaveData> undoActions = new List<ActionSaveData>();
    public List<int> cubeTileIDs = new List<int>();

    public static MagicCubeSaveData CreateSaveData(MagicCubeManager manager)
    {
        MagicCubeSaveData savedata = new MagicCubeSaveData();
        //deep copies to be safe.
        foreach (var cubeTile in manager.GetCubeTileListReference())
        {
            savedata.positions.Add(Vector3Serializable.CreateVector3Serializable(cubeTile.transform.position));
            savedata.rotations.Add(QuaternionSerializable.CreateQuaternionSerializable(cubeTile.transform.rotation));
            savedata.cubeTileIDs.Add(cubeTile.GetComponent<CubeTileInfo>().GetID());
        }
        foreach (var action in manager.GetUndoListReference())
        {
            savedata.undoActions.Add(ActionSaveData.CreateSaveData(action));
        }

        savedata.magicCubeSize = manager.GetMagicCubeSize();
        savedata.time = manager.GetTimePassed();

        return savedata;
    }
}

[System.Serializable]
public class ActionSaveData
{
    public enum ActionType { ZXClock = 0, ZXCounterClock, ZYClock, ZYCounterClock, YXClock, YXCounterClock}
    public enum CollectionType { Row = 0, ColumnX, ColumnZ }

    public int action = (int)ActionType.ZXClock;
    public List<int> cubeCollection = new List<int>();

    public static ActionSaveData CreateSaveData(CubeAction cubeAction)
    {
        ActionSaveData savedata = new ActionSaveData();
        if (cubeAction.GetType() == typeof(ActionRotateZXClockWise))
        {
            savedata.action = (int)ActionType.ZXClock;
        }else if (cubeAction.GetType() == typeof(ActionRotateZXCounterClockWise))
        {
            savedata.action = (int)ActionType.ZXCounterClock;
        }
        else if (cubeAction.GetType() == typeof(ActionRotateZYClockWise))
        {
            savedata.action = (int)ActionType.ZYClock;
        }
        else if (cubeAction.GetType() == typeof(ActionRotateZYCounterClockWise))
        {
            savedata.action = (int)ActionType.ZYCounterClock;
        }
        else if (cubeAction.GetType() == typeof(ActionRotateYXClockWise))
        {
            savedata.action = (int)ActionType.YXClock;
        }
        else if (cubeAction.GetType() == typeof(ActionRotateYXCounterClockWise))
        {
            savedata.action = (int)ActionType.YXCounterClock;
        }

        foreach (var cubeTile in cubeAction.GetCubeTileListReference())
        {
            savedata.cubeCollection.Add(cubeTile.GetComponent<CubeTileInfo>().GetID());
        }

        return savedata;
    }
}

[System.Serializable]
public class Vector3Serializable
{
    public float x = 0.0f;
    public float y = 0.0f;
    public float z = 0.0f;

    static public Vector3Serializable CreateVector3Serializable(Vector3 v)
    {
        Vector3Serializable save = new Vector3Serializable
        {
            x = v.x,
            y = v.y,
            z = v.z
        };
        return save;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x,y,z);
    }
}

[System.Serializable]
public class QuaternionSerializable
{
    public float x = 0.0f;
    public float y = 0.0f;
    public float z = 0.0f;
    public float w = 0.0f;

    static public QuaternionSerializable CreateQuaternionSerializable(Quaternion q)
    {
        QuaternionSerializable save = new QuaternionSerializable
        {
            x = q.x,
            y = q.y,
            z = q.z,
            w = q.w
        };
        return save;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(x,y,z,w);
    }
}