using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationHandler : MonoBehaviour
{
    public Text confirmationText;

    public delegate void YesFunction();
    public delegate void NoFunction();
    public delegate void CloseFunction();

    private YesFunction yesFunction;
    private NoFunction noFunction;
    private CloseFunction closeFunction;

    public void InitConfirmationCanvas(YesFunction yf, NoFunction nf, CloseFunction cf)
    {
        yesFunction = yf;
        noFunction = nf;
        closeFunction = cf;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CloseConfirmation()
    {
        closeFunction();
    }

    public void YesConfirmation()
    {
        yesFunction();
    }

    public void NoConfirmation()
    {
        noFunction();
    }

    internal void SetMessage(String message)
    {
        confirmationText.text = message;
    }
}
