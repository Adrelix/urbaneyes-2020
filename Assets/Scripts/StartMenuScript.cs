using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StartMenuScript : MonoBehaviour
{


    public void PlayGame()
    {
    SceneManager.LoadScene("SampleScene");
    }



    public void AdjustOption1(float newPar){		//Setting the parameter values to the static values that can later be accessed from all scenes	
    ParameterValues.option1 = newPar;		
    }
    public void AdjustOption2(float newPar){
    ParameterValues.option2 = newPar;
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
