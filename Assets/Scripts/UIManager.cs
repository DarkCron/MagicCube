using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject confirmationCanvas;
    public GameObject mainGameUI;
    public GameObject gameMenuCanvas;
    public GameObject mainMenuCanvas;

    private GameObject currentTopCanvasLayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public MainGameUI GetMainGameUI()
    {
        if (mainGameUI == null)
        {
            throw new UnityException();
        }
        if (mainGameUI.GetComponent<MainGameUI>() == null)
        {
            throw new UnityException();
        }
        return mainGameUI.GetComponent<MainGameUI>();
    }

    public void MainMenuToGame()
    {
        mainMenuCanvas.SetActive(false);
        mainGameUI.SetActive(true);
    }


    internal void OpenGameMenu()
    {
        if (Camera.main.GetComponent<CubeControl>().GetMagicCubeManager().IsCurrentActionDone())
        {
            gameMenuCanvas.GetComponent<GameMenuBehaviour>().Init(CloseGameMenu, ShowHideGameTimer, RestartGame, MainMenuGame);
            gameMenuCanvas.SetActive(true);
            MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.GAME_MENU);
        }
    }

    /// <summary>
    /// Game menu functionalities
    /// </summary>
    public void CloseGameMenu()
    {
        gameMenuCanvas.SetActive(false);
        MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.MAIN_GAME);
    }

    public void ShowHideGameTimer()
    {
        mainGameUI.GetComponent<MainGameUI>().ToggleTimerVisibility();
    }

    public void MainMenuGame()
    {
        confirmationCanvas.GetComponent<ConfirmationHandler>().SetMessage("Main Menu?");
        confirmationCanvas.GetComponent<ConfirmationHandler>().InitConfirmationCanvas(MainMenuYes, MainMenuNo, MainMenuClose);
        confirmationCanvas.SetActive(true);
    }

    public void RestartGame()
    {
        confirmationCanvas.GetComponent<ConfirmationHandler>().SetMessage("Restart Game?");
        confirmationCanvas.GetComponent<ConfirmationHandler>().InitConfirmationCanvas(RestartYes, RestartNo, RestartClose);
        confirmationCanvas.SetActive(true);
    }

    /// <summary>
    /// Game menu RESTART functionalities
    /// </summary>
    /// YES BUTTON
    public void RestartYes()
    {
        Camera.main.GetComponent<MainMenuHandler>().StartGame();
        gameMenuCanvas.SetActive(false);
        confirmationCanvas.SetActive(false);
        MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.MAIN_GAME);
    }

    public void RestartNo()
    {
        confirmationCanvas.SetActive(false);
    }

    public void RestartClose()
    {
        confirmationCanvas.SetActive(false);
    }

    /// <summary>
    /// Game menu MAIN MENU functionalities
    /// </summary>
    /// YES BUTTON
    public void MainMenuYes()
    {
        confirmationCanvas.SetActive(false);
        gameMenuCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
        mainGameUI.SetActive(false);
        MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.MAIN_MENU);
        //TODO: save progress
    }

    public void MainMenuNo()
    {
        confirmationCanvas.SetActive(false);
    }

    public void MainMenuClose()
    {
        confirmationCanvas.SetActive(false);
    }
}
