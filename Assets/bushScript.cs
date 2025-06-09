using UnityEngine;

public class bushScript : MonoBehaviour
{
    public int numFruits;
    float hungerValue = 70;
    SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Log(spriteRenderer.sprite.name);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float eaten()
    {
        Debug.Log("-1 Fruit");
        numFruits--;
        spriteRenderer.sprite = Resources.Load<Sprite>("Bush_" + numFruits);
        if (numFruits == 0) gameObject.tag = "not food";
        return hungerValue;
    }

    public void growFruit()
    {
        numFruits++;
        spriteRenderer.sprite = Resources.Load<Sprite>("Bush_" + numFruits);
        gameObject.tag = "food";
    }
}
