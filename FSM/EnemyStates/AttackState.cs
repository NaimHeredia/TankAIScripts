using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Name: Naim Heredia
 * Purpose: Attack State for the Enemies AI, enemies will shoot in the direction of the player
 * 
*/

public class AttackState : FSMState
{
    EnemyShooting enemyShooting = null;
    EnemyController npcTankControl = null;
    EnemyFSM enemyStateControl = null;

    SlotManager playerSlot = null;

    float intervalTime;
    float elapsedTime;
    int availableSlotIndex;

    Transform npc;
    Transform player;

    public AttackState(Transform[] wp, EnemyController npcTank)
    {
        waypoints = wp;
        stateID = FSMStateID.Attacking;
        npcTankControl = npcTank;
        enemyStateControl = npcTank.enemyFSMControl;

        curRotSpeed = 1.0f;
        curSpeed = 2.0f;

        intervalTime = 5.0f;
        elapsedTime = 0.0f;
        availableSlotIndex = -1;

        playerSlot = enemyStateControl.GetPlayerSlot();

        enemyShooting = npcTankControl.enemyShooting;

        npc = npcTankControl.gameObject.transform;
        player = enemyStateControl.GetPlayerTransform();
    }

    public override void EnterStateInit()
    {

        elapsedTime = 0.0f;

        if (enemyShooting)
        {
            enemyShooting.Firing = false;
        }

        enemyStateControl.navAgent.speed = 3.5f;
    }

    //Reason
    public override void Reason()
    {
        // Movement

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= intervalTime)
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

            if(enemyShooting)
            {
                enemyShooting.Firing = false;
            }

            enemyStateControl.PerformTransition(Transition.ResetAttackPos);
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
            if (enemyShooting)
            {
                enemyShooting.Firing = false;
            }

            enemyStateControl.PerformTransition(Transition.NoHealth);
        }
        else if (npcTankControl.HitPoints < npcTankControl.maxHitPoints / 2)
        {
            if (enemyShooting)
            {
                enemyShooting.Firing = false;
            }

            enemyStateControl.PerformTransition(Transition.LowHealth);
        }

        // Check if we reached slot
        if (IsInCurrentRange(npc, player.position, EnemyFSM.ATTACK_DIST))
        {
            
            if(enemyStateControl.receivedAttackCommand)
            {
                if (enemyShooting)
                {
                    enemyShooting.Firing = true;
                }
            }
            else
            {
                enemyShooting.Firing = false;
            }
            
        }
        else if (IsInCurrentRange(npc, player.position, EnemyFSM.CHASE_DIST))
        {           
            if (enemyShooting)
            {
                enemyShooting.Firing = false;
            }

            enemyStateControl.PerformTransition(Transition.ResetAttackPos);
        }
        else
        {
            if (enemyShooting)
            {
                enemyShooting.Firing = false;
            }

            // Patrol
            enemyStateControl.PerformTransition(Transition.Patrol);
        }
    }

    //Act
    public override void Act()
    {
        Quaternion leftQuatMax = Quaternion.AngleAxis(-45, new Vector3(0, 1, 0));
        Quaternion rightQuatMax = Quaternion.AngleAxis(45, new Vector3(0, 1, 0));

        UsefullFunctions.DebugRay(npc.transform.position, leftQuatMax * npc.transform.forward * 3.0f, Color.green);
        UsefullFunctions.DebugRay(npc.transform.position, rightQuatMax * npc.transform.forward * 3.0f, Color.blue);
        
        // Rotate Turret
        Transform turret = npc.GetComponent<EnemyFSM>().turret;
        Vector3 TargetDirection = player.position - turret.position;


        Quaternion turretTragetRotation = Quaternion.LookRotation(TargetDirection);

        float angleBetween = Vector3.Angle(TargetDirection, npc.transform.forward);

        if(angleBetween<45)
        {
            turret.rotation = Quaternion.Slerp(turret.rotation, turretTragetRotation, Time.deltaTime * curRotSpeed);
        }
        else
        {
            npc.rotation = Quaternion.Slerp(npc.rotation, turretTragetRotation, Time.deltaTime * curRotSpeed);
        }


    }

}
