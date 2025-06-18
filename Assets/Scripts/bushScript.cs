using UnityEngine;
using System.Collections;

public class bushScript : MonoBehaviour
{
    public int numFruits;
    float hungerValue = 70.0f;
    float fruitGrowthTime = 10.0f;
    SpriteRenderer spriteRenderer;
    Sprite[] bushSprites;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bushSprites = Resources.LoadAll<Sprite>("Bush");
        StartCoroutine(GrowLoop());
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

    public float eaten()
    {
        Debug.Log("-1 Fruit");
        numFruits--;
        spriteRenderer.sprite = bushSprites[numFruits];
        if (numFruits == 0) gameObject.tag = "not food";
        return hungerValue;
    }

    public void growFruit()
    {
        Debug.Log("+1 Fruit");
        numFruits++;
        spriteRenderer.sprite = bushSprites[numFruits];
        gameObject.tag = "food";
    }
}
