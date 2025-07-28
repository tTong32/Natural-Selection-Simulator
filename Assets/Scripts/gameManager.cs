using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class gameManager : MonoBehaviour
{
    float turntime = 1.0f;
    float initialBlobSpawnOffset = 2.0f;

    public List<blobScript> blobList;

    public int numberOfTurns = 0;
    public static int numReturnedStats = 0;

    // delete
    // all graphs
    graphScript blobNumGraph;
    // Follows same order as returnStats
    List<graphScript> blobStatGraphs = new List<graphScript>();
    string[] graphNames = {"Population", "Movement", "Sight", "Reach", "Incubation Time", "Size",
        "Turn Time", "Predation", "Maturation Time", "Child Threshold"};
    
    float[] blobStatAverages;

    public GameObject blobPrefab;
    public GameObject mainScene;
    public GameObject graphScene;

    public sceneSwitcher sceneSwitch;
    public graphManager graphManager;

    void Awake()
    {
        blobList = new List<blobScript>();
        for (int i = 0; i < 10; i++)
        {
            spawnBlob(Random.Range(-initialBlobSpawnOffset, initialBlobSpawnOffset),
                      Random.Range(-initialBlobSpawnOffset, initialBlobSpawnOffset));
        }
        numReturnedStats = blobList[0].returnStats().Length;
        blobStatAverages = new float[numReturnedStats];
        StartCoroutine(TurnLoop());

        // delete
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
            graphManager.updateGraphs(blobList.count);
            Debug.Log("Graph Update");
            turnsUntilGraph = graphInterval;
        }

        foreach (blobScript b in new List<blobScript>(blobList)) { b.decay(1f); }
    }

    public IEnumerator blobReproduction(float[] b1stats, float[] b2stats, blobScript b1, blobScript b2)
    {
        blobScript female = b1.gender == "female" ? b1 : b2;
        female.startPregnancy();
        yield return new WaitForSeconds((b1stats[3] + b2stats[3]) / 2);
        if (female != null)
        {
             female.endPregnancy();
            Debug.Log("Spawn");
            blobScript child;
            if (b1 == female) child = spawnBlob(b1.returnPosition(), b1stats, b2stats, b1, b2);
            else child = spawnBlob(b2.returnPosition(), b1stats, b2stats, b1, b2);
            b1.children.Add(child);
            b2.children.Add(child);
        }
    }

    blobScript spawnBlob(float[] bPosition, float[] b1stats, float[] b2stats, blobScript b1, blobScript b2)
    {
        float x = bPosition[0];
        float y = bPosition[1];
        GameObject newBlob = Instantiate(blobPrefab, new Vector3(x, y, 0), Quaternion.identity, mainScene.transform);
        blobScript blob = newBlob.GetComponent<blobScript>();
        float[] newBlobStats = new float[numReturnedStats];
        float[] parentAvgStats = new float[numReturnedStats];
        for (int i = 0; i < numReturnedStats; i++)
        {
            parentAvgStats[i] = (b1stats[i] + b2stats[i]) / 2;
            if (checkIgnoreStats(i)) newBlobStats[i] = parentAvgStats[i];
            else if (i == 15)
            {
                newBlobStats[i] = parentAvgStats[i] + Random.Range(-0.07f, 0.07f);
                if (newBlobStats[i] <= 0) newBlobStats[i] = 0.0001f;
                else if (newBlobStats[i] >= 1) newBlobStats[i] = 0.9999f;
            }
            else newBlobStats[i] = parentAvgStats[i] * returnReproductionOffset();
        }
        blobScript father = b1.gender == "male" ? b1 : b2;
        blobScript mother = b1.gender == "female" ? b1 : b2;
        blob.setStats(newBlobStats, parentAvgStats, father, mother);
        blob.gender = Random.Range(0f, 1f) <= 0.5f ? "male" : "female";
        blobList.add(blob);
        checkScene(newBlob);
        return blob;
    }

    void spawnBlob(float x, float y)
    {
        GameObject newBlob = Instantiate(blobPrefab, new Vector3(x, y, 0), Quaternion.identity, mainScene.transform);
        blobScript blob = newBlob.GetComponent<blobScript>();
        blob.gender = Random.Range(0f, 1f) <= 0.5f ? "male" : "female";
        blobList.add(blob);
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
        if (blob.prey != null) blob.prey.predators.Remove(blob);
        foreach (blobScript predator in blob.predators)
        {
            predator.prey = null;
            blob.predators.Remove(predator);
        }
        Debug.Log("Dead");
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

    bool checkIgnoreStats(int i)
    {
        // Skip hunger, water, energy, and thresholds
        return i >= 6 && i <= 14;
    }
}