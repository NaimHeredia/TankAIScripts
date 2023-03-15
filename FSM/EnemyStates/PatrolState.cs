using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Name: Naim Heredia
 * Purpose: Patrol State for the Enemies AI, Follow a path behaviour
 * 
*/
public class PatrolState : FSMState
{
    EnemyController npcTankControl = null;
    EnemyFSM enemyStateControl = null;
    EnemyShooting enemyShooting = null;
    Transform npc;
    Transform player;


    Transform[] Waypoints;
    int currentWaypoint;


    public PatrolState(Transform[] wp, EnemyController npcTank)
    {
        stateID = FSMStateID.Patrolling;
        npcTankControl = npcTank;
        enemyStateControl = npcTank.enemyFSMControl;

        curRotSpeed = 2.0f;
        curSpeed = 2.0f;
        

        npc = npcTankControl.gameObject.transform;
        player = enemyStateControl.GetPlayerTransform();

        enemyShooting = npcTankControl.enemyShooting;

        currentWaypoint = 0;
        Waypoints = wp;

        destPos = Waypoints[currentWaypoint].position;
    }

    public override void EnterStateInit()
    {
        enemyStateControl.navAgent.speed = 5f;

        if (enemyShooting)
        {
            enemyShooting.Firing = false;
        }
    }

    //Reason
    public override void Reason()
    {        

        // Check if we reached waypoint
        if (IsInCurrentRange(npc, destPos, EnemyFSM.SLOT_DIST))
        {
            if (currentWaypoint < Waypoints.Length)
            {
                destPos = Waypoints[currentWaypoint].position;
                currentWaypoint++;
                if (currentWaypoint >= Waypoints.Length)
                {
                    currentWaypoint = 0;
                }
            }
            else
            {
                currentWaypoint = 0;
                destPos = Waypoints[currentWaypoint].position;
            }
        }


        // Transitions

        // Death
        if (npcTankControl.IsDead)
        {
            enemyStateControl.PerformTransition(Transition.NoHealth);
        }

        // Patrol Based on player skill
        if (WorldData.Instance.PlayerSkill == WorldData.SkillClassifierType.EXCELLENT || WorldData.Instance.PlayerSkill == WorldData.SkillClassifierType.VERY_GOOD)
        {
            // More Speed to patrol
            enemyStateControl.navAgent.speed = 10f;
        }

        // Chase
        // If player is range and our hp is higher than 75%
        if (IsInCurrentRange(npc, player.position, EnemyFSM.CHASE_DIST) && npcTankControl.HitPoints >= npcTankControl.maxHitPoints * 0.75)
        {
            // Chase
            enemyStateControl.PerformTransition(Transition.ResetAttackPos);
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
        // Follow Path

        // Rotate
        Quaternion targetRotation = Quaternion.LookRotation(destPos - enemyStateControl.transform.position);
        //enemyStateControl.transform.rotation = targetRotation;
        enemyStateControl.transform.rotation = Quaternion.Slerp(enemyStateControl.transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        enemyStateControl.turret.rotation = Quaternion.Slerp(enemyStateControl.turret.rotation, enemyStateControl.transform.rotation, Time.deltaTime * curRotSpeed);

        // Move
        enemyStateControl.navAgent.destination = destPos;
    }

}
