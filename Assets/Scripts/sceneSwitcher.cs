using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class sceneSwitcher : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // main
    public GameObject mainScene;
    public GameObject graphScene;
    public TMP_Text buttonText;
    public Camera camera;
    Color graphColor, mainColor;
    public string currentScene = "main";

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

        if (ColorUtility.TryParseHtmlString("#BABABA", out graphColor))
        {
            camera.backgroundColor = graphColor;
        }
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

        if (ColorUtility.TryParseHtmlString("#314D79", out mainColor))
        {
            camera.backgroundColor = mainColor;
        }
        buttonText.text = "Graphs";
    }

}
