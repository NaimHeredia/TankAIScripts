using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Name: Naim Heredia
 * Purpose: Cover State for the Enemies AI, using a slot manager the enemies will go to specific points to take cover from player
 * 
*/

public class CoverState : FSMState
{
    EnemyController npcTankControl = null;
    EnemyFSM enemyStateControl = null;
    EnemyShooting enemyShooting = null;

    SlotManager coverSlot = null;

    float intervalTime;
    float elapsedTime;
    int availableSlotIndex;

    Transform npc;
    Transform player;

    public CoverState(Transform[] wp, EnemyController npcTank)
    {
        stateID = FSMStateID.TakinCover;
        npcTankControl = npcTank;
        enemyStateControl = npcTank.enemyFSMControl;

        curRotSpeed = 2.0f;
        curSpeed = 2.0f;

        intervalTime = 1.0f;
        elapsedTime = 0.0f;
        availableSlotIndex = -1;

        enemyShooting = npcTankControl.enemyShooting;

        coverSlot = enemyStateControl.GetCoverSlot();

        npc = npcTankControl.gameObject.transform;
        player = enemyStateControl.GetPlayerTransform();
    }

    public override void EnterStateInit()
    {

        coverSlot.ClearSlots(enemyStateControl.gameObject);
        availableSlotIndex = coverSlot.ReserveSlotAroundObject(enemyStateControl.gameObject);

        if (availableSlotIndex != -1)
        {
            destPos = coverSlot.GetSlotPosition(availableSlotIndex);
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
        // DestPos

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= intervalTime)
        {
            elapsedTime = 0.0f;

            coverSlot.ReleaseSlot(availableSlotIndex, enemyStateControl.gameObject);
            availableSlotIndex = coverSlot.ReserveSlotAroundObject(enemyStateControl.gameObject);

            if (availableSlotIndex != -1)
            {
                destPos = coverSlot.GetSlotPosition(availableSlotIndex);
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
        // Death
        if (npcTankControl.IsDead)
        {
            enemyStateControl.PerformTransition(Transition.NoHealth);
        }

        //Patrol

        // If player is range and our hp is higher than 75%
        if (IsInCurrentRange(npc, player.position, EnemyFSM.CHASE_DIST) && npcTankControl.HitPoints >= npcTankControl.maxHitPoints * 0.75)
        {
            // Chase
            enemyStateControl.PerformTransition(Transition.ResetAttackPos);
        }
        // If player is out of range and our hp is higher than 75%
        else if (!IsInCurrentRange(npc, player.position, EnemyFSM.CHASE_DIST) && npcTankControl.HitPoints >= npcTankControl.maxHitPoints * 0.75)
        {
            // Patrol
            enemyStateControl.PerformTransition(Transition.Patrol);
        }
        // If player is in range and our hp is lower than 75%
        else if (IsInCurrentRange(npc, player.position, EnemyFSM.CHASE_DIST) && npcTankControl.HitPoints < npcTankControl.maxHitPoints * 0.75)
        {
            // Flee
            enemyStateControl.PerformTransition(Transition.LowHealth);
        }
        
    }

    //Act
    public override void Act()
    {
        // Movement

        // Rotate
        Quaternion targetRotation = Quaternion.LookRotation(destPos - enemyStateControl.transform.position);
        enemyStateControl.transform.rotation = Quaternion.Slerp(enemyStateControl.transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        enemyStateControl.turret.rotation = Quaternion.Slerp(enemyStateControl.turret.rotation, enemyStateControl.transform.rotation, Time.deltaTime * curRotSpeed);

        // Move
        enemyStateControl.navAgent.destination = destPos;

        // Increase Health

        if (IsInCurrentRange(npc, destPos, EnemyFSM.SLOT_DIST))
        {
            npcTankControl.IncreaseHealth(5);
        }        
    }
}
