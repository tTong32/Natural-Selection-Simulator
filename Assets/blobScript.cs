using UnityEngine;

public class blobScript : MonoBehaviour
{
    // Radius of the blob
    public float movement;
    public float sight;

    public float reach;

    GameObject target;
    float moveX = 0.0f;
    float moveY = 0.0f;

    float maxHunger = 100.0f;
    // Current hunger level of the blob
    float hunger = 100.0f;
    // Decay rate of hunger per turn
    float hungerDecayRate = 5.0f;
    float hungerThreshold = 35.0f;

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
        
        Collider2D[] withinReach = Physics2D.OverlapCircleAll(transform.position, reach);

        // Decrease hunger by the decay rate
        hunger -= hungerDecayRate;

        // If hunger is below the threshold, seek food
        if (hunger < hungerThreshold)
        {
            bool eaten = false;

            // if blob is within reach of food, eat food
            foreach (Collider2D collider in withinReach)
            {
                if (collider.CompareTag("food"))
                {
                    // If the blob is close enough to the food, eat it
                    if (collider.GetComponent<bushScript>().numFruits <= 0) continue;
                    else
                    {
                        Debug.Log("Eaten!");
                        hunger += collider.GetComponent<bushScript>().eaten();
                        if (hunger > maxHunger) hunger = maxHunger; // Cap the hunger at maxHunger
                        eaten = true;
                    }
                }
            }
            
            if (!eaten)
            {
                // If blob has not eaten
                seek("food");
                Debug.Log("Seeking food");
                return;
            }

        }
        else
        {
            // Otherwise, wander randomly
            wander();
            return;
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

    void wander(float angle)
    {
        moveX = transform.position.x + Mathf.Cos(angle) * movement;
        moveY = transform.position.y + Mathf.Sin(angle) * movement;
    }

    void seek(string tag)
    {
        // Find food within sight radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, sight);

        if (colliders.Length == 0)
        {
            wander();
            return; // No food found, exit the method
        }

        // find the closest food item with the specified tag
        float closestDistance = sight+1f; // Initialize to a value larger than sight radius
        Collider2D closest = null;
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(tag))
            {
                float dist = Vector2.Distance(transform.position, collider.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closest = collider;
                }
            }
        }
        // find angle between the blob and the closest food item
        Vector2 direction = closest.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x);
        wander(angle);
    }
}

