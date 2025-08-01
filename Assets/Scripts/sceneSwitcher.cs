using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class sceneSwitcher : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // main
    public GameObject mainScene;
    public GameObject graphScene;
    public TMP_Text buttonText;

    public string currentScene = "main";
    public cameraScript cam;

    void Start()
    {
        SwitchToMain();
    }

    public void SwitchScene()
    {
        if (currentScene == "main")
        {
            SwitchToGraph();
            currentScene = "graph";
        }
        else
        {
            SwitchToMain();
            currentScene = "main";
        }

    }

    void SwitchToGraph()
    {
        foreach (var renderer in mainScene.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }

        foreach (var renderer in graphScene.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }

        foreach (var text in graphScene.GetComponentsInChildren<TMP_Text>())
        {
            text.enabled = true;
        }
        foreach (var dropdown in graphScene.GetComponentsInChildren<TMP_Dropdown>(true))
        {
            dropdown.gameObject.SetActive(true);
        }
        cam.setGraphCamera();
        buttonText.text = "Back";
    }

    void SwitchToMain()
    {
        foreach (var renderer in mainScene.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }

        foreach (var renderer in graphScene.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        foreach (var text in graphScene.GetComponentsInChildren<TMP_Text>())
        {
            text.enabled = false;
        }
        foreach (var dropdown in graphScene.GetComponentsInChildren<TMP_Dropdown>(true))
        {
            dropdown.gameObject.SetActive(false);
        }
        cam.setMainCamera();
        buttonText.text = "Graphs";

    }

}
