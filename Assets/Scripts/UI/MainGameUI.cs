using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameUI : MonoBehaviour
{
    public Color actionAvailableColor;
    public Color actionUnavailableColor;
    public Text timerText;
    public Button menuButton;

    //public GameObject redoAction;
    public GameObject undoAction;

    private MagicCubeManager magicCubeManager;
    private int timePassed = 0;

    public delegate void UndoAction();
    //public delegate void RedoAction();
    private UndoAction undo;
    //private RedoAction redo;

    public delegate void MenuAction();
    private MenuAction menu;

    private bool bIsTimerVisible = true;

    public void Init(MagicCubeManager magicCubeManager)
    {
        this.magicCubeManager = magicCubeManager;
        timerText.text = "00:00";
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void ToggleTimerVisibility()
    {
        bIsTimerVisible = !bIsTimerVisible;
        timerText.gameObject.SetActive(bIsTimerVisible);
    }

    public void SetTimer(int timePassed)
    {
        this.timePassed = timePassed;
        ProcessTimeToText(this.timePassed);
    }

    private void ProcessTimeToText(int time)
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

        timerText.text = minutesText + ":" + secondsText;
    }

    public void SetUndoRedoActions(UndoAction undo)//, RedoAction redo)
    {
        this.undo = undo;
        //this.redo = redo;
        timerText.text = "00:00";
        //redoAction.GetComponent<Button>().image.color = actionUnavailableColor;
        undoAction.GetComponent<Button>().image.color = actionUnavailableColor;
        //redoAction.GetComponent<Button>().interactable = false;
        undoAction.GetComponent<Button>().interactable = false;
        timerText.gameObject.SetActive(true);
    }

    public void SetMenuAction(MenuAction menu)
    {
        this.menu = menu;
    }

    public void ProcessUndoRedoList(bool bCanUndo)//, bool bCanRedo)
    {
        //redoAction.GetComponent<Button>().interactable = bCanRedo;
        undoAction.GetComponent<Button>().interactable = bCanUndo;

        //if (redoAction.GetComponent<Button>().interactable)
        //{
        //    redoAction.GetComponent<Button>().image.color = actionAvailableColor;
        //}
        //else
        //{
        //    redoAction.GetComponent<Button>().image.color = actionUnavailableColor;
        //}
        if (undoAction.GetComponent<Button>().interactable)
        {
            undoAction.GetComponent<Button>().image.color = actionAvailableColor;
        }
        else
        {
            undoAction.GetComponent<Button>().image.color = actionUnavailableColor;
        }
    }

    internal void EnableMenuButton()
    {
        menuButton.interactable = true;
    }

    internal void DisableMenuButton()
    {
        menuButton.interactable = false;
    }

    public void UndoActionExecute()
    {
        undo();
    }

    //public void RedoActionExecute()
    //{
    //    redo();
    //}

    public void MenuActionExecute()
    {
        menu();
    }
}
