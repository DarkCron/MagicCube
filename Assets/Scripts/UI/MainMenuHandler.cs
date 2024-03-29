﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    public Color availableColor;
    public Color notAvailableColor;
    public Button continueButton;
    public Text continueText;
    public Text sliderText;
    public Slider cubeSizeSlider;
    public Canvas mainMenuCanvas;

    private int cubeSize = 0;

    private bool bContinueGameIsAvailable = false;
    private GameObject magicCube;

    // Start is called before the first frame update
    void Start()
    {
        bContinueGameIsAvailable = CheckForLoadGameAvailable();
        if (!bContinueGameIsAvailable)
        {
            continueText.color = notAvailableColor;
        }
        else
        {
            continueText.color = availableColor;
        }
        continueButton.interactable = bContinueGameIsAvailable;
        ChangeValue(cubeSizeSlider.value);

        MainGameLogic.StartGame();
    }

    public void Reset()
    {
        bContinueGameIsAvailable = CheckForLoadGameAvailable();
        if (!bContinueGameIsAvailable)
        {
            continueText.color = notAvailableColor;
        }
        else
        {
            continueText.color = availableColor;
        }
        continueButton.interactable = bContinueGameIsAvailable;
        ChangeValue(cubeSizeSlider.value);
    }

    private bool CheckForLoadGameAvailable()
    {
        return System.IO.File.Exists(MainGameLogic.SAVE_GAME_LOCATION);
    }

    // Update is called once per frame
    void Update()
    {
        if (!MainGameLogic.IsMainMenu())
        {
            return;
        }

        if (magicCube == null && GameObject.FindGameObjectWithTag("MagicCube") != null)
        {
            magicCube = GameObject.FindGameObjectWithTag("MagicCube");
        }

        if (magicCube != null)
        {
            if (magicCube.GetComponent<MagicCubeBehaviour>().getMagicCubeManager() != null)
            {
                MainGameLogic.GetMainCamera().transform.RotateAround(magicCube.GetComponent<MagicCubeBehaviour>().getMagicCubeManager().GetPivot(), new Vector3(0, 1, 1), Time.deltaTime * 30.0f);
            }
        }
    }

    public void ChangeValue(float value)
    {
        sliderText.text = Mathf.RoundToInt(value).ToString();
        if (cubeSize != Mathf.RoundToInt(value))
        {
            MainGameLogic.GetMainCamera().GetComponent<CubeControl>().CreateMagicCube(Mathf.RoundToInt(cubeSizeSlider.value));
        }
        cubeSize = Mathf.RoundToInt(value);
    }

    public void StartGame()
    {
        MainGameLogic.GetMainCamera().GetComponent<CubeControl>().CreateMagicCube(Mathf.RoundToInt(cubeSizeSlider.value));
        MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.MAIN_GAME);
        MainGameLogic.LinkMagicCubeManagerAndUI(MainGameLogic.GetMainCamera().GetComponent<CubeControl>().GetMagicCubeManager(), MainGameLogic.GetMainCamera().GetComponent<UIManager>().GetMainGameUI());
        MainGameLogic.LinkMagicCubeManagerAndFinish(MainGameLogic.GetMainCamera().GetComponent<CubeControl>().GetMagicCubeManager(),MainGameLogic.GetMainCamera().GetComponent<UIManager>().GetFinishCanvas());

        MainGameLogic.GetMainCamera().GetComponent<UIManager>().MainMenuToGame();
        MainGameLogic.GetMainCamera().GetComponent<CubeControl>().GetMagicCubeManager().InitRandomMoves(2.7f);
    }

    public void LoadGame()
    {
        try
        {
            MagicCubeSaveData savedata = MainGameLogic.LoadGameSave();

            MainGameLogic.GetMainCamera().GetComponent<CubeControl>().CreateMagicCube(savedata.magicCubeSize);
            MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.MAIN_GAME);
            MainGameLogic.LinkMagicCubeManagerAndUI(MainGameLogic.GetMainCamera().GetComponent<CubeControl>().GetMagicCubeManager(), MainGameLogic.GetMainCamera().GetComponent<UIManager>().GetMainGameUI());
            MainGameLogic.LinkMagicCubeManagerAndFinish(MainGameLogic.GetMainCamera().GetComponent<CubeControl>().GetMagicCubeManager(), MainGameLogic.GetMainCamera().GetComponent<UIManager>().GetFinishCanvas());
            MainGameLogic.GetMainCamera().GetComponent<CubeControl>().GetMagicCubeManager().LoadGame(savedata);

            MainGameLogic.GetMainCamera().GetComponent<UIManager>().MainMenuToGame();
        }
        catch (Exception e)
        {
            StartGame();
        }

    }
}
