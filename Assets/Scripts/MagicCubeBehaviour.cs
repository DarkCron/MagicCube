using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCubeBehaviour : MonoBehaviour
{
    private MagicCubeManager manager;

    private GameObject startCubeTile;
    private Collider startFace = null;

    private Vector3 mouseDragStartPos;
    private Vector3 currentDragPos;
    private SwipeDragDirection currentDragDirection = SwipeDragDirection.NONE;
    private readonly float deltaDistanceToSwipe = 75;

    public enum SwipeDragDirection { UP, DOWN, LEFT, RIGHT, NONE}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (startCubeTile == null)
            {
                startCubeTile = manager.QueryFirstCubeTile();
                if (startCubeTile == null)
                {
                    if (Camera.main.GetComponent<CameraControl>() != null)
                    {
                        Camera.main.GetComponent<CameraControl>().InitRotation(transform.gameObject);
                    }
                }
            }
        }
    }

    public void Init(MagicCubeManager manager)
    {
        this.manager = manager;
    }

    private void OnMouseDrag()
    {
        Debug.Log(startFace);

        if (!manager.IsPerformingAction())
        {
            if (startCubeTile == null || startFace == null)
            {
                startCubeTile = manager.QueryFirstCubeTile();
                startFace = manager.QueryMagicCubeFace();

                if (startCubeTile != null && startFace != null)
                {
                    mouseDragStartPos = Input.mousePosition;
                }
            }

            if (startCubeTile != null && startFace != null)
            {
                currentDragPos = Input.mousePosition;
            }
        }
    }

    private void OnMouseUp()
    {
        if (startCubeTile != null && startFace != null)
        {
            Vector3 dragResult = -mouseDragStartPos + currentDragPos;
            if (Mathf.Abs(dragResult.x) > deltaDistanceToSwipe || Mathf.Abs(dragResult.y) > deltaDistanceToSwipe)
            {
                if (Mathf.Abs(dragResult.x) > Mathf.Abs(dragResult.y))
                {
                    if (dragResult.x > 0)
                    {
                        currentDragDirection = SwipeDragDirection.RIGHT;
                    }
                    else
                    {
                        currentDragDirection = SwipeDragDirection.LEFT;
                    }
                }
                else
                {
                    if (dragResult.y > 0)
                    {
                        currentDragDirection = SwipeDragDirection.UP;
                    }
                    else
                    {
                        currentDragDirection = SwipeDragDirection.DOWN;
                    }
                }
            }

            if (!manager.IsPerformingAction())
            {
                manager.QuerySliceRotation(startCubeTile, startFace, currentDragDirection);
            }
        }


        startCubeTile = null;
        startFace = null;
        mouseDragStartPos = Vector3.negativeInfinity;
        currentDragDirection = SwipeDragDirection.NONE;
    }
}
