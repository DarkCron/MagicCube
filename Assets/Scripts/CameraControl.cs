using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public AnimationCurve easeInCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float easeInDistance = 600.0f;
    private float distanceMoved = 0.0f;

    public float minZoom = -4.0f;
    public float maxZoom = -13.0f;
    public float zoomSpeed = 20.0f;
    private float currentZoom = -8.0f;

    public float cameraSpeed = 90.0f;

    private GameObject rotationTarget;
    private bool bStartedRotation = false;
    private bool bLockedAxis = false;
    private bool bRotateX = false;
    private bool bRotateY = false;

    private Vector3 startMousePos;
    private Vector3 currentMousePos;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!MainGameLogic.IsMainGame())
        {
            return;
        }

        if (bStartedRotation)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Reset();
            }
        }
        if (bStartedRotation)
        {

            Vector3 difference = startMousePos - Input.mousePosition;
            difference = new Vector3(-difference.y, difference.x, 0);
            distanceMoved += Vector3.Distance(currentMousePos, Input.mousePosition);
            distanceMoved = (startMousePos.x - Input.mousePosition.x) + (startMousePos.y - Input.mousePosition.y);
            if (distanceMoved > easeInDistance)
            {
                distanceMoved = easeInDistance;
            }
            else if(distanceMoved < - easeInDistance)
            {
                distanceMoved = -easeInDistance;
            }
            //Debug.Log(distanceMoved);
            
            currentMousePos = Input.mousePosition;

            float delta = 0.0f;
            if (Math.Abs(distanceMoved) < easeInDistance)
            {
                delta = Math.Abs(distanceMoved) / easeInDistance;
                delta = easeInCurve.Evaluate(delta);
            }
            else
            {
                delta = 1.0f;
            }
            float actualCameraMoveSpeed = delta * cameraSpeed;

            if (!bLockedAxis)
            {
                if (Math.Abs(difference.x) > easeInDistance / 4)
                {
                    bRotateX = true;
                    bLockedAxis = true;

                }
                if (Math.Abs(difference.y) > easeInDistance / 4)
                {
                    bRotateY = true;
                    bLockedAxis = true;
                }
            }

            if (bRotateX)
            {
                if (difference.x > 0)
                {
                    transform.RotateAround(rotationTarget.transform.position, -transform.right, Time.deltaTime * actualCameraMoveSpeed);
                }
                else
                {
                    transform.RotateAround(rotationTarget.transform.position, transform.right, Time.deltaTime * actualCameraMoveSpeed);
                }
            }else if (bRotateY)
            {
                if (difference.y > 0)
                {
                    transform.RotateAround(rotationTarget.transform.position, -transform.up, Time.deltaTime * actualCameraMoveSpeed);
                }
                else
                {
                    transform.RotateAround(rotationTarget.transform.position, transform.up, Time.deltaTime * actualCameraMoveSpeed);
                }
            }
        }
        else
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                // scroll up
                ZoomIn();
            }
            else if (scroll < 0f)
            {
                // scroll down
                ZoomOut();
            }
        }
    }

    public void ZoomIn()
    {
        if (currentZoom < minZoom)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z) + transform.forward * Time.deltaTime * zoomSpeed;
            currentZoom += Time.deltaTime * zoomSpeed;
        }
    }

    public void ZoomOut()
    {
        if (currentZoom > maxZoom)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z) - transform.forward * Time.deltaTime * zoomSpeed;
            currentZoom -= Time.deltaTime * zoomSpeed;
        }
    }

    private void Reset()
    {
        bStartedRotation = false;
        distanceMoved = 0.0f;
        bLockedAxis = false;
        bRotateX = false;
        bRotateY = false;
    }

    public void InitRotation(GameObject rotationTarget)
    {
        this.rotationTarget = rotationTarget;
        bStartedRotation = true;

        startMousePos = Input.mousePosition;
        currentMousePos = Input.mousePosition;
    }

    public float GetMinZoom()
    {
        return minZoom;
    }

    public float GetMaxZoom()
    {
        return maxZoom;
    }

    public float GetCurrentZoom()
    {
        return currentZoom;
    }
}
