using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class StartMenuScript : MonoBehaviour
{
    // public Slider[] optionSliders;
    public Text[] texts;

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void AdjustStreetWidth(float newPar){		//Setting the parameter values to the static values that can later be accessed from all scenes	
    Debug.Log(newPar);
    texts[0].text = newPar.ToString("0") + "%";
    ParameterValues.streetWidth = newPar;
    
    }
    public void AdjustShoulderWidth(float newPar){
    texts[1].text = newPar.ToString("0") + "%";
    ParameterValues.shoulderWidth = newPar;
    }
    public void AdjustOption3(float newPar){
    ParameterValues.option3 = newPar;
    }
    public void AdjustOption4(float newPar){
    ParameterValues.option4 = newPar;
    }
    public void AdjustOption5(float newPar){
    ParameterValues.option5 = newPar;
    }

}
