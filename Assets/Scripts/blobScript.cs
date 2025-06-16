using UnityEngine;

public class blobScript : MonoBehaviour
{
    Rigidbody2D rb;

    // basic stats
    float movement = 2f, sight = 10f, reach = 0.5f;

    // target stats
    float moveX = 0.0f, moveY = 0.0f;

    // hunger stats
    float maxHunger = 100.0f, hunger = 100.0f, hungerDecayRate = 2.5f, hungerThreshold = 35.0f;
    // water stats
    float maxWater = 100.0f, water = 100.0f, waterDecayRate = 6.5f, waterThreshold = 40.0f;
    // reproduction stats
    float reproReach = 0.5f, incubationTime = 10f, reproThreshold = 70f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update function was deleted, but can be put back if needed for non-rigidbody functions
    // FixedUpdate is called at a fixed interval
    void FixedUpdate()
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
            // serves as a method to see if water is within reach
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
            // serves as a method to see if water is within reach
            bool eaten = checkHunger(withinReach);
            if (!eaten)
            {
                // If blob has not eaten
                seek("food");
                return;
            }
        }
        else if (hunger < reproThreshold && water < reproThreshold)
        {
            checkReproduction();
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
        Vector2 target = new Vector2(moveX, moveY);
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, movement * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, sight);

        Collider2D closest = null;
        float closestDistance = float.MaxValue;

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

        if (closest != null)
        {
            Vector2 direction = closest.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x);
            wander(angle);
        }
        else
        {
            wander();
        }
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
        // if blob is within reach of food, eat food
        foreach (Collider2D collider in withinReach)
        {
            if (collider.CompareTag("food"))
            {
                // If the blob is close enough to the food, eat it
                if (collider.TryGetComponent<bushScript>(out var bush) && bush.numFruits > 0)
                {
                    Debug.Log("Eaten!");
                    hunger += bush.eaten();
                    if (hunger > maxHunger) hunger = maxHunger; // Cap the hunger at maxHunger
                    return true;
                }
            }
        }
        return false;
    }

    bool checkWater(Collider2D[] withinReach)
    {
        foreach (Collider2D collider in withinReach)
        {
            if (collider.CompareTag("water"))
            {
                water = maxWater;
                return true;
            }
        }
        return false;
    }

    void checkReproduction()
    {
        
    }
}

