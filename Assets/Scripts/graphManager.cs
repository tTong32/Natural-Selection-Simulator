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

    // centers of the graphs
    float[] topLeftCenter = { -8f, 5f }, topRightCenter = {9f, 5f},
        bottomLeftCenter = { -8f, -5f }, bottomRightCenter = { 9f, -5f };

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
                blobStatGraphs[j].setCenter(16 * (i+1), 0f);
                if (j < cameras.Length)
                {
                    cameras[j].transform.Translate(new Vector3(-6.5f + 16 * j, 4, -10), Space.World);
                    cameras[j].orthographicSize = 8f;
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
            graphDropdowns[i].ClearOptions();
            graphDropdowns[i].AddOptions(new List<string>(graphNames));
            graphDropdowns[i].value = i;
            graphDropdowns[i].onValueChanged.AddListener((index) => OnDropdownChanged(i, index));
        }
    }

    void OnDropdownChanged(int index, int value)
    {
        
    }
}
