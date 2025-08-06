using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class graphManager : MonoBehaviour
{

    public GameObject graphPrefab;
    public GameObject graphScene;
    public GameObject graphList;
    public gameManager gameManager;

    TMP_Dropdown[] graphDropdowns = new TMP_Dropdown[4];
    Camera[] cameras = new Camera[4];

    int numReturnedStats;

    // all graphs
    graphScript blobNumGraph;
    // Follows same order as returnStats
    List<graphScript> blobStatGraphs = new List<graphScript>();
    string[] graphNames = {"Population", "Movement", "Sight", "Reach", "Incubation Time", "Size",
        "Turn Time", "Predation", "Maturation Time", "Child Threshold"};

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        GameObject numBlobs = Instantiate(graphPrefab, new Vector3(0, 0, 0), Quaternion.identity, graphList.transform);
        blobNumGraph = numBlobs.GetComponent<graphScript>();
        blobNumGraph.setCenter(0, 0); 
        numReturnedStats = gameManager.numReturnedStats;

        graphDropdowns = graphScene.GetComponentsInChildren<TMP_Dropdown>(true);
        cameras = graphScene.GetComponentsInChildren<Camera>(true);
        inititializeDropdowns();

        int j = 0;
        for (int i = 0; i < numReturnedStats; i++)
        {
            if (!gameManager.checkIgnoreStats(i)) // Skip hunger, water, energy, and thresholds
            {
                GameObject newGraph = Instantiate(graphPrefab, new Vector3(0, 0, 0), Quaternion.identity, graphList.transform);
                blobStatGraphs.Add(newGraph.GetComponent<graphScript>());
                blobStatGraphs[j].setCenter(20 * (j+1), 0f);

                if (j < cameras.Length)
                {
                    // Set the center of the graph in world space
                    float centerX = -3.75f + 20 * j;
                    float centerY = 4.5f;
                    float graphWidth = 16f;
                    float graphHeight = 9f;

                    cameras[j].transform.position = new Vector3(centerX, centerY, -10);
                    cameras[j].orthographicSize = 5.1f;

                    // this sets the viewbox of the camera to a 16:9 aspect ratio
                    Rect rect = cameras[j].rect;
                    rect.x = 0;
                    rect.y = 0;
                    rect.width = graphWidth / graphHeight;
                    rect.height = 1f;
                    cameras[j].rect = rect;
                }
                j++;
            }
        }
    }

    public void updateGraphs(int numBlobs, float[] blobStatAverages)
    {
        blobNumGraph.UpdateData(numBlobs);

        int j = 0;
        for (int i = 0; i < numReturnedStats; i++)
        {
            if (!gameManager.checkIgnoreStats(i))
            {
                blobStatGraphs[j].UpdateData(blobStatAverages[i]);
                j++;
            }
        }
    }

    void inititializeDropdowns()
    {
        for (int i = 0; i < graphDropdowns.Length; i++)
        {
            int dropdownIndex = i;
            graphDropdowns[i].ClearOptions();
            graphDropdowns[i].AddOptions(new List<string>(graphNames));
            graphDropdowns[i].value = i;
            graphDropdowns[i].onValueChanged.AddListener((index) => OnDropdownChanged(dropdownIndex, index));

            var template = graphDropdowns[i].template;
            var itemText = template.Find("Viewport/Content/Item/Item Label")?.GetComponent<TMPro.TextMeshProUGUI>();
            if (itemText != null) itemText.fontSize = 20;
    
        }
    }

    void OnDropdownChanged(int dropdownIndex, int dropdownChosen)
    {
        cameras[dropdownIndex].transform.position = new Vector3(-3.75f + 20 * dropdownChosen, 4.5f, -10);
    }
}
