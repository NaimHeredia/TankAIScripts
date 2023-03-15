using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Name: Naim Heredia
 * Purpose: Dead State for the Enemies AI, no action takes place
 * 
*/

public class DeadState : FSMState
{
    EnemyController npcTankControl = null;
    EnemyFSM enemyStateControl = null;

    public DeadState(Transform[] wp, EnemyController npcTank)
    {
        stateID = FSMStateID.Dead;
        npcTankControl = npcTank;
        enemyStateControl = npcTank.enemyFSMControl;
    }
    
    //Reason
    public override void Reason()
    {
        // do nothing
    }

    //Act
    public override void Act()
    {
        // do nothing
    }

}
