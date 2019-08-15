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
    private readonly float deltaDistanceToSwipe = 0.2f;

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
                startCubeTile = null;
            }
        }
    }

    public void Init(MagicCubeManager manager)
    {
        this.manager = manager;
    }

    private void OnMouseDrag()
    {
        if (!manager.IsPerformingAction())
        {
            if (startCubeTile == null || startFace == null)
            {
                startCubeTile = manager.QueryFirstCubeTile();
                startFace = manager.QueryMagicCubeFace();

                if (startCubeTile != null && startFace != null)
                {
                    Vector3 temp = manager.GetWorldPos();
                    if (temp != Vector3.positiveInfinity)
                    {
                        mouseDragStartPos = temp;
                    }
                }
            }

            if (startCubeTile != null && startFace != null)
            {
                Vector3 temp = manager.GetWorldPos();
                if (!float.IsInfinity(temp.x))
                {
                    currentDragPos = temp;
                }
            }
        }
    }

    private void OnMouseUp()
    {
        if (startCubeTile != null && startFace != null && !float.IsInfinity(currentDragPos.x))
        {
            //Debug.Log(mouseDragStartPos + "             " + currentDragPos);
            Vector3 dragResult = -mouseDragStartPos + currentDragPos;
            if (Mathf.Abs(dragResult.x) > deltaDistanceToSwipe || Mathf.Abs(dragResult.y) > deltaDistanceToSwipe || Mathf.Abs(dragResult.z) > deltaDistanceToSwipe)
            {
                switch (manager.GetWhichSide(startFace))
                {
                    case MagicCubeManager.MagicCubeSide.PosX:
                        if (Mathf.Abs(dragResult.z) > Mathf.Abs(dragResult.y))
                        {
                            if (dragResult.z > 0)
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
                        break;
                    case MagicCubeManager.MagicCubeSide.NegX:
                        if (Mathf.Abs(dragResult.z) > Mathf.Abs(dragResult.y))
                        {
                            if (dragResult.z > 0)
                            {
                                currentDragDirection = SwipeDragDirection.LEFT;
                            }
                            else
                            {
                                currentDragDirection = SwipeDragDirection.RIGHT;
                            }
                        }
                        else
                        {
                            if (dragResult.y > 0)
                            {
                                currentDragDirection = SwipeDragDirection.DOWN;
                            }
                            else
                            {
                                currentDragDirection = SwipeDragDirection.UP;
                            }
                        }
                        break;
                    case MagicCubeManager.MagicCubeSide.PosY:
                        if (Mathf.Abs(dragResult.z) > Mathf.Abs(dragResult.x))
                        {
                            if (dragResult.z > 0)
                            {
                                currentDragDirection = SwipeDragDirection.LEFT;
                            }
                            else
                            {
                                currentDragDirection = SwipeDragDirection.RIGHT;
                            }
                        }
                        else
                        {
                            if (dragResult.x > 0)
                            {
                                currentDragDirection = SwipeDragDirection.DOWN;
                            }
                            else
                            {
                                currentDragDirection = SwipeDragDirection.UP;
                            }
                        }
                        break;
                    case MagicCubeManager.MagicCubeSide.NegY:
                        if (Mathf.Abs(dragResult.z) > Mathf.Abs(dragResult.x))
                        {
                            if (dragResult.z > 0)
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
                            if (dragResult.x > 0)
                            {
                                currentDragDirection = SwipeDragDirection.UP;
                            }
                            else
                            {
                                currentDragDirection = SwipeDragDirection.DOWN;
                            }
                        }
                        break;
                    case MagicCubeManager.MagicCubeSide.PosZ:
                        if (Mathf.Abs(dragResult.x) > Mathf.Abs(dragResult.y))
                        {
                            if (dragResult.x > 0)
                            {
                                currentDragDirection = SwipeDragDirection.LEFT;
                            }
                            else
                            {
                                currentDragDirection = SwipeDragDirection.RIGHT;
                            }
                        }
                        else
                        {
                            if (dragResult.y > 0)
                            {
                                currentDragDirection = SwipeDragDirection.DOWN;
                            }
                            else
                            {
                                currentDragDirection = SwipeDragDirection.UP;
                            }
                        }
                        break;
                    case MagicCubeManager.MagicCubeSide.NegZ:
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
                        break;
                    default:
                        break;
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
        currentDragPos = Vector3.positiveInfinity;
        currentDragDirection = SwipeDragDirection.NONE;
    }
}
