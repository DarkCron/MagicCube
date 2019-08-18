using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenuBehaviour : MonoBehaviour
{
    public delegate void CloseFunction();
    CloseFunction closeFunction;

    public delegate void ShowHideTimer();
    ShowHideTimer showHideTimer;

    public delegate void RestartFunction();
    RestartFunction restartFunction;

    public delegate void MainMenuFunction();
    MainMenuFunction mainMenuFunction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(CloseFunction cf, ShowHideTimer shtf, RestartFunction rf, MainMenuFunction mmf)
    {
        closeFunction = cf;
        showHideTimer = shtf;
        restartFunction = rf;
        mainMenuFunction = mmf;
    }

    public void RestartGameClick()
    {
        restartFunction();
    }

    public void MainMenuClick()
    {
        mainMenuFunction();
    }

    public void ShowHideTimerclick()
    {
        showHideTimer();
    }

    public void Close()
    {
        closeFunction();
    }
}
