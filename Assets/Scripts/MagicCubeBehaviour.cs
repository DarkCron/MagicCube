using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCubeBehaviour : MonoBehaviour
{
    MagicCubeManager manager;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("I LIVE!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(MagicCubeManager manager)
    {
        this.manager = manager;
    }

    private void OnMouseDrag()
    {
        Debug.Log("What a drag ;)");
    }
}
