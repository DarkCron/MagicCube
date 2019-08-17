using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeControl : MonoBehaviour
{
    public GameObject tileRef;
    private MagicCubeManager manager;

    // Start is called before the first frame update
    void Start()
    {
        // manager = new MagicCubeManager(tileRef, 8, new Vector3(0, 2, 0));
    }

    // Update is called once per frame
    void Update()
    {
        if (!MainGameLogic.IsMainGame())
        {
            return;
        }

        if (manager != null)
        {
            manager.Update();
        }

        if (Input.GetKeyDown(KeyCode.Home))
        {
            if (MainGameLogic.IsMainGame() || MainGameLogic.IsGameMenu())
            {
                manager.Save();
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (MainGameLogic.IsMainGame() || MainGameLogic.IsGameMenu())
        {
            manager.Save();
        }
    }

    public void CreateMagicCube(int size)
    {
        if (manager != null)
        {
            manager.Dispose();
        }
        manager = new MagicCubeManager(tileRef, size, new Vector3(0, 2, 0));
    }

    public MagicCubeManager GetMagicCubeManager()
    {
        return manager;
    }
}

public class MagicCubeManager
{
    //This game's magic cube size (cubed)
    private int magicCubeSize = 2;
    private GameObject cubeTileRef;
    private List<GameObject> cubeTiles;
    private Vector3 mainPivot;

    private CubeCollectionHelper collectionHelper;

    private GameObject ownHolder;

    private BoxCollider colliderPosX;
    private BoxCollider colliderNegX;
    private BoxCollider colliderPosY;
    private BoxCollider colliderNegY;
    private BoxCollider colliderPosZ;
    private BoxCollider colliderNegZ;

    private List<CubeAction> actionList;
    private CubeAction currentAction;

    public enum MagicCubeSide { PosX, NegX, PosY, NegY, PosZ, NegZ, None }

    public delegate void UpdateTimer(int timePassed);
    private UpdateTimer updateTime;
    private float timePassed = 0.0f;
    private int seconds = 0;

    public delegate void ProcessUndoRedoPossible(bool bCanUndo);
    ProcessUndoRedoPossible processUndoRedoPossible;

    public delegate void ProcessFinishGameSetTime(int time);
    ProcessFinishGameSetTime processFinishGameSetTime;
    private bool bGameIsFinished = false;

    private bool bProcessingRandomActions = false;
    private float randomActionsTimePassed = 0.0f;
    private float randomActionTimer = 3.0f;
    private bool bIsLoading = false;

    public MagicCubeManager(GameObject cubeTileRef, int n, Vector3 pivot)
    {
        ownHolder = new GameObject("Magic Cube");
        ownHolder.transform.position = Vector3.zero;
        ownHolder.layer = 10;
        MagicCubeBehaviour behaviour = ownHolder.AddComponent<MagicCubeBehaviour>();
        behaviour.Init(this);
        ownHolder.tag = "MagicCube";

        actionList = new List<CubeAction>();

        this.cubeTileRef = cubeTileRef;
        magicCubeSize = n;
        cubeTiles = new List<GameObject>();
        mainPivot = pivot;

        collectionHelper = new CubeCollectionHelper(0);
        GenerateCubeTiles();

        Camera.main.transform.position = new Vector3(mainPivot.x, mainPivot.y, Camera.main.GetComponent<CameraControl>().GetCurrentZoom());
        Camera.main.transform.rotation = new Quaternion();
    }

    public void LinkGameTimer(MainGameUI mainGameUI)
    {
        updateTime = mainGameUI.SetTimer;
    }
    public void LinkUndoRedo(MainGameUI mainGameUI)
    {
        mainGameUI.SetUndoRedoActions(UndoLastMove);
    }
    public void LinkProcessUndoRedoPossible(MainGameUI mainGameUI)
    {
        processUndoRedoPossible = mainGameUI.ProcessUndoRedoList;
    }
    public void LinkProcessOpenMenu(MainGameUI mainGameUI)
    {
        mainGameUI.SetMenuAction(Camera.main.GetComponent<UIManager>().OpenGameMenu);
    }
    public void LinkProcessFinishGame(FinishCanvasHandler finishCanvasHandler)
    {
        processFinishGameSetTime = finishCanvasHandler.StartFinish;
    }

    public bool IsCurrentActionDone()
    {
        return currentAction == null;
    }

    internal Vector3 GetWorldPos()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("CubeTileLayer");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            return hit.point;
        }

        return Vector3.positiveInfinity;
    }

    internal void LoadGame(MagicCubeSaveData savedata)
    {
        bIsLoading = true;
        foreach (var item in cubeTiles)
        {
            UnityEngine.Object.Destroy(item);
        }

        cubeTiles.Clear();
        collectionHelper = new CubeCollectionHelper(0);

        for (int i = 0; i < savedata.positions.Count; i++)
        {
            var temp = GameObject.Instantiate(cubeTileRef,
                  savedata.positions[i].ToVector3()
                , savedata.rotations[i].ToQuaternion());
            collectionHelper.ProcessCubeTile(temp, temp.transform.position);
            temp.transform.SetParent(ownHolder.transform);
            cubeTiles.Add(temp);
            temp.GetComponent<CubeTileInfo>().SetID(savedata.cubeTileIDs[i]);
        }

        actionList.Clear();
        for (int i = 0; i < savedata.undoActions.Count; i++)
        {
            CubeAction action = null;
            List<GameObject> cubeTilesForAction = new List<GameObject>();
            foreach (var cubeID in savedata.undoActions[i].cubeCollection)
            {
                cubeTilesForAction.Add(cubeTiles.Find(ct => ct.GetComponent<CubeTileInfo>().GetID() == cubeID));
            }
            switch (savedata.undoActions[i].action)
            {
                case (int)ActionSaveData.ActionType.ZXClock:
                    action = new ActionRotateZXClockWise(ownHolder,this, cubeTilesForAction, true);
                    break;
                case (int)ActionSaveData.ActionType.ZXCounterClock:
                    action = new ActionRotateZXCounterClockWise(ownHolder, this, cubeTilesForAction, true);
                    break;
                case (int)ActionSaveData.ActionType.YXClock:
                    action = new ActionRotateYXClockWise(ownHolder, this, cubeTilesForAction, true);
                    break;
                case (int)ActionSaveData.ActionType.YXCounterClock:
                    action = new ActionRotateYXCounterClockWise(ownHolder, this, cubeTilesForAction, true);
                    break;
                case (int)ActionSaveData.ActionType.ZYClock:
                    action = new ActionRotateZYClockWise(ownHolder, this, cubeTilesForAction, true);
                    break;
                case (int)ActionSaveData.ActionType.ZYCounterClock:
                    action = new ActionRotateZYCounterClockWise(ownHolder, this, cubeTilesForAction, true);
                    break;
            }
            if (action == null)
            {
                actionList.Clear();
                break;
            }
            action.CompleteActionNoAnimation();
            actionList.Add(action);
        }

        timePassed = savedata.time;
        seconds = savedata.time;
        updateTime(seconds);
        processUndoRedoPossible(actionList.Count > 0);
        bIsLoading = false;
    }

    public void StartActionRemoveTile(List<GameObject> tiles)
    {
        foreach (var tile in tiles)
        {
            collectionHelper.RemoveCubeTile(tile);
        }
    }

    public void EndActionReinsertTile(List<GameObject> tiles)
    {
        foreach (var tile in tiles)
        {
            collectionHelper.ProcessCubeTile(tile);
        }
    }

    struct CubeCollectionHelper
    {
        SameCoordinateHelperX data_X;
        SameCoordinateHelperY data_Y;
        SameCoordinateHelperZ data_Z;

        public CubeCollectionHelper(int mock)
        {
            data_X = new SameCoordinateHelperX(0);
            data_Y = new SameCoordinateHelperY(0);
            data_Z = new SameCoordinateHelperZ(0);
        }

        public void ProcessCubeTile(GameObject tile)
        {
            data_X.ProcessCubeTile(tile, tile.transform.position);
            data_Y.ProcessCubeTile(tile, tile.transform.position);
            data_Z.ProcessCubeTile(tile, tile.transform.position);
        }

        public void ProcessCubeTile(GameObject tile, Vector3 position)
        {
            data_X.ProcessCubeTile(tile, position);
            data_Y.ProcessCubeTile(tile, position);
            data_Z.ProcessCubeTile(tile, position);
        }

        public void RemoveCubeTile(GameObject tile)
        {
            data_X.RemoveCubeTile(tile);
            data_Y.RemoveCubeTile(tile);
            data_Z.RemoveCubeTile(tile);
        }

        public void Report()
        {
            data_X.Report();
            data_Y.Report();
            data_Z.Report();
        }

        public Vector3Int GetIndicesFromCube(GameObject tile)
        {
            return new Vector3Int(data_X.GenerateIndex(tile), data_Y.GenerateIndex(tile), data_Z.GenerateIndex(tile));
        }

        struct SameCoordinateHelperX
        {
            Dictionary<int, List<GameObject>> data;

            public SameCoordinateHelperX(int mock)
            {
                data = new Dictionary<int, List<GameObject>>();
            }

            public int GenerateIndex(GameObject cubeTile)
            {
                return Mathf.RoundToInt(cubeTile.transform.position.x);
            }

            public void ProcessCubeTile(GameObject tile, Vector3 position)
            {
                if (data.ContainsKey(GenerateIndex(tile)))
                {
                    data[GenerateIndex(tile)].Add(tile);
                }
                else
                {
                    data.Add(GenerateIndex(tile), new List<GameObject> { tile });
                }
            }

            public Dictionary<int, List<GameObject>> GetData()
            {
                return data;
            }

            public void Report()
            {
                Debug.Log("Helper X Report:");
                Debug.Log("Entries: " + data.Keys.Count);
                foreach (var list in data.Values)
                {
                    Debug.Log("Entry length: " + list.Count);
                }
                Debug.Log("***************");
            }

            internal void RemoveCubeTile(GameObject tile)
            {
                data[Mathf.RoundToInt(tile.transform.position.x)].Remove(tile);
            }
        }
        struct SameCoordinateHelperY
        {
            Dictionary<int, List<GameObject>> data;

            public SameCoordinateHelperY(int mock)
            {
                data = new Dictionary<int, List<GameObject>>();
            }

            public int GenerateIndex(GameObject cubeTile)
            {
                return Mathf.RoundToInt(cubeTile.transform.position.y);
            }

            public void ProcessCubeTile(GameObject tile, Vector3 position)
            {
                if (data.ContainsKey(GenerateIndex(tile)))
                {
                    data[GenerateIndex(tile)].Add(tile);
                }
                else
                {
                    data.Add(GenerateIndex(tile), new List<GameObject> { tile });
                }
            }

            public Dictionary<int, List<GameObject>> GetData()
            {
                return data;
            }

            public void Report()
            {
                Debug.Log("Helper Y Report:");
                Debug.Log("Entries: " + data.Keys.Count);
                foreach (var list in data.Values)
                {
                    Debug.Log("Entry length: " + list.Count);
                }
                Debug.Log("***************");
            }

            internal void RemoveCubeTile(GameObject tile)
            {
                data[Mathf.RoundToInt(tile.transform.position.y)].Remove(tile);
            }
        }
        struct SameCoordinateHelperZ
        {
            Dictionary<int, List<GameObject>> data;

            public SameCoordinateHelperZ(int mock)
            {
                data = new Dictionary<int, List<GameObject>>();
            }

            public int GenerateIndex(GameObject cubeTile)
            {
                return Mathf.RoundToInt(cubeTile.transform.position.z);
            }

            public void ProcessCubeTile(GameObject tile, Vector3 position)
            {
                if (data.ContainsKey(GenerateIndex(tile)))
                {
                    data[GenerateIndex(tile)].Add(tile);
                }
                else
                {
                    data.Add(GenerateIndex(tile), new List<GameObject> { tile });
                }
            }

            public Dictionary<int, List<GameObject>> GetData()
            {
                return data;
            }

            public void Report()
            {
                Debug.Log("Helper Z Report:");
                Debug.Log("Entries: " + data.Keys.Count);
                foreach (var list in data.Values)
                {
                    Debug.Log("Entry length: " + list.Count);
                }
                Debug.Log("***************");
            }

            internal void RemoveCubeTile(GameObject tile)
            {
                data[Mathf.RoundToInt(tile.transform.position.z)].Remove(tile);
            }
        }

        public List<GameObject> GetRowTiles(int index)
        {
            return data_Y.GetData()[index];
        }

        public List<GameObject> GetColumnXTiles(int index)
        {
            return data_X.GetData()[index];
        }

        public List<GameObject> GetColumnZTiles(int index)
        {
            return data_Z.GetData()[index];
        }

        internal List<GameObject> GetColumnXTiles(GameObject cubeTile)
        {
            return data_X.GetData()[GetIndicesFromCube(cubeTile).x];
        }

        internal List<GameObject> GetRowTiles(GameObject cubeTile)
        {
            return data_Y.GetData()[GetIndicesFromCube(cubeTile).y];
        }

        internal List<GameObject> GetColumnZTiles(GameObject cubeTile)
        {
            return data_Z.GetData()[GetIndicesFromCube(cubeTile).z];
        }
    }

    private void GenerateCubeTiles()
    {
        Vector3 size = cubeTileRef.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        mainPivot = size * cubeTileRef.transform.localScale.z * (magicCubeSize - 1) / 2;
        ownHolder.transform.position = mainPivot;

        for (int i = 0; i < magicCubeSize; i++)
        {
            for (int j = 0; j < magicCubeSize; j++)
            {
                for (int k = 0; k < magicCubeSize; k++)
                {
                    if (k == 0 || j == 0 || i == 0 || k == magicCubeSize - 1 || j == magicCubeSize - 1 || i == magicCubeSize - 1)
                    {
                        var temp = GameObject.Instantiate(cubeTileRef,
                              cubeTileRef.transform.up * k * size.y * cubeTileRef.transform.localScale.y
                            + cubeTileRef.transform.forward * j * size.z * cubeTileRef.transform.localScale.z
                            + cubeTileRef.transform.right * i * size.x * cubeTileRef.transform.localScale.x
                            , cubeTileRef.transform.rotation);
                        collectionHelper.ProcessCubeTile(temp, temp.transform.position);
                        temp.transform.SetParent(ownHolder.transform);
                        cubeTiles.Add(temp);
                        temp.GetComponent<CubeTileInfo>().SetID(cubeTiles.Count);
                    }
                }
            }
        }
        //collectionHelper.Report();
        BoxCollider colliderPosZ = ownHolder.AddComponent<BoxCollider>();
        colliderPosZ.center = new Vector3(0, 0, (size * cubeTileRef.transform.localScale.z * (magicCubeSize) / 2).z);
        colliderPosZ.size = size * cubeTileRef.transform.localScale.z * magicCubeSize;
        colliderPosZ.size = new Vector3(colliderPosZ.size.x, colliderPosZ.size.y, 0.1f);

        BoxCollider colliderNegZ = ownHolder.AddComponent<BoxCollider>();
        colliderNegZ.center = new Vector3(0, 0, -(size * cubeTileRef.transform.localScale.z * (magicCubeSize) / 2).z);
        colliderNegZ.size = size * cubeTileRef.transform.localScale.z * magicCubeSize;
        colliderNegZ.size = new Vector3(colliderNegZ.size.x, colliderNegZ.size.y, 0.1f);

        BoxCollider colliderPosX = ownHolder.AddComponent<BoxCollider>();
        colliderPosX.center = new Vector3((size * cubeTileRef.transform.localScale.x * (magicCubeSize) / 2).x, 0, 0);
        colliderPosX.size = size * cubeTileRef.transform.localScale.x * magicCubeSize;
        colliderPosX.size = new Vector3(0.1f, colliderPosX.size.y, colliderPosX.size.z);

        BoxCollider colliderNegX = ownHolder.AddComponent<BoxCollider>();
        colliderNegX.center = new Vector3(-(size * cubeTileRef.transform.localScale.x * (magicCubeSize) / 2).x, 0, 0);
        colliderNegX.size = size * cubeTileRef.transform.localScale.x * magicCubeSize;
        colliderNegX.size = new Vector3(0.1f, colliderNegX.size.y, colliderNegX.size.z);

        BoxCollider colliderPosY = ownHolder.AddComponent<BoxCollider>();
        colliderPosY.center = new Vector3(0, (size * cubeTileRef.transform.localScale.x * (magicCubeSize) / 2).y, 0);
        colliderPosY.size = size * cubeTileRef.transform.localScale.x * magicCubeSize;
        colliderPosY.size = new Vector3(colliderPosY.size.x, 0.1f, colliderPosY.size.z);

        BoxCollider colliderNegY = ownHolder.AddComponent<BoxCollider>();
        colliderNegY.center = new Vector3(0, -(size * cubeTileRef.transform.localScale.x * (magicCubeSize) / 2).y, 0);
        colliderNegY.size = size * cubeTileRef.transform.localScale.x * magicCubeSize;
        colliderNegY.size = new Vector3(colliderNegY.size.x, 0.1f, colliderNegY.size.z);

        colliderPosX.isTrigger = true;
        colliderNegX.isTrigger = true;
        colliderPosY.isTrigger = true;
        colliderNegY.isTrigger = true;
        colliderPosZ.isTrigger = true;
        colliderNegZ.isTrigger = true;

        this.colliderPosX = colliderPosX;
        this.colliderNegX = colliderNegX;
        this.colliderPosY = colliderPosY;
        this.colliderNegY = colliderNegY;
        this.colliderPosZ = colliderPosZ;
        this.colliderNegZ = colliderNegZ;
    }

    public int GetMagicCubeSize()
    {
        return magicCubeSize;
    }

    public void Update()
    {
        if (bProcessingRandomActions)
        {
            randomActionsTimePassed += Time.deltaTime;
            if (randomActionsTimePassed > randomActionTimer)
            {
                bProcessingRandomActions = false;
                return;
            }
            if (currentAction != null)
            {
                if (!currentAction.IsActionDone())
                {
                    currentAction.Update();
                }
                else
                {
                    currentAction = null;
                }
            }

            if (currentAction == null)
            {
                currentAction = generateRandomAction();
            }
            return;
        }

        if (currentAction != null)
        {
            if (!currentAction.IsActionDone())
            {
                currentAction.Update();
            }
            else
            {
                currentAction = null;

                //After a move is fully completed check whether the game is finished or not 
                if (CheckIfGameIsFinished())
                {
                    FinishGame();
                }
            }
        }

        if (!bGameIsFinished)
        {
            timePassed += Time.deltaTime;
            if ((int)timePassed > seconds)
            {
                seconds = (int)timePassed;
                updateTime(seconds);
            }
        }
        bool bIsPinching = Input.touchCount >= 2;

        if (Input.GetMouseButtonDown(1) && !bIsPinching)
        {
            Camera.main.GetComponent<UIManager>().OpenGameMenu();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Save();
        }
    }

    public bool CheckIfGameIsFinished()
    {
        //Because of the way I programmed this game, a game should be finished if ALL cubes have the
        //same rotation
        float test = Mathf.Infinity;
        foreach (var cubeTile in cubeTiles)
        {
            if (float.IsInfinity(test))
            {
                test = cubeTile.transform.rotation.eulerAngles.sqrMagnitude;
                continue;
            }

            //NOTE: Vector3 sqrMagnitude is much faster than Magnitude
            if (!FloatIsInRange(test, cubeTile.transform.rotation.eulerAngles.sqrMagnitude , 1.0f))
            {
                return false;
            }
        }
        return true;
    }

    private bool FloatIsInRange(float target, float value, float range)
    {
        return Mathf.Abs(value - target) <= range;
    }

    public void FinishGame()
    {
        processFinishGameSetTime((int)timePassed);
        actionList.Clear();
        processUndoRedoPossible(false);
        bGameIsFinished = true;
    }

    public GameObject QueryFirstCubeTile()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("CubeTileLayer");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            return hit.transform.gameObject;
        }
        return null;
    }

    internal Collider QueryMagicCubeFace()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("MainGameLayer");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            return hit.collider;
        }
        return null;
    }

    public void QuerySliceRotation(GameObject cubeTile, Collider startFace, MagicCubeBehaviour.SwipeDragDirection direction)
    {
        MagicCubeSide side = GetWhichSide(startFace);
        switch (side)
        {
            case MagicCubeSide.PosX:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        currentAction = new ActionRotateYXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        currentAction = new ActionRotateYXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        currentAction = new ActionRotateZXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        currentAction = new ActionRotateZXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            case MagicCubeSide.NegX:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        currentAction = new ActionRotateYXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        currentAction = new ActionRotateYXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        currentAction = new ActionRotateZXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        currentAction = new ActionRotateZXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            case MagicCubeSide.PosY:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        currentAction = new ActionRotateYXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        currentAction = new ActionRotateYXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        currentAction = new ActionRotateZYClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        currentAction = new ActionRotateZYCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            case MagicCubeSide.NegY:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        currentAction = new ActionRotateYXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        currentAction = new ActionRotateYXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        currentAction = new ActionRotateZYClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        currentAction = new ActionRotateZYCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            case MagicCubeSide.PosZ:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        currentAction = new ActionRotateZYClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        currentAction = new ActionRotateZYCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        currentAction = new ActionRotateZXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        currentAction = new ActionRotateZXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            case MagicCubeSide.NegZ:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        currentAction = new ActionRotateZYClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        currentAction = new ActionRotateZYCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        currentAction = new ActionRotateZXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        currentAction = new ActionRotateZXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

        if (currentAction != null)
        {
            AddActionToUndoList(currentAction);
        }
    }

    public bool IsPerformingAction()
    {
        if (currentAction != null)
        {
            return !currentAction.IsActionDone();
        }
        else
        {
            return false;
        }
    }

    public MagicCubeSide GetWhichSide(Collider collider)
    {
        if (collider == colliderPosX)
        {
            return MagicCubeSide.PosX;
        }
        else if (collider == colliderNegX)
        {
            return MagicCubeSide.NegX;
        }
        else if (collider == colliderPosY)
        {
            return MagicCubeSide.PosY;
        }
        else if (collider == colliderNegY)
        {
            return MagicCubeSide.NegY;
        }
        else if (collider == colliderPosZ)
        {
            return MagicCubeSide.PosZ;
        }
        else if (collider == colliderNegZ)
        {
            return MagicCubeSide.NegZ;
        }
        throw new UnityException();
    }

    public Vector3 GetPivot()
    {
        return mainPivot;
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(colliderNegX);
        UnityEngine.Object.Destroy(colliderPosX);
        UnityEngine.Object.Destroy(colliderNegY);
        UnityEngine.Object.Destroy(colliderPosY);
        UnityEngine.Object.Destroy(colliderNegZ);
        UnityEngine.Object.Destroy(colliderPosZ);

        foreach (var item in cubeTiles)
        {
            UnityEngine.Object.Destroy(item);
        }
        UnityEngine.Object.Destroy(ownHolder);
    }

    public void AddActionToUndoList(CubeAction action)
    {
        actionList.Add(currentAction);

        processUndoRedoPossible(actionList.Count > 0);
    }

    public void UndoLastMove()
    {
        if (currentAction == null && actionList.Count != 0)
        {
            //Debug.Log("Trying Replay");
            CubeAction lastAction = actionList[actionList.Count - 1];
            actionList.Remove(lastAction);
            lastAction.GetUndoAction().StartAction();
            currentAction = lastAction.GetUndoAction();
        }

        processUndoRedoPossible(actionList.Count > 0);
    }

    public void InitRandomMoves(float howLongToGenerateRandomMoves = 3.0f)
    {
        randomActionTimer = howLongToGenerateRandomMoves;
        bProcessingRandomActions = true;
    }

    public CubeAction generateRandomAction()
    {
        int random = UnityEngine.Random.Range(0, 5);
        CubeAction action = null;
        if (random == 0)
        {
            action = new ActionRotateZXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTiles[UnityEngine.Random.Range(0, cubeTiles.Count - 1)])), true);
        }
        else if (random == 1)
        {
            action = new ActionRotateZXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTiles[UnityEngine.Random.Range(0, cubeTiles.Count - 1)])), true);
        }
        else if (random == 2)
        {
            action = new ActionRotateZYClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTiles[UnityEngine.Random.Range(0, cubeTiles.Count - 1)])), true);
        }
        else if (random == 3)
        {
            action = new ActionRotateZYCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTiles[UnityEngine.Random.Range(0, cubeTiles.Count - 1)])), true);
        }
        else if (random == 4)
        {
            action = new ActionRotateYXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTiles[UnityEngine.Random.Range(0, cubeTiles.Count - 1)])), true);
        }
        else if (random == 5)
        {
            action = new ActionRotateYXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTiles[UnityEngine.Random.Range(0, cubeTiles.Count - 1)])), true);
        }
        if (action == null)
        {
            Debug.Log("ERROR");
        }
        action.SetTimeToComplete(0.3f);
        return action;
    }

    public Vector3Int QueryHelperCubeTileIndices(GameObject cubeTile)
    {
        return collectionHelper.GetIndicesFromCube(cubeTile);
    }

    public List<CubeAction> GetUndoListReference()
    {
        return actionList;
    }

    public List<GameObject> GetCubeTileListReference()
    {
        return cubeTiles;
    }

    internal int GetTimePassed()
    {
        return seconds;
    }

    public void Save()
    {
        if (bGameIsFinished)
        {
            return;
        }
        MagicCubeSaveData saveData = MagicCubeSaveData.CreateSaveData(this);

        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        System.IO.FileStream file = System.IO.File.Create(MainGameLogic.SAVE_GAME_LOCATION);
        bf.Serialize(file, saveData);
        file.Close();

        Debug.Log("File written to: " + MainGameLogic.SAVE_GAME_LOCATION);
    }

    public bool IsLoading()
    {
        return bIsLoading;
    }
}

public abstract class CubeAction
{
    protected bool bStartAction = false;
    protected bool bActionIsDone = false;
    protected List<GameObject> cubeTileList;

    protected float timeToCompleteAction = 1.0f;
    protected float timePassed = 0.0f;

    protected float undoModifier = 3.0f;

    protected CubeAction undoAction;

    protected GameObject magicCube;
    protected MagicCubeManager manager;

    public CubeAction(GameObject magicCube, MagicCubeManager manager, List<GameObject> cubeTileList, bool bStartImmediately = false)
    {
        this.cubeTileList = cubeTileList;
        this.bStartAction = bStartImmediately;
        this.magicCube = magicCube;
        this.manager = manager;

        if (!bStartImmediately)
        {
            timeToCompleteAction /= undoModifier;
        }
        else
        {
            if (!manager.IsLoading())
            {
                StartAction();
            }
        }
    }

    public void SetTimeToComplete(float newTime)
    {
        timeToCompleteAction = newTime;
    }

    public Vector3Int QueryIndices()
    {
        return manager.QueryHelperCubeTileIndices(cubeTileList[0]);
    }


    public void CompleteAction()
    {
        bActionIsDone = true;
        manager.EndActionReinsertTile(cubeTileList);

    }

    public bool IsActionDone()
    {
        return bActionIsDone;
    }

    public CubeAction GetUndoAction()
    {
        return undoAction;
    }

    public void StartAction()
    {
        bStartAction = true;

        manager.StartActionRemoveTile(cubeTileList);
    }

    public abstract void Update();

    public void CompleteActionNoAnimation()
    {
        bActionIsDone = true;
        timePassed = timeToCompleteAction;
    }

    public List<GameObject> GetCubeTileListReference()
    {
        return cubeTileList;
    }
}

public class ActionRotateZXClockWise : CubeAction
{
    public ActionRotateZXClockWise(GameObject magicCube, MagicCubeManager manager, List<GameObject> cubeTileList, bool bStartImmediately = false) : base(magicCube, manager, cubeTileList, bStartImmediately)
    {
        if (bStartImmediately)
        {
            undoAction = new ActionRotateZXCounterClockWise(magicCube, manager, cubeTileList, false);
        }
    }

    public override void Update()
    {
        if (bStartAction && !bActionIsDone)
        {
            timePassed += Time.deltaTime;

            if (timePassed < timeToCompleteAction)
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                    new Vector3(magicCube.transform.position.x, cubeTileList[i].transform.position.y, magicCube.transform.position.z),
                    new Vector3(0, magicCube.transform.position.y, 0),
                                90 * Time.deltaTime / timeToCompleteAction);
                }
            }
            else
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                    new Vector3(magicCube.transform.position.x, cubeTileList[i].transform.position.y, magicCube.transform.position.z),
                    new Vector3(0, magicCube.transform.position.y, 0),
                                90 * (timeToCompleteAction - (timePassed - Time.deltaTime)) / timeToCompleteAction);
                }
            }

            if (timePassed >= timeToCompleteAction)
            {
                CompleteAction();
            }
        }
    }
}

public class ActionRotateZXCounterClockWise : CubeAction
{
    public ActionRotateZXCounterClockWise(GameObject magicCube, MagicCubeManager manager, List<GameObject> cubeTileList, bool bStartImmediately = false) : base(magicCube, manager, cubeTileList, bStartImmediately)
    {
        if (bStartImmediately)
        {
            undoAction = new ActionRotateZXClockWise(magicCube, manager, cubeTileList, false);
        }
    }

    public override void Update()
    {
        if (bStartAction && !bActionIsDone)
        {
            timePassed += Time.deltaTime;

            if (timePassed < timeToCompleteAction)
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                    new Vector3(magicCube.transform.position.x, cubeTileList[i].transform.position.y, magicCube.transform.position.z),
                    new Vector3(0, magicCube.transform.position.y, 0),
                                -90 * Time.deltaTime / timeToCompleteAction);
                }
            }
            else
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                    new Vector3(magicCube.transform.position.x, cubeTileList[i].transform.position.y, magicCube.transform.position.z),
                    new Vector3(0, magicCube.transform.position.y, 0),
                                -90 * (timeToCompleteAction - (timePassed - Time.deltaTime)) / timeToCompleteAction);
                }
            }

            if (timePassed >= timeToCompleteAction)
            {
                CompleteAction();
            }
        }
    }
}

public class ActionRotateZYClockWise : CubeAction
{
    public ActionRotateZYClockWise(GameObject magicCube, MagicCubeManager manager, List<GameObject> cubeTileList, bool bStartImmediately = false) : base(magicCube, manager, cubeTileList, bStartImmediately)
    {
        if (bStartImmediately)
        {
            undoAction = new ActionRotateZYCounterClockWise(magicCube, manager, cubeTileList, false);
        }
    }

    public override void Update()
    {
        if (bStartAction && !bActionIsDone)
        {
            timePassed += Time.deltaTime;

            if (timePassed < timeToCompleteAction)
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                            new Vector3(cubeTileList[i].transform.position.x, magicCube.transform.position.y, magicCube.transform.position.z),
                            new Vector3(magicCube.transform.position.x, 0, 0),
                                90 * Time.deltaTime / timeToCompleteAction);
                }
            }
            else
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                            new Vector3(cubeTileList[i].transform.position.x, magicCube.transform.position.y, magicCube.transform.position.z),
                            new Vector3(magicCube.transform.position.x, 0, 0),
                                90 * (timeToCompleteAction - (timePassed - Time.deltaTime)) / timeToCompleteAction);
                }
            }

            if (timePassed >= timeToCompleteAction)
            {
                CompleteAction();
            }
        }
    }
}

public class ActionRotateZYCounterClockWise : CubeAction
{
    public ActionRotateZYCounterClockWise(GameObject magicCube, MagicCubeManager manager, List<GameObject> cubeTileList, bool bStartImmediately = false) : base(magicCube, manager, cubeTileList, bStartImmediately)
    {
        if (bStartImmediately)
        {
            undoAction = new ActionRotateZYClockWise(magicCube, manager, cubeTileList, false);
        }
    }

    public override void Update()
    {
        if (bStartAction && !bActionIsDone)
        {
            timePassed += Time.deltaTime;

            if (timePassed < timeToCompleteAction)
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                            new Vector3(cubeTileList[i].transform.position.x, magicCube.transform.position.y, magicCube.transform.position.z),
                            new Vector3(magicCube.transform.position.x, 0, 0),
                                -90 * Time.deltaTime / timeToCompleteAction);
                }
            }
            else
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                            new Vector3(cubeTileList[i].transform.position.x, magicCube.transform.position.y, magicCube.transform.position.z),
                            new Vector3(magicCube.transform.position.x, 0, 0),
                                -90 * (timeToCompleteAction - (timePassed - Time.deltaTime)) / timeToCompleteAction);
                }
            }

            if (timePassed >= timeToCompleteAction)
            {
                CompleteAction();
            }
        }
    }
}

public class ActionRotateYXClockWise : CubeAction
{
    public ActionRotateYXClockWise(GameObject magicCube, MagicCubeManager manager, List<GameObject> cubeTileList, bool bStartImmediately = false) : base(magicCube, manager, cubeTileList, bStartImmediately)
    {
        if (bStartImmediately)
        {
            undoAction = new ActionRotateYXCounterClockWise(magicCube, manager, cubeTileList, false);
        }
    }

    public override void Update()
    {
        if (bStartAction && !bActionIsDone)
        {
            timePassed += Time.deltaTime;

            if (timePassed < timeToCompleteAction)
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                                new Vector3(magicCube.transform.position.x, magicCube.transform.position.y, cubeTileList[i].transform.position.z),
                                new Vector3(0, 0, magicCube.transform.position.z),
                                90 * Time.deltaTime / timeToCompleteAction);
                }
            }
            else
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                                new Vector3(magicCube.transform.position.x, magicCube.transform.position.y, cubeTileList[i].transform.position.z),
                                new Vector3(0, 0, magicCube.transform.position.z),
                                90 * (timeToCompleteAction - (timePassed - Time.deltaTime)) / timeToCompleteAction);
                }
            }

            if (timePassed >= timeToCompleteAction)
            {
                CompleteAction();
            }
        }
    }
}

public class ActionRotateYXCounterClockWise : CubeAction
{
    public ActionRotateYXCounterClockWise(GameObject magicCube, MagicCubeManager manager, List<GameObject> cubeTileList, bool bStartImmediately = false) : base(magicCube, manager, cubeTileList, bStartImmediately)
    {
        if (bStartImmediately)
        {
            undoAction = new ActionRotateYXClockWise(magicCube, manager, cubeTileList, false);
        }
    }

    public override void Update()
    {
        if (bStartAction && !bActionIsDone)
        {
            timePassed += Time.deltaTime;

            if (timePassed < timeToCompleteAction)
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                                new Vector3(magicCube.transform.position.x, magicCube.transform.position.y, cubeTileList[i].transform.position.z),
                                new Vector3(0, 0, magicCube.transform.position.z),
                                -90 * Time.deltaTime / timeToCompleteAction);
                }
            }
            else
            {
                for (int i = 0; i < cubeTileList.Count; i++)
                {
                    cubeTileList[i].transform.RotateAround(
                                new Vector3(magicCube.transform.position.x, magicCube.transform.position.y, cubeTileList[i].transform.position.z),
                                new Vector3(0, 0, magicCube.transform.position.z),
                                -90 * (timeToCompleteAction - (timePassed - Time.deltaTime)) / timeToCompleteAction);
                }
            }

            if (timePassed >= timeToCompleteAction)
            {
                CompleteAction();
            }
        }
    }
}