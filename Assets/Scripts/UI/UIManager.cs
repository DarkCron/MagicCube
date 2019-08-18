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
    public GameObject finishCanvas;

    // Start is called before the first frame update
    void Start()
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

    public FinishCanvasHandler GetFinishCanvas()
    {
        if (finishCanvas == null)
        {
            throw new UnityException();
        }
        if (finishCanvas.GetComponent<FinishCanvasHandler>() == null)
        {
            throw new UnityException();
        }
        return finishCanvas.GetComponent<FinishCanvasHandler>();
    }

    public void MainMenuToGame()
    {
        mainMenuCanvas.SetActive(false);
        mainGameUI.SetActive(true);
    }


    internal void OpenGameMenu()
    {
        if (MainGameLogic.GetMainCamera().GetComponent<CubeControl>().GetMagicCubeManager().IsCurrentActionDone())
        {
            gameMenuCanvas.GetComponent<GameMenuBehaviour>().Init(CloseGameMenu, ShowHideGameTimer, RestartGame, MainMenuGame);
            gameMenuCanvas.SetActive(true);
            MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.GAME_MENU);
        }
    }

    internal void OpenFinishCanvas()
    {
        if (MainGameLogic.GetMainCamera().GetComponent<CubeControl>().GetMagicCubeManager().IsCurrentActionDone())
        {
            finishCanvas.GetComponent<FinishCanvasHandler>().Init(CloseFinishCanvas,FinishRestart,FinishMainMenu);
            finishCanvas.SetActive(true);
            MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.FINISH_CANVAS);
        }
    }

    #region FINISH CANVAS functions
    /// <summary>
    /// Finish Canvas functionalities
    /// </summary>
    public void CloseFinishCanvas()
    {
        finishCanvas.SetActive(false);
        MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.MAIN_GAME);
    }

    public void FinishMainMenu()
    {
        confirmationCanvas.GetComponent<ConfirmationHandler>().SetMessage("Main Menu?");
        confirmationCanvas.GetComponent<ConfirmationHandler>().InitConfirmationCanvas(FinishMainMenuYes, FinishMainMenuNo, FinishMainMenuClose);
        confirmationCanvas.SetActive(true);
    }

    public void FinishRestart()
    {
        confirmationCanvas.GetComponent<ConfirmationHandler>().SetMessage("Restart Game?");
        confirmationCanvas.GetComponent<ConfirmationHandler>().InitConfirmationCanvas(FinishRestartYes, FinishRestartNo, FinishRestartClose);
        confirmationCanvas.SetActive(true);
    }

    internal void DisableMenuButton()
    {
        mainGameUI.GetComponent<MainGameUI>().DisableMenuButton();
    }

    internal void EnableMenuButton()
    {
        mainGameUI.GetComponent<MainGameUI>().EnableMenuButton();
    }

    /// <summary>
    /// Finish menu RESTART functionalities
    /// </summary>
    /// YES BUTTON
    public void FinishRestartYes()
    {
        MainGameLogic.GetMainCamera().GetComponent<MainMenuHandler>().StartGame();
        finishCanvas.SetActive(false);
        confirmationCanvas.SetActive(false);
        MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.MAIN_GAME);
    }

    public void FinishRestartNo()
    {
        confirmationCanvas.SetActive(false);
    }

    public void FinishRestartClose()
    {
        confirmationCanvas.SetActive(false);
    }

    /// <summary>
    /// Finish MAIN MENU functionalities
    /// </summary>
    /// YES BUTTON
    public void FinishMainMenuYes()
    {
        finishCanvas.SetActive(false);
        confirmationCanvas.SetActive(false);
        gameMenuCanvas.SetActive(false);
        MainGameLogic.GetMainCamera().GetComponent<MainMenuHandler>().Reset();
        mainMenuCanvas.SetActive(true);
        mainGameUI.SetActive(false);
        MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.MAIN_MENU);
    }

    public void FinishMainMenuNo()
    {
        confirmationCanvas.SetActive(false);
    }

    public void FinishMainMenuClose()
    {
        confirmationCanvas.SetActive(false);
    }
    #endregion

    #region Game Menu Stuff
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
        MainGameLogic.GetMainCamera().GetComponent<MainMenuHandler>().StartGame();
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
        MainGameLogic.GetMainCamera().GetComponent<CubeControl>().GetMagicCubeManager().Save();
        MainGameLogic.GetMainCamera().GetComponent<MainMenuHandler>().Reset();
        mainMenuCanvas.SetActive(true);
        mainGameUI.SetActive(false);
        MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.MAIN_MENU);
        
    }

    public void MainMenuNo()
    {
        confirmationCanvas.SetActive(false);
    }

    public void MainMenuClose()
    {
        confirmationCanvas.SetActive(false);
    }
    #endregion
}
