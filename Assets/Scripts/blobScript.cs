using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class blobScript : MonoBehaviour
{
    Rigidbody2D rb;
    public CircleCollider2D circleCollider;
    Renderer ren;
    gameManager[] gm;
    shelterScript shelter;

    // basic stats
    public float movement = 2f, sight = 4f, reach = 0.5f, size = 0.64f, turnTime = 1.0f;
    public float energy = 100f, hunger = 55f, water = 55f;

    // target stats
    float moveX = 0.0f, moveY = 0.0f, speed = 0.0f;

    // hunger stats
    float maxHunger = 100.0f, hungerDecayRate = 2.5f, hungerThreshold = 35.0f;
    // water stats
    float maxWater = 100.0f, waterDecayRate = 7f, waterThreshold = 35.0f;
    // reproduction stats
    float incubationTime = 10f, reproThreshold = 55f;
    // energy stats
    float maxEnergy = 100f, energyDecayRate = 2f, energyRestorationPercent = 0.15f;

    // tired and sleep modifiers have the current and anchor stat
    // tired reduces move by 30%, sleep reduces decay by 50%
    float[] tiredFactor = { 1f, 0.8f }, sleepFactor = { 1f, 0.65f };

    // energy costs have a cost, a threshold, and the boost
    float[] sightEnergyCost = { 10f, 20f, 0.25f };
    float[] decayEnergyCost = { 7f, 10f, 0.20f }; // reduces decay by 20%
    float[] movementEnergyCost = { 15f, 40f, 0.15f, 10 }; // increases move by 25%
    // fourth energy cost is the sprint condition for water and hunger

    float predation = 0.07f;
    int[] predationTimer = { 0, 3 };
    public blobScript prey;
    public List<blobScript> predators;
    List<bool> noticedPredators;

    bool reproduced = false, sleep = false, hidden = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectsByType<gameManager>(FindObjectsSortMode.None);
        ren = GetComponent<Renderer>();
        shelter = null;
        wander(false, "none");
        StartCoroutine(TurnLoop());
    }

    IEnumerator TurnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(turnTime);
            turn();
        }
    }

    // Update function was deleted, but can be put back if needed for non-rigidbody functions
    // FixedUpdate is called at a fixed interval
    void FixedUpdate()
    {
        if (prey != null)
        {
            wander(prey.transform.position);
            setSpeed();
        }
        move();
    }

    void turn()
    {
        decay(0.25f);
        reproduced = false;
        Collider2D[] withinReach = Physics2D.OverlapCircleAll(transform.position, reach);
        int numPredators = noticePredators();

        // blobs that have a prey will lock on for three turns
        if (predationTimer[0] > 0)
        {
            if (prey != null)
            {
                predationTimer[0]--;
                checkHungerBlob(withinReach);
            }
            else
            {
                predationTimer[0] = 0;
            }

        }
        // if blob is being targeted and water or hunger isn't too low
        else if (numPredators > 0 && water > maxWater * 0.2 && hunger > maxHunger * 0.2)
        {
            sleep = checkShelter(withinReach);
            if (!sleep) seek("shelter", false, true);
        }
        // check water first
        if (water < waterThreshold)
        {
            // serves as a method to see if water is within reach
            bool drank = checkWater(withinReach);
            if (!drank) seek("water", false, false);
        }
        // If hunger is below the threshold, seek food
        else if (hunger < hungerThreshold * 2)
        {
            // serves as a method to see if food is within reach
            bool eaten = checkHunger(withinReach);
            if (!eaten && hunger < hungerThreshold) seek("food", false, false);
        }
        // energy is less than 30% of maxEnergy
        else if (energy < maxEnergy * 0.3f || sleep)
        {
            sleep = checkShelter(withinReach);
            if (!sleep) seek("shelter", false, false);
        }

        else wander(false, "none");  // otherwise, wander randomly

        if (energy > maxEnergy) energy = maxEnergy;
        if (energy < 0) energy = 0; 

        checkSleep();
        checkTired();
        setSpeed();

        if (checkReproductionConditions() && !sleep) checkReproduction();
        else if (checkReproductionConditions() && sleep) checkReproductionShelter();
    }

    public void decay(float turnFactor)
    {
        checkDeath();
        hunger -= hungerDecayRate * sleepFactor[0] * turnFactor;
        water -= waterDecayRate * sleepFactor[0] * turnFactor;
        if (!sleep) energy -= energyDecayRate * turnFactor;
        sleep = false;

        if (energy > decayEnergyCost[1] && !sleep)
        {
            energy -= decayEnergyCost[0];
            // hunger and water increase by a percentage of decayRate
            hunger += hungerDecayRate * decayEnergyCost[2] * turnFactor;
            water += waterDecayRate * decayEnergyCost[2] * turnFactor;
        }

        if (energy < 0f) { energy = 0f; } // Prevent negative energy
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
        else speed = Vector2.Distance(rb.position, target) / turnTime;
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
    // this function makes no sense???
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
            float energyFactor = 1 - (Vector2.Distance(rb.position, new Vector2(moveX, moveY)) / range);
            Debug.Log(energyFactor);
            energy += (maxEnergy - energy) * energyFactor;
        }
    }

    void wander(Vector3 target)
    {
        float range = movement * tiredFactor[0];
        float distance = Vector2.Distance(transform.position, target);
        bool farFromTarget = range < distance;

        if ((farFromTarget && energy > movementEnergyCost[1]) || (energy > movementEnergyCost[0] && (water < movementEnergyCost[3] || hunger < movementEnergyCost[3])))
        {
            energy -= movementEnergyCost[0];
            if (range * movementEnergyCost[2] < distance)
            {
                Vector2 direction = target - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x);
                moveX = transform.position.x + Mathf.Cos(angle) * range * (1 + movementEnergyCost[2]);
                moveY = transform.position.y + Mathf.Sin(angle) * range * (1 + movementEnergyCost[2]);
            }
            else
            {
                moveX = target.x;
                moveY = target.y;
            }
        }
        else
        {
            if (range < distance)
            {
                Vector2 direction = target - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x);
                moveX = transform.position.x + Mathf.Cos(angle) * range;
                moveY = transform.position.y + Mathf.Sin(angle) * range;
            }
            else
            {
                 moveX = target.x;
                moveY = target.y;
                energy += (maxEnergy - energy) / (1 - (distance / range));
            }
        }
    }

    void seek(string tag, bool enhancedSearch, bool targeted)
    {
        Collider2D[] colliders;

        if (enhancedSearch)
        {
            colliders = Physics2D.OverlapCircleAll(transform.position, sight * (1 + sightEnergyCost[2]));
        }
        else { colliders = Physics2D.OverlapCircleAll(transform.position, sight); }

        bool targetBlobs = false;
        blobScript blobPrey = null;
        Collider2D closestBlob = null;
        float closestDistanceBlob = float.MaxValue;

        Collider2D closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            bool detectBlob = tag == "food" && collider.CompareTag("blob");
            float dist = Vector2.Distance(transform.position, collider.transform.position);

            if (collider.CompareTag(tag) || detectBlob)
            {
                // look for shelters, either for energy or to hide from blobs
                if (tag == "shelter")
                {
                    shelterScript shelter = collider.GetComponent<shelterScript>();
                    if (size < shelter.capacity && size < shelter.sizeCondition)
                    {
                        if (dist < closestDistance)
                        {
                            // if the blob is being targeted by another blob
                            if (targeted)
                            {
                                Vector3 shelterPosition = collider.transform.position;
                                float predatorDistanceToShelter = float.MaxValue;
                                int i = 0;
                                foreach (blobScript p in predators)
                                {
                                    // if the prey knows they're being spotted
                                    if (noticedPredators[i])
                                    {
                                        // find closest predator to the shelter
                                        float predatorDistance = Vector2.Distance(p.transform.position, shelterPosition);
                                        if (dist < predatorDistance) predatorDistanceToShelter = predatorDistance;
                                    }
                                    i++;
                                }
                                // if distance of self to shelter is less than distance of predator to shelter
                                // set that shelter as the target (if it's closer than previous target)
                                if (dist < predatorDistanceToShelter && dist < closestDistance)
                                {
                                    closest = collider;
                                    closestDistance = dist;
                                }
                            }
                            // simply look for the closest shelter
                            else
                            {
                                closestDistance = dist;
                                closest = collider;
                            }
                        }
                    }
                    else continue;
                }
                // if a blob is detected
                else if (detectBlob)
                {
                    if (dist < closestDistanceBlob)
                    {
                        closestDistanceBlob = dist;
                        closestBlob = collider;
                        blobPrey = collider.GetComponent<blobScript>();
                    }
                }
                else
                {
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closest = collider;
                    }
                }
            }
        }

        if (tag == "food" && Random.Range(0f, 100f) < predation) targetBlobs = true;

        // if this blob is targeting another blob
        if (targetBlobs && closestBlob != null)
        {
            closest = closestBlob;
            closestDistance = closestDistanceBlob;
            // set targets
            blobPrey.addPredators(this);
            if (prey != null) prey.removePredators(this);
            prey = blobPrey;
            predationTimer[0] = predationTimer[1];
        }
        else
        {
            // if the blob is targeting a non-blob entity, then remove itself from the predator list
            if (prey != null) prey.removePredators(this);
            prey = null;
        }

        if (closest != null) wander(closest.transform.position);
        else
        {
            if (energy > sightEnergyCost[1] && !enhancedSearch)
            {
                energy -= sightEnergyCost[0];
                seek(tag, true, prey);
            }
            else
            {
                // if the blob is targeted and a shelter cannot be found
                if (targeted)
                {
                    int numPredators = predators.Count;
                    float avgAngle = 0f;
                    int predatorsSpotted = 0;
                    for (int i = 0; i < numPredators; i++)
                    {
                        // if the blob spots the predators
                        if (noticedPredators[i])
                        {
                            Vector2 direction = predators[i].transform.position - transform.position;
                            float angle = Mathf.Atan2(direction.y, direction.x) + Mathf.PI;
                            avgAngle += angle;
                            predatorsSpotted++;
                        }
                    }
                    avgAngle /= predatorsSpotted;
                    wander(avgAngle, true);
                }
                else wander(true, tag);
            }
        }
    }

    void checkDeath()
    {
        if (hunger <= 0.0f || water <= 0.0f)
        {
            gm[0].removeBlob(this);
        }
    }

    bool checkHunger(Collider2D[] withinReach)
    {
        float maxHungerValue = 0.0f;
        object food = null;
        // if blob is within reach of food, eat food
        foreach (Collider2D collider in withinReach)
        {
            if (collider.CompareTag("food"))
            {
                // If the blob is close enough to the food, eat it
                if (collider.TryGetComponent<bushScript>(out var bush) && bush.numFruits > 0)
                {
                    float hungerValue = bush.returnHungerValue();
                    if (maxHungerValue < hungerValue)
                    {
                        maxHungerValue = hungerValue;
                        food = bush;
                    }
                }
            }
            else if (collider.CompareTag("blob"))
            {
                if (collider.TryGetComponent<blobScript>(out var otherBlob) && hunger < maxHunger * predation)
                {
                    float hungerValue = otherBlob.size * 100f;
                    if (maxHungerValue < hungerValue)
                    {
                        maxHungerValue = hungerValue;
                        food = otherBlob;
                    }
                }
            }
        }
        if (food is blobScript blobFood)
        {
            Debug.Log("ate");
            gm[0].removeBlob(blobFood);
        }
        else if (food is bushScript bushFood) bushFood.eaten();

        if (maxHungerValue > 0.0f)
        {
            hunger += maxHungerValue;
            if (hunger > maxHunger) hunger = maxHunger; // Cap the hunger at maxHunger
            return true;
        }

        return false;
    }

    void checkHungerBlob(Collider2D[] withinReach)
    {

        foreach (Collider2D collider in withinReach)
        {
            if (collider.CompareTag("blob"))
            {
                if (collider.TryGetComponent<blobScript>(out var otherBlob) == prey)
                {
                    float hungerValue = otherBlob.size * 100f;
                    hunger += hungerValue;
                    if (hunger > maxHunger) hunger = maxHunger;
                    predationTimer[0] = 0;
                    return;
                }
            }
        }
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
                    foreach (blobScript p in predators)
                    {
                        p.prey = null;
                        removePredators(p);
                    }
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, reach + 1.5f);

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

    public float[] returnStats()
    {
        float[] stats = new float[16];
        stats[0] = movement;
        stats[1] = sight;
        stats[2] = reach;
        stats[3] = incubationTime;
        stats[4] = size;
        stats[5] = turnTime;
        stats[6] = hungerDecayRate;
        stats[7] = waterDecayRate;
        stats[8] = energyDecayRate;
        stats[9] = maxHunger;
        stats[10] = maxWater;
        stats[11] = maxEnergy;
        stats[12] = hungerThreshold;
        stats[13] = waterThreshold;
        stats[14] = reproThreshold;
        stats[15] = predation;
        return stats;
    }

    public void setStats(float[] stats, float[] baseStats)
    {
        movement = stats[0];
        sight = stats[1];
        reach = stats[2];
        incubationTime = stats[3];
        size = stats[4];
        turnTime = stats[5];
        hungerDecayRate = stats[6];
        waterDecayRate = stats[7];
        energyDecayRate = stats[8];
        maxHunger = stats[9];
        maxWater = stats[10];
        maxEnergy = stats[11];
        hungerThreshold = stats[12];
        waterThreshold = stats[13];
        reproThreshold = stats[14];
        predation = stats[15];

        float moveFactor = movement / baseStats[0];
        float sightFactor = sight / baseStats[1];
        float reachFactor = reach / baseStats[2];
        float sizeFactor = size / baseStats[4];
        float turnFactor = turnTime / baseStats[5];

        float basicStatSizeFactor = Mathf.Pow(sizeFactor, 1.2f);

        // set blob size
        circleCollider.radius = size;
        transform.localScale = new Vector3(0.5f * sizeFactor, 0.5f * sizeFactor, 1f);

        hungerDecayRate *= moveFactor * sizeFactor * (1 / Mathf.Pow(turnFactor, 2f));
        waterDecayRate *= moveFactor * sizeFactor * (1 / Mathf.Pow(turnFactor, 2f));
        energyDecayRate *= sightFactor * sizeFactor * (1 / Mathf.Pow(turnFactor, 2f));

        hungerThreshold *= sizeFactor * turnFactor;
        waterThreshold *= sizeFactor * turnFactor;
        reproThreshold *= sizeFactor;

        maxWater *= sizeFactor;
        maxHunger *= sizeFactor;
        maxEnergy *= 1 / sightFactor * sizeFactor;

        reach *= sizeFactor;
        movement *= sizeFactor * Mathf.Pow(turnFactor, 1.5f);
        incubationTime *= sizeFactor * turnFactor;
        turnTime *= reachFactor * moveFactor;

    }

    float[] returnPosition()
    {
        float[] position = new float[2];
        position[0] = transform.position.x;
        position[1] = transform.position.y;
        return position;
    }

    public void addPredators(blobScript predator)
    {
        predators.Add(predator);
        noticedPredators.Add(false);
    }

    public void removePredators(blobScript predator)
    {
        int index = predators.IndexOf(predator);
        predators.RemoveAt(index);
        noticedPredators.RemoveAt(index);
    }

    int noticePredators()
    {
        for (int i = 0; i < predators.Count; i++)
        {
            float distance = Vector2.Distance(rb.position, predators[i].rb.position);
            float rolledNumber = (Random.Range(0f, 1f) + Random.Range(0f, 1f)) / 2;
            if (distance > sight || (rolledNumber > distance / sight && !noticedPredators[i])) noticedPredators[i] = false;
            else noticedPredators[i] = true;
        }
        return predators.Count;
    }
}

