/// <summary>
/// Controls the enemy shooting.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting  : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform fireTransform;
    public float rateOfFire = 1.0f;         // The rate at which shells will be fired (in secs).
    public float launchForce = 15.0f;

    public bool Firing                          // Allows manager to set when this tank can fire.
    {
        get { return bFiring; }
        set { bFiring = value; }
    }
    
    private bool bFiring;                       // Controls whether take is allowed to fire.
    private float fireTimer;                  // Controls the fire rate;

    private void OnEnable()
    {
        fireTimer = 0.0f;
        bFiring = false;
    }

    private void OnDisable()
    {
        fireTimer = 0.0f;
        bFiring = false;
    }

    private void Start()
    {
        // When the tank is turned on, reset the launch force and the UI
        fireTimer = 0.0f;
        bFiring = false;
    }


    private void Update()
    {
        //Checking if we are able to fire (was I told to fire?)
        if (bFiring)
        {
            // Update the fire time.
            fireTimer += Time.deltaTime;

            // Check if it is time to fire... 
            if (fireTimer > rateOfFire)
            {
                Fire();
                fireTimer = 0.0f;
            }
        }
    }

    private void Fire()
    {
        Rigidbody bulletRB;
        // Set the fired flag so only Fire is only called once.

        // Create an instance of the bullet and store a reference to it's rigidbody.
        GameObject bullet =
            Instantiate(bulletPrefab, fireTransform.position, fireTransform.rotation) as GameObject;
        bulletRB = bullet.GetComponent<Rigidbody>();
        if (bulletRB)
        {
            bulletRB.velocity = launchForce * fireTransform.forward;
        }
    }

}
