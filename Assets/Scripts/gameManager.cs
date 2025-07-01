using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class gameManager : MonoBehaviour
{
    float turntime = 1.0f;
    float initialBlobSpawnOffset = 2.0f;

    // every 15 seconds, put a point on the graph
    int graphInterval = 15;
    int turnsUntilGraph = 0;
    public int numberOfTurns = 0;
    public GameObject blobPrefab;
    public GameObject mainScene;
    public sceneSwitcher sceneSwitch;
    public graphScript graphScript;
    List<blobScript> blobList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blobList = new List<blobScript>();
        for (int i = 0; i < 8; i++)
        {
            spawnBlob(Random.Range(-initialBlobSpawnOffset, initialBlobSpawnOffset),
                      Random.Range(-initialBlobSpawnOffset, initialBlobSpawnOffset));
        }
        StartCoroutine(TurnLoop());
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
            graphScript.UpdateData(blobList);
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
        int n = b1stats.Length;
        GameObject newBlob = Instantiate(blobPrefab, new Vector3(x, y, 0), Quaternion.identity, mainScene.transform);
        blobScript blob = newBlob.GetComponent<blobScript>();
        float[] newBlobStats = new float[n];
        for (int i = 0; i < n; i++) { newBlobStats[i] = (b1stats[i] + b2stats[i]) / 2 * returnReproductionOffset(); }
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
}
