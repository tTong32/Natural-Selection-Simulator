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
        for (int i = 0; i < 4; i++)
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

    void spawnBlob(float x, float y)
    {
        Instantiate(blobPrefab, new Vector3(x, y, 0), Quaternion.identity);
    }

    public void blobReproduction(float b1x, float b1y, float b2x, float b2y)
    {
        spawnBlob((b1x + b2x) / 2, (b1y + b2y) / 2);
    }
}
