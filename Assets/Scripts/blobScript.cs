using UnityEngine;

public class blobScript : MonoBehaviour
{
    Rigidbody2D rb;

    // basic stats
    public float movement = 2f, sight = 4f, reach = 0.5f;
    float anchorMove, anchorSight, anchorReach;
    // base basic stats
    float baseMovement = 2f, baseSight = 4f, baseReach = 0.5f;

    // target stats
    float moveX = 0.0f, moveY = 0.0f;

    // hunger stats
    float maxHunger = 100.0f, hunger = 55.0f, hungerDecayRate = 2.5f, hungerThreshold = 35.0f;
    // water stats
    float maxWater = 100.0f, water = 55.0f, waterDecayRate = 7f, waterThreshold = 40.0f;
    // reproduction stats
    float reproReach = 2f, incubationTime = 10f, reproThreshold = 60f;
    // energy stats
    float maxEnergy = 100f, energy = 100f, energyDecayRate = 0.9f, energyRestorationRate = 9f, energyThreshold = 25f;
    // tired stats
    float tiredMove, tiredSight, tiredReach;
    bool sleep = false, tired = false;
    bool reproduced = false;
    gameManager[] gm;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectsByType<gameManager>(FindObjectsSortMode.None);
        tiredMove = movement / 2;
        anchorMove = movement;
        tiredSight = sight / 2;
        anchorSight = sight;
        tiredReach = reach / 2;
        anchorReach = reach;
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
        reproduced = false;
        Collider2D[] withinReach = Physics2D.OverlapCircleAll(transform.position, reach);

        // Decrease hunger and water by decayrate
        hunger -= hungerDecayRate;
        water -= waterDecayRate;
        energy -= energyDecayRate;

        // check water first
        if (water < waterThreshold)
        {
            sleep = false;
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
            sleep = false;
            // serves as a method to see if water is within reach
            bool eaten = checkHunger(withinReach);
            if (!eaten)
            {
                // If blob has not eaten
                seek("food");
                return;
            }
        }
        else if ((energy < energyThreshold || sleep) && energy < maxEnergy)
        {
            sleep = checkSleep(withinReach);
            if (!sleep)
            {
                seek("shelter");
                return;
            }
        }
        else
        {
            sleep = false;
            // Otherwise, wander randomly
            wander();
            return;
        }

        if (checkReproductionConditions())
        {
            checkReproduction();
        }

        if (energy < energyThreshold - 10) setStats("tired");
        else setStats("normal"); 
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
            gm[0].removeBlob(this);
            Debug.Log("Dead");
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
                    energy -= 15f;
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
                energy -= 12f;
                water = maxWater;
                return true;
            }
        }
        return false;
    }

    bool checkSleep(Collider2D[] withinReach)
    {
        foreach (Collider2D collider in withinReach)
        {
            if (collider.CompareTag("shelter"))
            {
                energy += energyRestorationRate;
                if (energy > maxEnergy) energy = maxEnergy;
                return true;
            }
        }
        return false;
    }

    public bool checkReproductionConditions()
    {
        if (hunger > reproThreshold && water > reproThreshold && energy > reproThreshold && reproduced == false)
        {
            return true;
        }
        return false;
    }

    void checkReproduction()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, reproReach);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("blob") && collider.gameObject != this.gameObject)
            {
                blobScript otherBlob = collider.GetComponent<blobScript>();
                if (otherBlob.checkReproductionConditions())
                {
                    Debug.Log("Success");
                    gm[0].blobReproduction(returnPosition(),
                                            otherBlob.returnPosition(),
                                            returnStats(),
                                            otherBlob.returnStats());
                    reproduced = true;
                    water -= 37.0f;
                    hunger -= 37.0f;
                    energy -= 37.0f;
                    otherBlob.water -= 37.0f;
                    otherBlob.hunger -= 37.0f;
                    otherBlob.energy -= 37.0f;
                    return;
                }
            }
        }
    }

    void setStats(string condition)
    {
        if (condition == "tired" && !tired)
        {
            movement = tiredMove;
            sight = tiredSight;
            reach = tiredReach;
            tired = true;
        }
        else if (condition == "normal" && tired)
        {
            movement = anchorMove;
            sight = anchorSight;
            reach = anchorReach;
            tired = false;
        }
    }

    float[] returnStats()
    {
        float[] stats = new float[4];
        stats[0] = movement;
        stats[1] = sight;
        stats[2] = reach;
        stats[3] = incubationTime;
        /*
        stats[3] = maxHunger;
        stats[4] = hungerDecayRate;
        stats[5] = hungerThreshold;
        stats[6] = maxWater;
        stats[7] = waterDecayRate;
        stats[8] = waterThreshold;
        stats[9] = reproReach;
        stats[10] = incubationTime;
        */
        return stats;
    }

    public void setStats(float[] stats)
    {
        movement = stats[0];
        sight = stats[1];
        reach = stats[2];
        incubationTime = stats[3];
        /*
        maxHunger = stats[3];
        hungerDecayRate = stats[4];
        hungerThreshold = stats[5];
        maxWater = stats[6];
        waterDecayRate = stats[7];
        waterThreshold = stats[8];
        reproReach = stats[9];
        incubationTime = stats[10];
        */

        // decay rates are inversely proportional to movement
        hungerDecayRate = baseMovement / movement * hungerDecayRate;
        waterDecayRate = baseMovement / movement * waterDecayRate;

        // Thresholds are inversely proportional to sight
        hungerThreshold = baseSight / sight * hungerThreshold;
        waterThreshold = baseSight / sight * waterThreshold;
    }

    float[] returnPosition()
    {
        float[] position = new float[2];
        position[0] = transform.position.x;
        position[1] = transform.position.y;
        return position;
    }
}

