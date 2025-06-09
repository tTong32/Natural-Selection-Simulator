using UnityEngine;
using System.Collections;

public class gameManager : MonoBehaviour
{
    float turntime = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(TurnLoop());
    }

    System.Collections.IEnumerator TurnLoop()
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
}
