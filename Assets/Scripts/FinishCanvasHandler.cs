using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishCanvasHandler : MonoBehaviour
{
    public Text timeText;

    public delegate void CloseFunction();
    CloseFunction closeFunction;

    public delegate void RestartFunction();
    RestartFunction restartFunction;

    public delegate void MainMenuFunction();
    MainMenuFunction mainMenuFunction;

    private bool bStartedFinishAnimation = false;
    private GameObject magicCube = null;
    private float cameraZoomTimer = 0.05f;
    private float cameraZoomTimePassed = 0.0f;
    private float degreesTurned = 0.0f;

    public void Init(CloseFunction cf, RestartFunction rf, MainMenuFunction mmf)
    {
        this.closeFunction = cf;
        this.restartFunction = rf;
        this.mainMenuFunction = mmf;
    }

    private void Reset()
    {
        magicCube = null;
        cameraZoomTimePassed = 0.0f;
        degreesTurned = 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (MainGameLogic.IsFinish())
        {
            if (bStartedFinishAnimation)
            {
                float degrees = Time.deltaTime * 40.0f;
                Camera.main.transform.RotateAround(magicCube.GetComponent<MagicCubeBehaviour>().getMagicCubeManager().GetPivot(), Camera.main.transform.up, degrees);
                degreesTurned += degrees;

                cameraZoomTimePassed += Time.deltaTime;
                if (cameraZoomTimePassed > cameraZoomTimer)
                {
                    cameraZoomTimePassed = 0.0f;
                    Camera.main.GetComponent<CameraControl>().ZoomOut(Time.deltaTime*2);
                }

                if (degreesTurned >= 360)
                {
                    FinishDialogue();
                }
            }
        }
    }

    public void FinishDialogue()
    {
        bStartedFinishAnimation = false;
        GetComponent<Canvas>().enabled = true;
        Camera.main.GetComponent<UIManager>().EnableMenuButton();
    }

    public void StartFinish(int time)
    {
        int minutes = time / 60;
        int seconds = time - minutes * 60;

        string minutesText = "";
        string secondsText = "";

        if (minutes < 10)
        {
            minutesText = "0" + minutes;
        }
        else
        {
            minutesText = minutes.ToString();
        }
        if (seconds < 10)
        {
            secondsText = "0" + seconds;
        }
        else
        {
            secondsText = seconds.ToString();
        }

        timeText.text = minutesText + ":" + secondsText;

        StartFinishAnim();
        Camera.main.GetComponent<UIManager>().OpenFinishCanvas();
        GetComponent<Canvas>().enabled = false;
        Camera.main.GetComponent<UIManager>().DisableMenuButton();

        //After finishing a game we delete the save file, no need for it anymore.
        //If we want to keep save games after finishing a game, I need to modify the save file
        //slightly to store whether the game has finished or not, to ensure equal states.
        MainGameLogic.DeleteSaveGame();
    }

    private void StartFinishAnim()
    {
        Reset();
        bStartedFinishAnimation = true;
        magicCube = GameObject.FindGameObjectWithTag("MagicCube");
        MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.FINISH_CANVAS);
    }

    public void RestartGameClick()
    {
        restartFunction();
    }

    public void MainMenuClick()
    {
        mainMenuFunction();
    }

    public void Close()
    {
        closeFunction();
    }
}
