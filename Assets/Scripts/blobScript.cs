using UnityEngine;

public class blobScript : MonoBehaviour
{
    // Radius of the blob
    float movement = 1.5f;
    float sight = 10f;

    float reach = 0.5f;

    GameObject target;
    float moveX = 0.0f;
    float moveY = 0.0f;

    float maxHunger = 100.0f;
    // Current hunger level of the blob
    float hunger = 100.0f;
    // Decay rate of hunger per turn
    float hungerDecayRate = 2.5f;
    float hungerThreshold = 35.0f;

    float maxWater = 100.0f;
    float water = 100.0f;

    float waterDecayRate = 6.5f;
    float waterThreshold = 40.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        move();
    }

    public void turn()
    {

        checkDeath();
        Collider2D[] withinReach = Physics2D.OverlapCircleAll(transform.position, reach);

        // Decrease hunger and water by decayrate
        hunger -= hungerDecayRate;
        water -= waterDecayRate;

        // check water first
        if (water < waterThreshold)
        {
            bool drank = checkWater(withinReach);
            if (!drank)
            {
                seek("water");
                return;
            }
        }
        // If hunger is below the threshold, seek food
        else if (hunger < hungerThreshold)
        {
            bool eaten = checkHunger(withinReach);
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
            return; // no target found
        }

        // find the closest food item with the specified tag
        float closestDistance = sight + 1f; // Initialize to a value larger than sight radius
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

    void checkDeath()
    {
        if (hunger <= 0.0f || water <= 0.0f)
        {
            // If the blob's hunger reaches zero, destroy it
            Destroy(gameObject);
        }
    }

    bool checkHunger(Collider2D[] withinReach)
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
        return eaten;
    }

    bool checkWater(Collider2D[] withinReach)
    {
        bool drank = false;
        foreach (Collider2D collider in withinReach)
        {
            if (collider.CompareTag("water"))
            {
                water = maxWater;
                drank = true;
            }
        }
        return drank;
    }
}

