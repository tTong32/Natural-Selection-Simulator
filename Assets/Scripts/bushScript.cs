using UnityEngine;
using System.Collections;

public class bushScript : MonoBehaviour
{
    public int numFruits;
    float fruitGrowthTime = 10.0f;
    public float hungerValue = 0.0f;
    SpriteRenderer spriteRenderer;
    Sprite[] bushSprites;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bushSprites = Resources.LoadAll<Sprite>("Bush");
        StartCoroutine(GrowLoop());
        hungerValue = Random.Range(40f, 60f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GrowLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(fruitGrowthTime);
            if (numFruits < 3)
            {
                growFruit();
            }
        }
    }

    public void eaten()
    {
        numFruits--;
        spriteRenderer.sprite = bushSprites[numFruits];
        if (numFruits == 0) gameObject.tag = "not food";
    }

    public float returnHungerValue()
    {
        return hungerValue;
    }

    public void growFruit()
    {
        numFruits++;
        spriteRenderer.sprite = bushSprites[numFruits];
        gameObject.tag = "food";
    }
}
