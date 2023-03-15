using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * Name: Naim Heredia
 * Purpose: Flee State for the Enemies AI, Enemies will move away from the player
 * 
*/
public class FleeState : FSMState
{
    EnemyController npcTankControl = null;
    EnemyFSM enemyStateControl = null;
    EnemyShooting enemyShooting = null;

    float intervalTime;
    float elapsedTime;
    
    Transform npc;
    Transform player;

    public FleeState(Transform[] wp, EnemyController npcTank)
    {
        stateID = FSMStateID.Fleeing;
        npcTankControl = npcTank;
        enemyStateControl = npcTank.enemyFSMControl;

        curRotSpeed = 3.0f;
        curSpeed = 3.0f;

        intervalTime = 0.0f;
        elapsedTime = 0.0f;

        enemyShooting = npcTankControl.enemyShooting;

        enemyShooting = npcTankControl.enemyShooting;

        npc = npcTankControl.gameObject.transform;
        player = enemyStateControl.GetPlayerTransform();

        if (enemyShooting)
        {
            enemyShooting.Firing = false;
        }

        enemyStateControl.navAgent.speed = 3.5f;
    }

    public override void EnterStateInit()
    {
        elapsedTime = 0.0f;
    }

    //Reason
    public override void Reason()
    {
        // Get Destination
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= intervalTime)
        {
            elapsedTime = 0.0f;

            Vector3 direction = npcTankControl.transform.position - player.transform.position;

            destPos = npcTankControl.transform.position + direction;
            
        }

        // Speed
        // Increase speed based on player skill
        if (WorldData.Instance.PlayerSkill == WorldData.SkillClassifierType.EXCELLENT || WorldData.Instance.PlayerSkill == WorldData.SkillClassifierType.VERY_GOOD)
        {            
            enemyStateControl.navAgent.speed = 7.5f;
        }

        // Transitions
        if (npcTankControl.IsDead)
        {
            enemyStateControl.PerformTransition(Transition.NoHealth);
        }

        
        // If player is out of range and our hp is low than 75%
        else if (!IsInCurrentRange(npc, player.position, EnemyFSM.CHASE_DIST) && npcTankControl.HitPoints < npcTankControl.maxHitPoints * 0.75)
        {
            // Cover
            enemyStateControl.PerformTransition(Transition.TakeCover);
        }
    }

    //Act
    public override void Act()
    {

        // Rotate
        Quaternion targetRotation = Quaternion.LookRotation(destPos - enemyStateControl.transform.position);
        enemyStateControl.transform.rotation = Quaternion.Slerp(enemyStateControl.transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        enemyStateControl.turret.rotation = Quaternion.Slerp(enemyStateControl.turret.rotation, enemyStateControl.transform.rotation, Time.deltaTime * curRotSpeed);

        // Move
        enemyStateControl.navAgent.destination = destPos;
    }
}
