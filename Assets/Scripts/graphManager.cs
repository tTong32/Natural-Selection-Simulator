using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class graphManager : MonoBehaviour
{

    public GameObject graphPrefab;

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

        int j = 0;
        for (int i = 0; i < numReturnedStats; i++)
        {
            if (!checkIgnoreStats(i)) // Skip hunger, water, energy, and thresholds
            {
                GameObject newGraph = Instantiate(graphPrefab, new Vector3(0, 0, 0), Quaternion.identity, graphScene.transform);
                blobStatGraphs.Add(newGraph.GetComponent<graphScript>());
                blobStatGraphs[j].setCenter(0, -8f);
                j++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateGraphs(int numBlobs)
    {
        blobNumGraph.UpdateData(numBlobs);
        blobStatAverages = returnAverage();

        int j = 0;
        for (int i = 0; i < numReturnedStats; i++)
        {
            if (!checkIgnoreStats(i))
            {
                blobStatGraphs[j].UpdateData(blobStatAverages[i]);
                j++;
            }
        }
    }

}
