﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MainGameLogic 
{
    public enum CurrentActiveElement { MAIN_MENU, MAIN_GAME, GAME_MENU, CONFIRMATION_DIALOGUE, FINISH_CANVAS};
    private static CurrentActiveElement currentActiveElement = MainGameLogic.CurrentActiveElement.MAIN_MENU;

    static public string SAVE_GAME_LOCATION = Application.persistentDataPath + "/magicCubeSaveData.save";

    public static MagicCubeSaveData LoadGameSave()
    {
        if (System.IO.File.Exists(SAVE_GAME_LOCATION))
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.FileStream file = System.IO.File.Open(SAVE_GAME_LOCATION, System.IO.FileMode.Open);
            MagicCubeSaveData savedata = (MagicCubeSaveData)bf.Deserialize(file);
            file.Close();
            return savedata;
        }
        throw new UnityException();
    }

    public static void DeleteSaveGame()
    {
        if (System.IO.File.Exists(SAVE_GAME_LOCATION))
        {
            System.IO.File.Delete(SAVE_GAME_LOCATION);
        }
    }

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

    public static bool IsFinish()
    {
        return currentActiveElement == CurrentActiveElement.FINISH_CANVAS;
    }

    public static void LinkMagicCubeManagerAndUI(MagicCubeManager manager, MainGameUI mainGameUI)
    {
        manager.LinkGameTimer(mainGameUI);
        manager.LinkUndoRedo(mainGameUI);
        manager.LinkProcessUndoRedoPossible(mainGameUI);
        manager.LinkProcessOpenMenu(mainGameUI);
    }

    internal static void LinkMagicCubeManagerAndFinish(MagicCubeManager manager, FinishCanvasHandler finishCanvasHandler)
    {
        manager.LinkProcessFinishGame(finishCanvasHandler);
    }
}
