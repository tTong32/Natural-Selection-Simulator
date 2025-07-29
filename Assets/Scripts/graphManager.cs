using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class graphManager : MonoBehaviour
{

    public GameObject graphPrefab;
    public GameObject graphScene;
    public gameManager gameManager;

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

        GameObject numBlobs = Instantiate(graphPrefab, new Vector3(0, 0, 0), Quaternion.identity, graphScene.transform);
        blobNumGraph = numBlobs.GetComponent<graphScript>();
        blobNumGraph.setCenter(0, 0);

        numReturnedStats = gameManager.numReturnedStats;

        int j = 0;
        for (int i = 0; i < numReturnedStats; i++)
        {
            if (!gameManager.checkIgnoreStats(i)) // Skip hunger, water, energy, and thresholds
            {
                GameObject newGraph = Instantiate(graphPrefab, new Vector3(0, 0, 0), Quaternion.identity, graphScene.transform);
                blobStatGraphs.Add(newGraph.GetComponent<graphScript>());
                blobStatGraphs[j].setCenter(0, -8f);
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
}
