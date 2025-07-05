using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class gameManager : MonoBehaviour
{
    float turntime = 1.0f;
    float initialBlobSpawnOffset = 2.0f;

    public List<blobScript> blobList;

    // every 15 seconds, put a point on the graph
    int graphInterval = 15;
    int turnsUntilGraph = 0;
    public int numberOfTurns = 0;
    int numReturnedStats = 0;

    // all graphs
    graphScript blobNumGraph;
    // Follows same order as returnStats
    List<graphScript> blobStatGraphs = new List<graphScript>();
    float[] blobStatAverages;

    public GameObject blobPrefab;
    public GameObject graphPrefab;
    public GameObject mainScene;
    public GameObject graphScene;
    public sceneSwitcher sceneSwitch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blobList = new List<blobScript>();
        for (int i = 0; i < 8; i++)
        {
            spawnBlob(Random.Range(-initialBlobSpawnOffset, initialBlobSpawnOffset),
                      Random.Range(-initialBlobSpawnOffset, initialBlobSpawnOffset));
        }
        numReturnedStats = blobList[0].returnStats().Length;
        blobStatAverages = new float[numReturnedStats];
        StartCoroutine(TurnLoop());

        GameObject numBlobs = Instantiate(graphPrefab, new Vector3(0, 0, 0), Quaternion.identity, graphScene.transform);
        blobNumGraph = numBlobs.GetComponent<graphScript>();
        blobNumGraph.setCenter(0, 0);

        for (int i = 0; i < numReturnedStats; i++)
        {
            GameObject newGraph = Instantiate(graphPrefab, new Vector3(0, 0, 0), Quaternion.identity, graphScene.transform);
            blobStatGraphs.Add(newGraph.GetComponent<graphScript>());
            blobStatGraphs[i].setCenter(0, -8f);
        }
    }

    IEnumerator TurnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(turntime);
            turn();
        }
    }

    void turn()
    {
        // Logic for a blob turn can be added here
        turnsUntilGraph--;
        numberOfTurns++;
        if (turnsUntilGraph <= 0)
        {
            Debug.Log("Graph Update");
            blobNumGraph.UpdateData(blobList.Count);
            blobStatAverages = returnAverage();
            for (int i = 0; i < numReturnedStats; i++)
            {
                blobStatGraphs[i].UpdateData(blobStatAverages[i]);
            }
            turnsUntilGraph = graphInterval;
        }

        foreach (blobScript b in new List<blobScript>(blobList)) { b.turn(); }
    }

    public IEnumerator blobReproduction(float[] b1pos, float[] b2pos, float[] b1stats, float[] b2stats)
    {
        yield return new WaitForSeconds((b1stats[3] + b2stats[3]) / 2);
        Debug.Log("Spawn");
        spawnBlob((b1pos[0] + b2pos[0]) / 2, (b1pos[1] + b2pos[1]) / 2, b1stats, b2stats);
    }

    void spawnBlob(float x, float y, float[] b1stats, float[] b2stats)
    {
        GameObject newBlob = Instantiate(blobPrefab, new Vector3(x, y, 0), Quaternion.identity, mainScene.transform);
        blobScript blob = newBlob.GetComponent<blobScript>();
        float[] newBlobStats = new float[numReturnedStats];
        for (int i = 0; i < numReturnedStats; i++) { newBlobStats[i] = (b1stats[i] + b2stats[i]) / 2 * returnReproductionOffset(); }
        blob.setStats(newBlobStats);
        blobList.Add(blob);
        checkScene(newBlob);
        return;
    }

    void spawnBlob(float x, float y)
    {
        GameObject newBlob = Instantiate(blobPrefab, new Vector3(x, y, 0), Quaternion.identity, mainScene.transform);
        blobScript blob = newBlob.GetComponent<blobScript>();
        blobList.Add(blob);
    }

    float returnReproductionOffset()
    {
        float n = Random.Range(-50f, 50f);

        if (n < 0)
        {
            return 1 - 0.008f * Mathf.Pow(n, 2) / 100;
        }
        else
        {
            return 1 + 0.008f * Mathf.Pow(n, 2) / 100;
        }
    }

    public void removeBlob(blobScript blob)
    {
        blobList.Remove(blob);
        Destroy(blob.gameObject);
    }

    void checkScene(GameObject obj)
    {
        if (sceneSwitch.currentScene != "main")
        {
            obj.GetComponent<Renderer>().enabled = false;
        }
    }

    public void checkScene(blobScript blob)
    {
        if (sceneSwitch.currentScene != "main")
        {
            blob.GetComponent<Renderer>().enabled = false;
        }
    }

    float[] returnAverage()
    {
        int n = blobList.Count;
        float[] avg = new float[numReturnedStats];

        foreach (blobScript b in blobList)
        {
            float[] stats = new float[numReturnedStats];
            stats = b.returnStats();
            for (int i = 0; i < numReturnedStats; i++)
                avg[i] += stats[i];
        }

        for (int i = 0; i < numReturnedStats; i++)
                avg[i] /= (float)n;

        return avg;
    }
}
