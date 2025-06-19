using UnityEngine;
using System.Collections;

public class gameManager : MonoBehaviour
{
    float turntime = 1.0f;
    float initialBlobSpawnOffset = 2.0f;
    public GameObject blobPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(TurnLoop());
        for (int i = 0; i < 6; i++)
        {
            spawnBlob(Random.Range(-initialBlobSpawnOffset, initialBlobSpawnOffset),
                      Random.Range(-initialBlobSpawnOffset, initialBlobSpawnOffset));
        }
        blobScript[] blob = FindObjectsByType<blobScript>(FindObjectsSortMode.None);

    }

    IEnumerator TurnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(turntime);
            // Logic for turning the blob can be added here
            blobScript[] blob = FindObjectsByType<blobScript>(FindObjectsSortMode.None);
            foreach (blobScript b in blob)
            {
                b.turn();
            }
        }
    }

    void spawnBlob(float x, float y, float[] b1stats, float[] b2stats)
    {
        int n = b1stats.Length;
        GameObject newBlob = Instantiate(blobPrefab, new Vector3(x, y, 0), Quaternion.identity);
        blobScript blob = newBlob.GetComponent<blobScript>();
        float[] newBlobStats = new float[n];
        for (int i = 0; i < n; i++) { newBlobStats[i] = (b1stats[i] + b2stats[i]) / 2 * returnReproductionOffset(); }
        blob.setStats(newBlobStats);
        return;
    }

    void spawnBlob(float x, float y)
    {
        Instantiate(blobPrefab, new Vector3(x, y, 0), Quaternion.identity);
    }

    public void blobReproduction(float[] b1pos, float[] b2pos, float[] b1stats, float[] b2stats)
    {
        spawnBlob((b1pos[0] + b2pos[0]) / 2, (b1pos[1] + b1pos[1]) / 2, b1stats, b2stats);
    }

    float returnReproductionOffset()
    {
        return 1 + 0.00016f * Mathf.Pow(Random.Range(-50f, 50f), 3) / 100;
    } 
}
