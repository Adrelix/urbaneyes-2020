using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMaster : MonoBehaviour
{
    public GameObject streetGen;
    // Start is called before the first frame update
    void Start()
    {

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        streetGen.GetComponent<StreetGen>().makeRoad();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
