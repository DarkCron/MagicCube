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

    private bool bContinueGameIsAvailable = false;

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
        
    }

    public void ChangeValue(float value)
    {
        sliderText.text = Mathf.RoundToInt(value).ToString();
    }
}
