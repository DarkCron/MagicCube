using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    public Color notAvailableColor;
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
        if (!bContinueGameIsAvailable)
        {
            continueText.color = notAvailableColor;
        }
        ChangeValue(cubeSizeSlider.value);
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
                Camera.main.transform.RotateAround(magicCube.GetComponent<MagicCubeBehaviour>().getMagicCubeManager().GetPivot(), new Vector3(0, 1, 1), Time.deltaTime * 30.0f);
            }
        }
    }

    public void ChangeValue(float value)
    {
        sliderText.text = Mathf.RoundToInt(value).ToString();
        if (cubeSize != Mathf.RoundToInt(value))
        {
            Camera.main.GetComponent<CubeControl>().CreateMagicCube(Mathf.RoundToInt(cubeSizeSlider.value));
        }
        cubeSize = Mathf.RoundToInt(value);
    }

    public void StartGame()
    {
        Camera.main.GetComponent<CubeControl>().CreateMagicCube(Mathf.RoundToInt(cubeSizeSlider.value));
        MainGameLogic.SetCurrentActiveElement(MainGameLogic.CurrentActiveElement.MAIN_GAME);
        MainGameLogic.LinkMagicCubeManagerAndUI(Camera.main.GetComponent<CubeControl>().GetMagicCubeManager(), Camera.main.GetComponent<UIManager>().GetMainGameUI());

        Camera.main.GetComponent<UIManager>().MainMenuToGame();
        Camera.main.GetComponent<CubeControl>().GetMagicCubeManager().InitRandomMoves();
    }
}
