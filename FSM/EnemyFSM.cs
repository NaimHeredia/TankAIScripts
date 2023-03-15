/// <summary>
/// Controls the enemy with respect to the its state machine.
/// </summary>

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

/*
 * Name: Naim Heredia
 * Purpose: FSM for the enemies in the game, here we include the states and transitions, and initialize and use the FSM
 * 
*/
public class EnemyFSM : AdvancedFSM
{
    //Adjust these as needed...
    public static int SLOT_DIST = 1;
    public static int ATTACK_DIST = 25;
    public static int CHASE_DIST = 40;
    public static int WAYPOINT_DIST = 1;

    public  SlotManager defendSlotManager;
    public NavMeshAgent navAgent;
    public Transform turret;

    public bool receivedAttackCommand;

    [HideInInspector]
    public Rigidbody rigBody;

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }
    public SlotManager GetPlayerSlot()
    {
        return playerSlotManager;
    }

    public SlotManager GetCoverSlot()
    {
        return coverSlotManager;
    }
    public string GetStateString()
    {
        string state = "NONE";
        if (CurrentState != null)
        {
            state = CurrentState.ToString();
        }
        return state;
    }

    private Transform playerTransform;
    private GameObject[] pointList;
    private SlotManager playerSlotManager;    
    private SlotManager coverSlotManager;
    private bool debugDraw;
    private EnemyController enemyControl;

    // Initialize the FSM for the NPC tank.
    protected override void Initialize()
    {
        enemyControl = GetComponent<EnemyController>();
        debugDraw = true;

        // Find the Player and init appropriate data.
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;
        playerSlotManager = objPlayer.GetComponent<SlotManager>();
        coverSlotManager = GameObject.FindGameObjectWithTag("CoverSlotManager").GetComponent<SlotManager>();
        rigBody = GetComponent<Rigidbody>();

        receivedAttackCommand = false;

        // Listen to attack events
        AIManager.instance.startAttackEvent.AddListener(HandleStartAttackEvent);
        AIManager.instance.stopAttackEvent.AddListener(HandleStopAttackEvent);

        // Create the FSM for the tank.
        ConstructFSM();
    }

    // Update each frame.
    protected override void FSMUpdate()
    {
        if (CurrentState != null)
        {
            CurrentState.Reason();
            CurrentState.Act();
        }
        if (debugDraw)
        {
            UsefullFunctions.DebugRay(transform.position, transform.forward * 5.0f, Color.red);
        }
    }
    
    private void ConstructFSM()
    {
        pointList = GameObject.FindGameObjectsWithTag("WayPoint");
        // Creating a waypoint transform array for each state.
        Transform[] waypoints = new Transform[pointList.Length];
        int i = 0;
        foreach (GameObject obj in pointList)
        {
            waypoints[i] = obj.transform;
            i++;
        }
        //
        // Create States.
        //
        // Create Dead State.
        DeadState dead = new DeadState(waypoints, enemyControl);
        // Set a transition out of the state: 
        // The dead state of the AI handles an enable transition request by going to the "patrol" state.
        dead.AddTransition(Transition.Enable, FSMStateID.Chasing);

        // Chase
        ChaseState chase = new ChaseState(waypoints, enemyControl);
        chase.AddTransition(Transition.Enable, FSMStateID.Chasing);
        chase.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        chase.AddTransition(Transition.LowHealth, FSMStateID.Fleeing);
        chase.AddTransition(Transition.ReachedPlayer, FSMStateID.Attacking);
        chase.AddTransition(Transition.Patrol, FSMStateID.Patrolling);

        //Attack
        AttackState attack = new AttackState(waypoints, enemyControl);
        attack.AddTransition(Transition.Enable, FSMStateID.Chasing);
        attack.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        attack.AddTransition(Transition.LowHealth, FSMStateID.Fleeing);
        attack.AddTransition(Transition.ResetAttackPos, FSMStateID.Chasing);
        attack.AddTransition(Transition.Patrol, FSMStateID.Patrolling);

        //Flee
        FleeState flee = new FleeState(waypoints, enemyControl);
        flee.AddTransition(Transition.Enable, FSMStateID.Chasing);
        flee.AddTransition(Transition.NoHealth, FSMStateID.Dead);        
        flee.AddTransition(Transition.TakeCover, FSMStateID.TakinCover);

        //Cover
        CoverState cover = new CoverState(waypoints, enemyControl);
        cover.AddTransition(Transition.Enable, FSMStateID.Chasing);
        cover.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        cover.AddTransition(Transition.LowHealth, FSMStateID.Fleeing);
        cover.AddTransition(Transition.ResetAttackPos, FSMStateID.Chasing);
        cover.AddTransition(Transition.Patrol, FSMStateID.Patrolling);

        // Patrol
        PatrolState patrol = new PatrolState(waypoints, enemyControl);
        patrol.AddTransition(Transition.Enable, FSMStateID.Chasing);
        patrol.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        patrol.AddTransition(Transition.LowHealth, FSMStateID.Fleeing);
        patrol.AddTransition(Transition.ResetAttackPos, FSMStateID.Chasing);        

        //Add states to the state list.
        AddFSMState(chase); //First one in List is default
        AddFSMState(attack);
        AddFSMState(flee);
        AddFSMState(cover);
        AddFSMState(patrol);
        AddFSMState(dead);
    }

    void HandleStartAttackEvent(int number)
    {
        if(number == enemyControl.charID && CurrentStateID == FSMStateID.Attacking)
        {
            // Shoot
            receivedAttackCommand = true;
        }
    }

    void HandleStopAttackEvent()
    {
        receivedAttackCommand = false;
    }

    private void OnEnable()
    {
       if (navAgent)
            navAgent.isStopped = false;
        if (CurrentState != null)
            // Request the state machine to perform the "enable" transition.
            // The current state of the AI must handle the enable request. See Dead state initalization in ConstructFSM.
            PerformTransition(Transition.Enable);  
    }
    private void OnDisable()
    {
        if (navAgent && navAgent.isActiveAndEnabled)
            navAgent.isStopped = true;
    }
   
     
}
