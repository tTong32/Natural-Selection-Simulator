using UnityEngine;

public class bushScript : MonoBehaviour
{
    public int numFruits = 3;

    public Sprite bushSprite;
    SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void eaten()
    {
        numFruits--;
        bushSprite = Resources.Load<Sprite>("Bush_" + numFruits);
    }

    public void growFruit()
    {
        numFruits++;
        bushSprite = Resources.Load<Sprite>("Bush_" + numFruits);
    }
}
