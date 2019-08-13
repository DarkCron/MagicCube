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
        manager = new MagicCubeManager(tileRef,3, new Vector3(0,2,0));
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

        if (keyDown)
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("CubeTile"))
            {
                obj.transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * 180);
            }
        }
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

    private MagicCubeRow[] cubeRows;
    private MagicCubeColumnX[] cubeColumns_X;
    private MagicCubeColumnZ[] cubeColumns_Z;

    private GameObject ownHolder;

    private BoxCollider colliderPosX;
    private BoxCollider colliderNegX;
    private BoxCollider colliderPosY;
    private BoxCollider colliderNegY;
    private BoxCollider colliderPosZ;
    private BoxCollider colliderNegZ;

    public MagicCubeManager(GameObject cubeTileRef, int n, Vector3 pivot)
    {
        ownHolder = new GameObject("Magic Cube");
        ownHolder.transform.position = Vector3.zero;

        this.cubeTileRef = cubeTileRef;
        magicCubeSize = n;
        cubeTiles = new GameObject[magicCubeSize * magicCubeSize * magicCubeSize];
        mainPivot = pivot;

        collectionHelper = new CubeCollectionHelper(0);
        GenerateCubeTiles();

        cubeRows = new MagicCubeRow[magicCubeSize];
        cubeColumns_X = new MagicCubeColumnX[magicCubeSize];
        cubeColumns_Z = new MagicCubeColumnZ[magicCubeSize];

        GenerateCubeTileCollections();
    }

    private void GenerateCubeTileCollections()
    {
        collectionHelper.generateData(this);
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

        public void ProcessCubeTile(GameObject tile, Vector3 position)
        {
            data_X.ProcessCubeTile(tile, position);
            data_Y.ProcessCubeTile(tile, position);
            data_Z.ProcessCubeTile(tile, position);
        }

        public void generateData(MagicCubeManager manager)
        {
            List<MagicCubeRow> rows = new List<MagicCubeRow>();
            foreach (var entry in data_Y.GetData())
            {
                rows.Add(new MagicCubeRow(manager,entry.Value));
            }
        }

        public void Report()
        {
            data_X.Report();
            data_Y.Report();
            data_Z.Report();
        }

        struct SameCoordinateHelperX
        {
            Dictionary<float,List<GameObject>> data;

            public SameCoordinateHelperX(int mock)
            {
                data = new Dictionary<float, List<GameObject>>();
            }

            public void ProcessCubeTile(GameObject tile, Vector3 position)
            {
                if (data.ContainsKey(position.x))
                {
                    data[position.x].Add(tile);
                }
                else
                {
                    data.Add(position.x, new List<GameObject> { tile });
                }
            }

            public Dictionary<float, List<GameObject>> GetData()
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
        }
        struct SameCoordinateHelperY
        {
            Dictionary<float, List<GameObject>> data;

            public SameCoordinateHelperY(int mock)
            {
                data = new Dictionary<float, List<GameObject>>();
            }

            public void ProcessCubeTile(GameObject tile, Vector3 position)
            {
                if (data.ContainsKey(position.y))
                {
                    data[position.y].Add(tile);
                }
                else
                {
                    data.Add(position.y, new List<GameObject> { tile });
                }
            }

            public Dictionary<float, List<GameObject>> GetData()
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
        }
        struct SameCoordinateHelperZ
        {
            Dictionary<float, List<GameObject>> data;

            public SameCoordinateHelperZ(int mock)
            {
                data = new Dictionary<float, List<GameObject>>();
            }

            public void ProcessCubeTile(GameObject tile, Vector3 position)
            {
                if (data.ContainsKey(position.z))
                {
                    data[position.z].Add(tile);
                }
                else
                {
                    data.Add(position.z, new List<GameObject> { tile });
                }
            }

            public Dictionary<float, List<GameObject>> GetData()
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
        }
    }

    private void GenerateCubeTiles()
    {
        Vector3 size = cubeTileRef.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        mainPivot = size * cubeTileRef.transform.localScale.z * (magicCubeSize-1) / 2;
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
        colliderPosZ.center = new Vector3(0,0,(size * cubeTileRef.transform.localScale.z * (magicCubeSize) / 2).z);
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
        colliderNegX.center = new Vector3( -(size * cubeTileRef.transform.localScale.x * (magicCubeSize) / 2).x, 0, 0);
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
}

/// <summary>
/// Helper data management class for the magic cube.
/// We will manage the magic nxnxn cube in "rows" and "columns"
///  - a "ROW" contains (in Unity) all cubeTiles on the same ZX plane (Y constant).
///  - a "COLUMN_X" contains (in Unity) all cubeTile on the same ZY plane (X constant)
///  - a "COLUMN_Z" contains (in Unity) all cubeTile on the same YX plane (Z constant)
/// </summary>
public abstract class MagicCubeCollection
{
    //Contains references to the maintained cubes by this data structure
    private GameObject[] managedCubes;

    //A weighted center based on all managed cubes, to make the rotation possible.
    private Vector3 pivot;

    private MagicCubeManager manager;

    public MagicCubeCollection(MagicCubeManager manager, List<GameObject> items)
    {
        this.manager = manager;
        managedCubes = items.ToArray();
        if (managedCubes.Length != manager.GetMagicCubeSize() * manager.GetMagicCubeSize())
        {
            throw new UnityException();
        }
        else
        {
            Debug.Log("NICE!");
        }
       // managedCubes = new GameObject[manager.GetMagicCubeSize() * manager.GetMagicCubeSize()];
    }

    public void AddCubeTile(GameObject gameObject)
    {
        for (int i = 0; i < managedCubes.Length; i++)
        {
            if (managedCubes[i] == null)
            {
                managedCubes[i] = gameObject;
                break;
            }
        }
    }

    public void ReplaceCubeTile(GameObject other, GameObject fromThisCollection)
    {
        for (int i = 0; i < managedCubes.Length; i++)
        {
            if (managedCubes[i] == fromThisCollection)
            {
                managedCubes[i] = other;
                break;
            }
        }
    }
}

public class MagicCubeRow : MagicCubeCollection
{
    public MagicCubeRow(MagicCubeManager manager, List<GameObject> items) : base(manager, items)
    {
    }
}

public class MagicCubeColumnX : MagicCubeCollection
{
    public MagicCubeColumnX(MagicCubeManager manager, List<GameObject> items) : base(manager, items)
    {
    }
}

public class MagicCubeColumnZ : MagicCubeCollection
{
    public MagicCubeColumnZ(MagicCubeManager manager, List<GameObject> items) : base(manager, items)
    {
    }
}
