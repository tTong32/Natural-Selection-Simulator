using UnityEngine;

public class blobScript : MonoBehaviour
{
    Rigidbody2D rb;
    public CircleCollider2D circleCollider;
    Renderer ren;
    gameManager[] gm;
    shelterScript shelter;

    // basic stats
    float baseMovement = 2f, baseSight = 4f, baseReach = 1f, baseSize = 0.64f;
    public float movement = 2f, sight = 4f, reach = 1f, size = 0.64f;

    // base basic stats

    // target stats
    float moveX = 0.0f, moveY = 0.0f, speed = 0.0f;

    // hunger stats
    float maxHunger = 100.0f, hungerDecayRate = 3f, hungerThreshold = 35.0f;
    // water stats
    float maxWater = 100.0f, waterDecayRate = 8f, waterThreshold = 35.0f;
    // reproduction stats
    float reproReach = 2f, incubationTime = 10f, reproThreshold = 60f;
    // energy stats
    float maxEnergy = 100f, energyDecayRate = 4f, energyRestorationPercent = 0.15f;
    
    // tired and sleep modifiers have the current and anchor stat
    // tired reduces move by 30%, sleep reduces decay by 50%
    float[] tiredFactor = { 1f, 0.8f }, sleepFactor = { 1f, 0.65f };

    // energy costs have a cost, a threshold, and the boost
    float[] sightEnergyCost = {10f, 20f, 0.25f};
    float[] decayEnergyCost = {5f, 5f, 0.15f}; // reduces decay by 20%
    // fourth energy cost is the sprint condition for water and hunger
    float[] movementEnergyCost = { 15f, 40f, 0.15f, 10}; // increases move by 25%
    public float energy = 100f, hunger = 55f, water = 55f;
    bool reproduced = false, sleep = false, hidden = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectsByType<gameManager>(FindObjectsSortMode.None);
        ren = GetComponent<Renderer>();
        shelter = null;
        wander(false, "none");
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

        hunger -= hungerDecayRate * sleepFactor[0];
        water -= waterDecayRate * sleepFactor[0];
        if (!sleep) energy -= energyDecayRate;
        sleep = false;

        if (energy > decayEnergyCost[1] && !sleep)
        {
            energy -= decayEnergyCost[0];
            // hunger and water increase by a percentage of decayRate
            hunger += hungerDecayRate * decayEnergyCost[2];
            water += waterDecayRate * decayEnergyCost[2];
        }

        if (energy < 0f) { energy = 0f; } // Prevent negative energy

        // check water first
        if (water < waterThreshold)
        {
            // serves as a method to see if water is within reach
            bool drank = checkWater(withinReach);
            if (!drank) seek("water", false);
        }
        // If hunger is below the threshold, seek food
        else if (hunger < hungerThreshold)
        {
            // serves as a method to see if water is within reach
            bool eaten = checkHunger(withinReach);
            if (!eaten) seek("food", false);
        }

        // energy is less than 30% of maxEnergy
        else if (energy < maxEnergy * 0.3f || sleep)
        {
            sleep = checkShelter(withinReach);
            if (!sleep) seek("shelter", false);
        }

        else wander(false, "none");  // otherwise, wander randomly

        checkSleep();
        checkTired();
        setSpeed();

        if (checkReproductionConditions() && !sleep) checkReproduction();
        else if (checkReproductionConditions() && sleep) checkReproductionShelter();
    }

    void move()
    {
        Vector2 target = new Vector2(moveX, moveY);
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    void setSpeed()
    {
        Vector2 target = new Vector2(moveX, moveY);
        if (rb == null) Debug.LogError("RB not found");
        else if (target == null) Debug.LogError("Target not found");
        else speed = Vector2.Distance(rb.position, target);
    }

    // wander function to wander randomly
    void wander(bool seeking, string tag)
    {
        float angle = Random.Range(0f, 2 * Mathf.PI);
        float range = movement * tiredFactor[0];
        if (seeking)
        {
            moveX = transform.position.x + Mathf.Cos(angle) * range;
            moveY = transform.position.y + Mathf.Sin(angle) * range;
            if (tag == "shelter")
            {
                float energyFactor = energy / maxEnergy;
                // restore some missing energy
                energy += (maxEnergy - energy) * energyRestorationPercent;
                moveX = transform.position.x + Mathf.Cos(angle) * range * energyFactor;
                moveY = transform.position.y + Mathf.Sin(angle) * range * energyFactor;
            }
        }
        else
        {
            // the lowered amount of movement is equivalent to how much energy blob is missing
            // blob gains more energy when stats are being lowered more
            float energyFactor = energy / maxEnergy;
            // restore 20% of missing energy
            energy += (maxEnergy - energy) * energyRestorationPercent;
            moveX = transform.position.x + Mathf.Cos(angle) * range * energyFactor;
            moveY = transform.position.y + Mathf.Sin(angle) * range * energyFactor;
        }
    }

    // wander function used for targets
    void wander(float angle, bool farfromTarget)
    {
        float range = movement * tiredFactor[0];
        if ((farfromTarget && energy > movementEnergyCost[1]) || (energy > movementEnergyCost[0] && (water < movementEnergyCost[3] || hunger < movementEnergyCost[3])))
        {
            energy -= movementEnergyCost[0];
            moveX = transform.position.x + Mathf.Cos(angle) * range * (1 + movementEnergyCost[2]);
            moveY = transform.position.y + Mathf.Sin(angle) * range * (1 + movementEnergyCost[2]);
        }
        else
        {
            moveX = transform.position.x + Mathf.Cos(angle) * range;
            moveY = transform.position.y + Mathf.Sin(angle) * range;
        }
    }

    void seek(string tag, bool enhancedSearch)
    {
        Collider2D[] colliders;
        if (enhancedSearch)
        {
            colliders = Physics2D.OverlapCircleAll(transform.position, sight * (1 + sightEnergyCost[2]));
        }
        else { colliders = Physics2D.OverlapCircleAll(transform.position, sight); } 
        
        Collider2D closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(tag))
            {
                if (tag == "shelter")
                {
                    shelterScript shelter = collider.GetComponent<shelterScript>();
                    if (size < shelter.capacity && size < shelter.sizeCondition)
                    {
                        float dist = Vector2.Distance(transform.position, collider.transform.position);
                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            closest = collider;
                        }
                    }
                    else continue;
                }
                else
                {
                    float dist = Vector2.Distance(transform.position, collider.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closest = collider;
                    }
                }
            }
        }

        if (closest != null)
        {
            Vector2 direction = closest.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x);
            // check if destination is within movement range
            if (closestDistance >= movement) wander(angle, true);
            else wander(angle, false);
        }
        else
        {
            if (energy > sightEnergyCost[1] && !enhancedSearch)
            {
                energy -= sightEnergyCost[0];
                seek(tag, true);
            }
            else wander(true, tag);
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
                    if (energy < 0f) { energy = 0f; } // Prevent negative energy
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

    bool checkShelter(Collider2D[] withinReach)
    {
        foreach (Collider2D collider in withinReach)
        {
            if (collider.CompareTag("shelter"))
            {
                shelter = collider.GetComponent<shelterScript>();
                if (size < shelter.capacity && size < shelter.sizeCondition)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool checkReproductionConditions()
    {
        if (hunger > reproThreshold && water > reproThreshold && reproduced == false)
        {
            return true;
        }
        return false;
    }

    void checkSleep()
    {
        if (sleep)
        {
            sleepFactor[0] = sleepFactor[1];
            energy += maxEnergy * energyRestorationPercent;
            if (energy > maxEnergy) energy = maxEnergy; // Cap the energy at maxEnergy
            if (!hidden)
            {
                hidden = true;
                ren.enabled = false;
                rb.simulated = false;
                circleCollider.enabled = false;
                shelter.enter(size, this);
            }
        }
        else
        {
            sleepFactor[0] = 1f;
            if (hidden)
            {
                hidden = false;
                ren.enabled = true;
                rb.simulated = true;
                circleCollider.enabled = true;
                shelter.exit(size, this);
                shelter = null;
                gm[0].checkScene(this);
            }
        }
        // sleep recovers energyRestorationPercent of maxEnergy
    }
    void checkTired()
    {
        if (energy <= 0) tiredFactor[0] = tiredFactor[1];
        else tiredFactor[0] = 1f;
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
                    gm[0].StartCoroutine(
                        gm[0].blobReproduction(
                            returnPosition(),
                            otherBlob.returnPosition(),
                            returnStats(),
                            otherBlob.returnStats()
                        )
                    );
                    reproduced = true;
                    water -= 20.0f;
                    hunger -= 20.0f;
                    otherBlob.water -= 20.0f;
                    otherBlob.hunger -= 20.0f;
                    return;
                }
            }
        }
    }

    void checkReproductionShelter()
    {
        foreach (blobScript blob in shelter.blobsInside)
        {
            if (blob.checkReproductionConditions() && blob != this)
            {
                Debug.Log("Success");
                gm[0].StartCoroutine(
                    gm[0].blobReproduction(
                        returnPosition(),
                        blob.returnPosition(),
                        returnStats(),
                        blob.returnStats()
                    )
                );
                reproduced = true;
                water -= 20.0f;
                hunger -= 20.0f;
                blob.water -= 20.0f;
                blob.hunger -= 20.0f;
                return;
            }
        }
    }

    float[] returnStats()
    {
        float[] stats = new float[5];
        stats[0] = movement;
        stats[1] = sight;
        stats[2] = reach;
        stats[3] = incubationTime;
        stats[4] = size;
        return stats;
    }

    public void setStats(float[] stats)
    {
        movement = stats[0];
        sight = stats[1];
        reach = stats[2];
        incubationTime = stats[3];
        size = stats[4];

        float moveFactor = movement / baseMovement;
        float sightFactor = sight / baseSight;
        float reachFactor = reach / baseReach;
        float sizeFactor = size / baseSize;

        // set blob size
        circleCollider.radius = size;
        transform.localScale = new Vector3(0.5f * sizeFactor, 0.5f * sizeFactor, 1f);

        hungerDecayRate *= moveFactor * sizeFactor;
        waterDecayRate *= moveFactor * sizeFactor;
        energyDecayRate *= sightFactor * sizeFactor;

        hungerThreshold *= sizeFactor;
        waterThreshold *= sizeFactor;
        reproThreshold *= sizeFactor;

        maxWater *= sizeFactor;
        maxHunger *= sizeFactor;
        maxEnergy *= sightFactor * sizeFactor;

        reach *= sizeFactor;
        movement *= sizeFactor;
        incubationTime *= sizeFactor;
    }

    float[] returnPosition()
    {
        float[] position = new float[2];
        position[0] = transform.position.x;
        position[1] = transform.position.y;
        return position;
    }
}

