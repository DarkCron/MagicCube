using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MainGameLogic 
{
    public enum CurrentActiveElement { MAIN_MENU, MAIN_GAME, GAME_MENU, CONFIRMATION_DIALOGUE};
    private static CurrentActiveElement currentActiveElement = MainGameLogic.CurrentActiveElement.MAIN_MENU;

    public static void SetCurrentActiveElement(CurrentActiveElement element)
    {
        currentActiveElement = element;
    }

    public static bool IsMainMenu()
    {
        return currentActiveElement == CurrentActiveElement.MAIN_MENU;
    }

    public static bool IsMainGame()
    {
        return currentActiveElement == CurrentActiveElement.MAIN_GAME;
    }

    public static bool IsGameMenu()
    {
        return currentActiveElement == CurrentActiveElement.MAIN_MENU;
    }

    public static bool IsConfirmation()
    {
        return currentActiveElement == CurrentActiveElement.CONFIRMATION_DIALOGUE;
    }

    public static void LinkMagicCubeManagerAndUI(MagicCubeManager manager, MainGameUI mainGameUI)
    {
        manager.LinkGameTimer(mainGameUI);
        manager.LinkUndoRedo(mainGameUI);
        manager.LinkProcessUndoRedoPossible(mainGameUI);
        manager.LinkProcessOpenMenu(mainGameUI);
    }
}
