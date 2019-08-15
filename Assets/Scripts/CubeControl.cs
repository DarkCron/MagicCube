using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeControl : MonoBehaviour
{
    public GameObject tileRef;
    private bool keyDown = false;
    private MagicCubeManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = new MagicCubeManager(tileRef, 8, new Vector3(0, 2, 0));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            keyDown = true;
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            keyDown = false;
        }

        manager.Update();

        if (keyDown)
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("CubeTile"))
            {
                obj.transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * 180);
            }
        }
    }

    private void OnMouseDrag()
    {
        
    }
}

public class MagicCubeManager
{
    //This game's magic cube size (cubed)
    private int magicCubeSize = 2;
    private GameObject cubeTileRef;
    private GameObject[] cubeTiles;
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

    public enum MagicCubeSide{ PosX, NegX, PosY, NegY, PosZ, NegZ, None}

    public MagicCubeManager(GameObject cubeTileRef, int n, Vector3 pivot)
    {
        ownHolder = new GameObject("Magic Cube");
        ownHolder.transform.position = Vector3.zero;
        ownHolder.layer = 10;
        MagicCubeBehaviour behaviour = ownHolder.AddComponent<MagicCubeBehaviour>();
        behaviour.Init(this);

        actionList = new List<CubeAction>();

        this.cubeTileRef = cubeTileRef;
        magicCubeSize = n;
        cubeTiles = new GameObject[magicCubeSize * magicCubeSize * magicCubeSize];
        mainPivot = pivot;

        collectionHelper = new CubeCollectionHelper(0);
        GenerateCubeTiles();
        Camera.main.transform.position = new Vector3(mainPivot.x,mainPivot.y,Camera.main.GetComponent<CameraControl>().GetCurrentZoom());
        Camera.main.transform.rotation = new Quaternion();
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

        /// <summary>
        /// Get all gameobjects from a ceretain row slice. Deep copy.
        /// </summary>
        /// <param name="index">indexing works from 0 - n-1, 0 being the lowest slice, n-1 the highest</param>
        /// <returns></returns>
        public List<GameObject> GetRowTiles(int index)
        {
            int count = 0;
            foreach (var slice in data_Y.GetData())
            {
                if (count == index)
                {
                    return new List<GameObject>(slice.Value);
                }
                count++;
            }

            throw new UnityException();
        }

        /// <summary>
        /// Get all gameobjects from a certain column slice. Deep copy.
        /// </summary>
        /// <param name="index">indexing works from 0 - n-1, 0 being the lowest slice, n-1 the highest</param>
        /// <returns></returns>
        public List<GameObject> GetColumnXTiles(int index)
        {
            int count = 0;
            foreach (var slice in data_X.GetData())
            {
                if (count == index)
                {
                    return new List<GameObject>(slice.Value);
                }
                count++;
            }

            throw new UnityException();
        }

        /// <summary>
        /// Get all gameobjects from a certain column slice. Deep copy.
        /// </summary>
        /// <param name="index">indexing works from 0 - n-1, 0 being the lowest slice, n-1 the highest</param>
        /// <returns></returns>
        public List<GameObject> GetColumnZTiles(int index)
        {
            int count = 0;
            foreach (var slice in data_Z.GetData())
            {
                if (count == index)
                {
                    return new List<GameObject>(slice.Value);
                }
                count++;
            }

            throw new UnityException();
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
                    var temp = GameObject.Instantiate(cubeTileRef,
                          cubeTileRef.transform.up * k * size.y * cubeTileRef.transform.localScale.y
                        + cubeTileRef.transform.forward * j * size.z * cubeTileRef.transform.localScale.z
                        + cubeTileRef.transform.right * i * size.x * cubeTileRef.transform.localScale.x
                        , cubeTileRef.transform.rotation);
                    collectionHelper.ProcessCubeTile(temp, temp.transform.position);
                    temp.transform.SetParent(ownHolder.transform);
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

    private CubeAction testAction;
    public void Update()
    {
        if (testAction != null)
        {
            if (!testAction.IsActionDone()) {
                testAction.Update();
            }
            else
            {
                testAction = null;
                //testAction.GetUndoAction().StartAction();
                //if (!testAction.GetUndoAction().IsActionDone())
                //{
                //    testAction.GetUndoAction().Update();
                //}
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (actionList.Count != 0)
            {
                Debug.Log("Trying Replay");
                CubeAction lastAction = actionList[actionList.Count-1];
                actionList.Remove(lastAction);
                lastAction.GetUndoAction().StartAction();
                testAction = lastAction.GetUndoAction();
            }
        }

        if (Input.GetMouseButtonDown(0) && false)
        {


            if (testAction != null)
            {

            }
            else
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                int layerMask = 1 << 10;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    //Debug.Log(hit.collider);
                    if (hit.collider.Equals(colliderPosY))
                    {
                        var collection = collectionHelper.GetRowTiles(2);
                        testAction = new ActionRotateZXClockWise(ownHolder, this, collection, true);
                        actionList.Add(testAction);
                    }
                    else if (hit.collider.Equals(colliderNegX))
                    {
                        var collection = collectionHelper.GetColumnXTiles(0);
                        testAction = new ActionRotateZYClockWise(ownHolder, this, collection, true);
                        actionList.Add(testAction);
                    }
                    else if (hit.collider.Equals(colliderNegZ))
                    {
                        var collection = collectionHelper.GetColumnZTiles(0);
                        testAction = new ActionRotateYXClockWise(ownHolder, this, collection, true);
                        actionList.Add(testAction);
                    }
                    else if (hit.collider.Equals(colliderPosX))
                    {
                        var collection = collectionHelper.GetColumnXTiles(2);
                        testAction = new ActionRotateZYClockWise(ownHolder, this, collection, true);
                        actionList.Add(testAction);
                    }
                }
            }
        }
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
                        testAction = new ActionRotateYXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        testAction = new ActionRotateYXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        Debug.Log("LEFT");
                        testAction = new ActionRotateZXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        Debug.Log("RIGHT");
                        testAction = new ActionRotateZXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            case MagicCubeSide.NegX:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        testAction = new ActionRotateYXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        testAction = new ActionRotateYXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        Debug.Log("LEFT");
                        testAction = new ActionRotateZXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        Debug.Log("RIGHT");
                        testAction = new ActionRotateZXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            case MagicCubeSide.PosY:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        testAction = new ActionRotateYXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        testAction = new ActionRotateYXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        Debug.Log("LEFT");
                        testAction = new ActionRotateZYClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        Debug.Log("RIGHT");
                        testAction = new ActionRotateZYCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            case MagicCubeSide.NegY:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        testAction = new ActionRotateYXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        testAction = new ActionRotateYXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnZTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        Debug.Log("LEFT");
                        testAction = new ActionRotateZYClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        Debug.Log("RIGHT");
                        testAction = new ActionRotateZYCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            case MagicCubeSide.PosZ:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        testAction = new ActionRotateZYClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        testAction = new ActionRotateZYCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        testAction = new ActionRotateZXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        testAction = new ActionRotateZXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            case MagicCubeSide.NegZ:
                switch (direction)
                {
                    case MagicCubeBehaviour.SwipeDragDirection.UP:
                        testAction = new ActionRotateZYClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.DOWN:
                        testAction = new ActionRotateZYCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetColumnXTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.LEFT:
                        testAction = new ActionRotateZXClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    case MagicCubeBehaviour.SwipeDragDirection.RIGHT:
                        testAction = new ActionRotateZXCounterClockWise(ownHolder, this, new List<GameObject>(collectionHelper.GetRowTiles(cubeTile)), true);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    public bool IsPerformingAction()
    {
        if (testAction != null)
        {
            return !testAction.IsActionDone();
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
        }else if (collider == colliderNegX)
        {
            return MagicCubeSide.NegX;
        }else if (collider == colliderPosY)
        {
            return MagicCubeSide.PosY;
        }
        else if (collider == colliderNegY)
        {
            return MagicCubeSide.NegY;
        }else if (collider == colliderPosZ)
        {
            return MagicCubeSide.PosZ;
        }
        else if (collider == colliderNegZ)
        {
            return MagicCubeSide.NegZ;
        }
        throw new UnityException();
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
            StartAction();
        }
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
            undoAction = new ActionRotateZXClockWise(magicCube,manager, cubeTileList, false);
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