/// <summary>
/// Component to control the enemy with respect to the game state.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : InteractiveBody
{
    public Text debugText;

    [Tooltip("Set the Finite State Machine Script")]
    public EnemyFSM enemyFSMControl;
    
    [Tooltip("Set the Enemy's Shooting Script")]
    public EnemyShooting enemyShooting;

    public int maxHitPoints = 100;

    protected override void Update()
    {
        if (CanvasDebugTransform)
        {
            debugText.text = "E " + charID + " " + enemyFSMControl.GetStateString();
        }
    }

    /// <summary>
    /// OnGameOver.
    /// </summary>
    protected override void OnGameOver()
    {
        // Enemy should not destroy itself in this case.
    }

    /// <summary>
    /// Called when to reset enemy after game has ended.
    /// </summary>
    public override void ResetCharacter()
    {
        base.ResetCharacter();
        transform.position = respawnPos;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        hitPoints = maxHitPoints;
    }

    void OnEnable()
    {
        collision.enabled = true;
        enemyFSMControl.enabled = true;
        enemyShooting.enabled = true;
        isDead = false;
    }

    void OnDisable()
    {
        StopAllCoroutines(); // if ghost is at home base in restore, stop because of reset
      
        collision.enabled = false;
        enemyFSMControl.enabled = false;
        enemyShooting.enabled = false;
    }

    /// <summary>
    /// ApplyDamage after enemy collides with player. Action depends on state of player (NORMAL or POWERED)
    /// </summary>
    public override void ApplyDamage(int damageAmount, DamageType damageType)
    {
        hitPoints = Mathf.Clamp(hitPoints - damageAmount, 0, MAX_HEALTH);

        if (damageAmount == -1)
        {
            hitPoints = 0;
        }

        if (hitPoints <= 0 || damageAmount == -1) //force death
        {
            if (!isDead)
            {
                StartCoroutine(Death());
            }
        }
    }

    public override void IncreaseHealth(int amount)
    {
        hitPoints = Mathf.Clamp(hitPoints + amount, 0, maxHitPoints);
        //if (hitPoints > maxHitPoints)
        //{
        //    hitPoints = maxHitPoints;
        //}
    }

    /// <summary>
    /// Called on start of death.
    /// </summary>
    protected override void DeathStart()
    {
        collision.enabled = false;  //disable collider so it can't kill other objects
        enemyFSMControl.enabled = false;
        enemyShooting.enabled = false;
    }

    /// <summary>
    /// Called on completion of death animation
    /// Uses to make any changes in between animation completion and death completion
    /// typically used for disabling movement 
    /// </summary>
    protected override void DeathAnimationComplete()
    {
    }

    /// <summary>
    /// Called on completion of death, most Bodies will want to destroy 
    /// themselves at this point, its virtual so can override.
    /// </summary>
    protected override void DeathComplete()
    {
       // Destroy(gameObject);
    }
}
