using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Name: Naim Heredia
 * Purpose: Chase State for the Enemies AI, the enemies will move towards the player and stop based on a specific point around the player (slot manager)
 * 
*/

public class ChaseState : FSMState
{
    EnemyController npcTankControl = null;
    EnemyFSM enemyStateControl = null;
    EnemyShooting enemyShooting = null;

    SlotManager playerSlot = null;

    float intervalTime;
    float elapsedTime;
    int availableSlotIndex;

    Transform npc;
    Transform player;

    public ChaseState(Transform[] wp, EnemyController npcTank)
    {
        stateID = FSMStateID.Chasing;
        npcTankControl = npcTank;
        enemyStateControl = npcTank.enemyFSMControl;

        curRotSpeed = 2.0f;
        curSpeed = 2.0f;

        intervalTime = 1.0f;
        elapsedTime = 0.0f;
        availableSlotIndex = -1;

        enemyShooting = npcTankControl.enemyShooting;

        playerSlot = enemyStateControl.GetPlayerSlot();

        npc = npcTankControl.gameObject.transform;
        player = enemyStateControl.GetPlayerTransform();
    }

    public override void EnterStateInit()
    {
        playerSlot.ClearSlots(enemyStateControl.gameObject);
        availableSlotIndex = playerSlot.ReserveSlotAroundObject(enemyStateControl.gameObject);

        if (availableSlotIndex != -1) 
        {
            destPos = playerSlot.GetSlotPosition(availableSlotIndex);
        }
        else
        {
            destPos = Vector3.zero;
        }

        if (enemyShooting)
        {
            enemyShooting.Firing = false;
        }

        elapsedTime = 0.0f;
        enemyStateControl.navAgent.speed = 3.5f;
    }

    //Reason
    public override void Reason()
    {
        elapsedTime += Time.deltaTime;


        
        if(elapsedTime >= intervalTime)
        {
            elapsedTime = 0.0f;

            playerSlot.ReleaseSlot(availableSlotIndex, enemyStateControl.gameObject);
            availableSlotIndex = playerSlot.ReserveSlotAroundObject(enemyStateControl.gameObject);

            if (availableSlotIndex != -1)
            {
                destPos = playerSlot.GetSlotPosition(availableSlotIndex);
            }
            else
            {
                destPos = Vector3.zero;
            }
        }

        // Speed
        // Increase speed based on player skill
        if (WorldData.Instance.PlayerSkill == WorldData.SkillClassifierType.EXCELLENT || WorldData.Instance.PlayerSkill == WorldData.SkillClassifierType.VERY_GOOD)
        {
            
            enemyStateControl.navAgent.speed = 7.5f;
        }


        // Transitions

        // Check if enemy Death
        if (npcTankControl.IsDead)
        {
            enemyStateControl.PerformTransition(Transition.NoHealth);
        }
        else if (npcTankControl.HitPoints < npcTankControl.maxHitPoints / 2)
        {
            enemyStateControl.PerformTransition(Transition.LowHealth);
        }


        //Check if player is within chase range
        if (IsInCurrentRange(npc, player.position, EnemyFSM.CHASE_DIST)) 
        {
            // Check if we reached slot
            if (IsInCurrentRange(npc, destPos, EnemyFSM.SLOT_DIST)) 
            {
                enemyStateControl.PerformTransition(Transition.ReachedPlayer);
            }
        }
        else
        {
            // Patrol
            enemyStateControl.PerformTransition(Transition.Patrol);

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
