using UnityEngine;

public class blobScript : MonoBehaviour
{
    // Radius of the blob
    public float movement = 0.5f;
    public float sight = 2.5f;

    public float reach = 0.2f;

    GameObject target;
    float moveX = 0.0f;
    float moveY = 0.0f;

    float maxHunger = 100.0f;
    // Current hunger level of the blob
    float hunger = 100.0f;
    // Decay rate of hunger per turn
    float hungerDecayRate = 7.5f;
    float hungerThreshold = 30.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        move();

        if (hunger <= 0.0f)
        {
            // If the blob's hunger reaches zero, destroy it
            Destroy(gameObject);
        }
    }

    public void turn()
    {

        // Decrease hunger by the decay rate
        hunger -= hungerDecayRate;

        // if blob is within reach of food, eat food


        // If hunger is below the threshold, seek food
        if (hunger < hungerThreshold)
        {
            seek("food");
        }
        else
        {
            // Otherwise, wander randomly
            wander();
        }
    }

    void move()
    {
        // it takes turnTime to move destination
        transform.position += new Vector3((moveX - transform.position.x) * movement * Time.deltaTime,
                                        (moveY - transform.position.y) * movement * Time.deltaTime, 0);
    }

    void wander()
    {
        // change the x and y destination of the blob
        float angle = Random.Range(0f, 2 * Mathf.PI);
        moveX = transform.position.x + Mathf.Cos(angle) * movement;
        moveY = transform.position.y + Mathf.Sin(angle) * movement;
    }

    void seek(string tag)
    {
        // Find food within sight radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, sight);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(tag))
            {
                // Move towards the food
                Vector3 foodPosition = collider.transform.position;
                moveX = foodPosition.x;
                moveY = foodPosition.y;
                break; // Stop searching after finding the first food item
            }
        }
    }
}

