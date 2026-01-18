using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Whisper;
using Whisper.Utils;

/// <summary>
/// This class is used to control the behavior of our Robot (State Machine and Utility function)
/// </summary>
public class PlayerBehavior : MonoBehaviour, WhisperInterface
{
    public static PlayerBehavior Instance;

    public Interactable currentlyInteractingWith;

    public Inventory inventory;

    public Transform manRig;
    public Transform rigHolder;

    private bool isGameOver;

    /// <summary>
    /// The Robot Action List
    /// </summary>
    [System.Serializable]
    public struct Actions
    {
        public string sentence;
        public string verb;
        public string noun;

        public Actions(string sentence, string verb, string noun)
        {
            this.sentence = sentence;
            this.verb = verb;
            this.noun = noun;
        }
    }

    /// <summary>
    /// Enum of the different possible states of our Robot
    /// </summary>
    private enum State
    {
        Idle,
        Sit,
        Hello, // Say hello
        Dance, // Be happy, dance
        Puzzled, // Be Puzzled
        GoHide, // Move to then hide
        Hiding, //
        MoveTo, // Move to a pillar
        UseInteract, // Move to then activate interact
        PickUp, // Move to then pickup
        BringObject, // Step one of bring object (move to it and grab it)
        BringObjectToPlayer // Step two of bring object (move to player and drop the object)

    }

    public bool isHiding;

    [Header("Robot Brain")]
    public SentenceSimilarity jammoBrain;

    [Header("Robot list of actions")]
    public List<Actions> actionsList;
    public List<Actions> actionsListOfCurrentRoom;
    public List<Actions> actionsListBonus;

    [Header("NavMesh and Animation")]
    public Animator anim;                       // Robot Animator
    public NavMeshAgent agent;                  // Robot agent (takes care of robot movement in the NavMesh)
    public float reachedPositionDistance;       // Tolerance distance between the robot and object.
    public float reachedObjectPositionDistance; // Tolerance distance between the robot and object.
    public Transform playerPosition;            // Our position
    public GameObject goalObject;               
    public GameObject grabPosition;             // Position where the object will be placed during the grab

    public Camera cam;                          // Main Camera

    [Header("Input UI")]
    public TMPro.TMP_InputField inputField;     // Our Input Field UI

    [SerializeField]
    private State state;

    [HideInInspector]
    public List<string> sentences; // Robot list of sentences (actions)
    public string[] sentencesArray;

    [HideInInspector]
    public float maxScore;
    public int maxScoreIndex;
    private WhisperStream _stream;

    private void Awake()
    {
        // Set the State to Idle
        state = State.Idle;
        sentences = new List<string>();
        Instance = this;
        // Take all the possible actions in actionsList
        foreach (PlayerBehavior.Actions actions in actionsList)
        {
            sentences.Add(actions.sentence);
        }
        sentencesArray = sentences.ToArray();
    }

    private void Start()
    {
        RegenerateActionsAndSentences();
    }
    /// <summary>
    /// Rotate the agent towards the camera
    /// </summary>
    private void RotateTo()
    {
        var _lookRotation = Quaternion.LookRotation(new Vector3(cam.transform.position.x, transform.position.y, cam.transform.position.z));
        agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation, _lookRotation, 360);
    }

    public void GameOver()
    {
        isGameOver = true;
        agent.SetDestination(transform.position);
        anim.SetTrigger("dying");
    }

    /// <summary>
    /// Grab the object by putting it in the grabPosition
    /// </summary>
    /// <param name="gameObject">Cube of color</param>
    void Grab(GameObject gameObject)
    {
        // Set the gameObject as child of grabPosition
        gameObject.transform.parent = grabPosition.transform;

        // To avoid bugs, set object velocity and angular velocity to 0
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set the gameObject transform position to grabPosition
        gameObject.transform.position = grabPosition.transform.position;
    }


    /// <summary>
    /// Drop the gameObject
    /// </summary>
    /// <param name="gameObject">Cube of color</param>
    void Drop(GameObject gameObject)
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.transform.SetParent(null);
    }


    /// <summary>
    /// Utility function: Given the results of HuggingFaceAPI, select the State with the highest score
    /// </summary>
    /// <param name="maxValue">Value of the option with the highest score</param>
    /// <param name="maxIndex">Index of the option with the highest score</param>
    public void Utility(float maxScore, int maxScoreIndex)
    {
        // First we check that the score is > of 0.2, otherwise we let our agent perplexed;
        // This way we can handle strange input text (for instance if we write "Go see the dog!" the agent will be puzzled).
        if (maxScore < 0.15f)
        {
            state = State.Puzzled;
        }
        else
        {
            if (isGameOver && maxScoreIndex == 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            Debug.Log("maxScoreIndex: "+ maxScoreIndex);
            if (state == State.Idle && maxScoreIndex >= actionsList.Count)
            {
                state = State.Puzzled;
                goalObject = null;
                return;
            }

            string verb = actionsList[maxScoreIndex].verb;
            
            //Debug.Log("goalobject: " + actionsList[maxScoreIndex].noun.ToLower().Replace(".", "") + "| state: " + (State)System.Enum.Parse(typeof(State), verb, true));


            if (isHiding)
            {
                isHiding = false;
                rigHolder.gameObject.SetActive(true);
                currentlyInteractingWith.EndInteraction();
                rigHolder.transform.localPosition = Vector3.zero;
                manRig.transform.localPosition = Vector3.zero;
                state = State.Idle;
            }
                

            if (currentlyInteractingWith != null && (verb=="back" ||verb =="stop" || verb == "exit" ||verb=="let's go"))
            {
                currentlyInteractingWith.EndInteraction();
                actionsList = actionsListOfCurrentRoom;
            }
            else if (currentlyInteractingWith != null)
                currentlyInteractingWith.PerformInteraction(actionsList[maxScoreIndex], inventory);


            object isState = null;
            // Set the Robot State == verb
            if (Enum.TryParse(typeof(State), verb, out isState))
                state = (State)System.Enum.Parse(typeof(State), verb, true);
            if (state==State.Hiding)
                state = State.GoHide;

            // Get the verb and noun (if there is one)
            if (state == State.PickUp)
            {
                
                goalObject = RoomInteractableManager.instance.GetCurrentRoomPickUpByName(actionsList[maxScoreIndex].noun.ToLower().Replace(".", "")).gameObject;
                if (goalObject == null)
                    goalObject = RoomInteractableManager.instance.GetClosestPickUpFromCurrentRoom().gameObject;
            }
            else if (state == State.UseInteract)
            {
                goalObject = RoomInteractableManager.instance.GetCurrentRoomInteractableByName(actionsList[maxScoreIndex].noun.ToLower().Replace(".", "")).gameObject;

            }
            else if (verb == "hide" && actionsList[maxScoreIndex].noun == "")
            {
                goalObject = Hidingspot.ClosestHidingSpot();
                state = State.GoHide;
            }
            else if (state == State.MoveTo)
                    {
                goalObject = null;
                Interactable inta = RoomInteractableManager.instance.GetCurrentRoomInteractableByName(actionsList[maxScoreIndex].noun.ToLower().Replace(".", ""));
                if (inta != null)
                    goalObject = inta.gameObject;
                if (inta == null)
                {
                    goalObject = RoomInteractableManager.instance.GetCurrentRoomSwitchersByName(actionsList[maxScoreIndex].noun.ToLower().Replace(".", "")).gameObject;
                }
                //if (goalObject == null)
                //    goalObject = GameObject.Find(actionsList[maxScoreIndex].noun);
            }
            //else if (actionsList[maxScoreIndex].noun != "")
            //    goalObject = GameObject.Find(actionsList[maxScoreIndex].noun);
            else
                goalObject = null;

            if (state == State.MoveTo && goalObject == null)
            {
                state = State.Puzzled;
            }
            if (state == State.BringObject && goalObject == null)
            {
                state = State.Puzzled;
            }
            if (state == State.BringObjectToPlayer && goalObject == null)
            {
                state = State.Puzzled;
            }
            if (state == State.UseInteract && goalObject == null)
            {
                state = State.Puzzled;
            }
            if (state == State.PickUp && goalObject == null)
            {
                state = State.Puzzled;
            }
            if (maxScore < actionsList.Count && maxScore >= 0)
            lastAction = actionsList[maxScoreIndex];
        }
    }

    public void OnSegmentFinished(string segment)
    {
        OnOrderGiven(segment);
    }


    public void RegenerateActionsAndSentences()
    {
        sentences = new List<string>();
        actionsList = new List<Actions>();

        if (isGameOver)
        {
            actionsList.Add(new Actions("restart", "restart", "restart"));
            sentences.Add("restart");
            return;
        }

        if (currentlyInteractingWith != null)
            actionsList.AddRange(currentlyInteractingWith.GetCurrentActions());
        actionsList.AddRange(actionsListBonus);
        //actionsList.AddRange(PickUp.GetPossibleActionsForAll());
        //actionsList.AddRange(Interactable.GetPossibleActionsForAll());
        actionsList.AddRange(RoomInteractableManager.instance.GetCurrentRoomActions());
        foreach (PlayerBehavior.Actions actions in actionsList)
        {
            sentences.Add(actions.sentence);
        }
    }

    /// <summary>
    /// When the user finished to type the order
    /// </summary>
    /// <param name="prompt"></param>
    public void OnOrderGiven(string prompt)
    {
        RegenerateActionsAndSentences();
        sentencesArray = sentences.ToArray();
        (int, float) tuple_ = jammoBrain.RankSimilarity(prompt, sentencesArray);
        Utility(tuple_.Item2, tuple_.Item1);
    }

    /*private void ResetEmotionBools()
    {
        anim.SetBool("hello", false);
        anim.SetBool("hide", false);
        anim.SetBool("puzzled", false);
    }*/
    public void SitDown(Transform sitPoint)
    {
        agent.isStopped = true;
        agent.ResetPath();

        // exakt positionieren
        transform.position = sitPoint.position;
        transform.rotation = sitPoint.rotation;

        state = State.Sit;
    }

    private Actions lastAction;

    public TextMeshPro debugWhereAmIGoing;

    private void Update()
    {
        debugWhereAmIGoing.text = goalObject != null ? goalObject.name : "nowhere";
        if (anim != null)
        {
            float speedParam = 0f;

            switch (state)
            {
                case State.MoveTo:
                case State.UseInteract:
                case State.PickUp:
                case State.BringObject:
                case State.BringObjectToPlayer:
                    // If speed than walk
                    speedParam = 1f;
                    break;

                default:
                    // Idle, Hello, Happy, Puzzled, usw. = stay
                    speedParam = 0f;
                    break;
            }

            anim.SetFloat("Speed", speedParam);
        }
        manRig.transform.localPosition = new Vector3(0, manRig.transform.localPosition.y, 0);
        manRig.transform.localEulerAngles = Vector3.zero;

        // Here's the State Machine, where given its current state, the agent will act accordingly
        switch (state)
        {
            default:
            case State.Idle:
                break;

            /*case State.Hello:
                agent.SetDestination(transform.position);
                RotateTo();
                anim.SetTrigger("hello");
                state = State.Idle;
                break;*/

            case State.Hiding:
                /*
                if (!isHiding && 
                    (((Hidingspot)currentlyInteractingWith).alternativeHideTransitionName == "" && anim.GetCurrentAnimatorStateInfo(0).IsName("transitionToHideState") ||
                    anim.GetCurrentAnimatorStateInfo(0).IsName(((Hidingspot)currentlyInteractingWith).alternativeHideTransitionName)) 
                    && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.75f)
                {
                    rigHolder.transform.position = ((Hidingspot)currentlyInteractingWith).teleportSpot.position;
                    isHiding = true;
                }
                */
                if (!isHiding)
                {
                    isHiding = true;
                    rigHolder.gameObject.SetActive(false);
                }
                break;

            case State.GoHide:
                agent.isStopped = false;
                agent.SetDestination(new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z));

                if (agent.velocity.magnitude < 0.3f &&
                    Mathf.Abs(transform.position.y - goalObject.transform.position.y) < 3 &&
                    Vector3.Distance(transform.position,
                        new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z))
                    < reachedPositionDistance)
                {
                    agent.isStopped = true;
                    agent.ResetPath();  // Bewegung wirklich beenden
                    Hidingspot hidingspot = goalObject.GetComponent<Hidingspot>();
                    if (hidingspot != null)
                    {
                        currentlyInteractingWith = hidingspot;
                        currentlyInteractingWith.StartInteraction(lastAction);
                        // anim.SetTrigger(hidingspot.alternativeHideTriggerName == "" ? "hide" : hidingspot.alternativeHideTriggerName);
                        state = State.Hiding;
                    }
                    goalObject = null;
                }
                break;

            case State.Dance:
                agent.SetDestination(transform.position);
                RotateTo();
                anim.SetTrigger("dance");
                state = State.Idle;
                break;

            /*case State.Puzzled:
                agent.SetDestination(transform.position);
                RotateTo();
                anim.SetTrigger("puzzled");
                state = State.Idle;
                break;*/

            case State.Puzzled:
                Debug.Log("STATE = PUZZLED (Trigger puzzled)");
                agent.isStopped = true;
                agent.ResetPath();
                RotateTo();
                anim.SetTrigger("puzzled");
                state = State.Idle;
                break;

            case State.Sit:
                anim.SetTrigger("sit");
                state = State.Idle;
                break;

            case State.Hello:
                Debug.Log("STATE = HELLO (Trigger hello)");
                agent.isStopped = true;
                agent.ResetPath();
                RotateTo();
                anim.SetTrigger("hello");
                state = State.Idle;
                break;


            /*case State.Hello:
                agent.SetDestination(transform.position);
                {
                    RotateTo();
                    ResetEmotionBools();         // NEU
                    anim.SetBool("hello", true); // nur hello aktiv
                    state = State.Idle;
                }
                break;

            case State.Hide:
                agent.SetDestination(transform.position);
                {
                    RotateTo();
                    ResetEmotionBools();         // NEU
                    anim.SetBool("hide", true);
                    state = State.Idle;
                }
                break;

            case State.Puzzled:
                agent.SetDestination(transform.position);
                {
                    RotateTo();
                    ResetEmotionBools();         // NEU
                    anim.SetBool("puzzled", true);
                    state = State.Idle;
                }
                break;*/

            case State.MoveTo:
                agent.isStopped = false;
                agent.SetDestination(new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z));

                if (agent.velocity.magnitude < 0.3f &&
                    Mathf.Abs(transform.position.y - goalObject.transform.position.y) < 3 &&
                    Vector3.Distance(transform.position,
                        new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z))
                    < reachedPositionDistance)
                {
                    agent.isStopped = true;
                    agent.ResetPath();  // Bewegung wirklich beenden

                    if (goalObject.name == "audiencepos")
                        state = State.Hello;
                    else if (goalObject.GetComponent<Hidingspot>() != null)
                    {
                        state = State.Hiding;
                        
                        currentlyInteractingWith = goalObject.GetComponent<Hidingspot>();
                        currentlyInteractingWith.StartInteraction(lastAction);
                        //anim.SetTrigger("hide");
                    }
                    else
                        state = State.Idle;
                    goalObject = null;
                }
                break;

            case State.UseInteract:
                agent.SetDestination(new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z));


                if (agent.velocity.magnitude < 0.3f && Vector3.Distance(transform.position, new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z)) < reachedPositionDistance)
                {
                    Hidingspot locspot = goalObject.GetComponent<Hidingspot>();
                    if (goalObject.name == "audiencepos")
                        state = State.Hello;
                    else if (locspot != null)
                    {
                        state = State.Hiding;
                        currentlyInteractingWith = locspot;
                        currentlyInteractingWith.StartInteraction(lastAction);
                        //anim.SetTrigger(locspot.alternativeHideTriggerName == "" ? "hide" : locspot.alternativeHideTriggerName);
                    }
                    else
                        state = State.Idle;
                    Interactable interactable = goalObject.GetComponent<Interactable>();
                    if (interactable != null)
                    {
                        currentlyInteractingWith = interactable;
                        List<PlayerBehavior.Actions> newActions = interactable.StartInteraction(lastAction);
                        if (newActions.Count > 0)
                            actionsList = newActions;
                    }
                    goalObject = null;
                }
                break;

            case State.PickUp:
                agent.SetDestination(new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z));

                if (agent.velocity.magnitude < 0.3f && Mathf.Abs(transform.position.y - goalObject.transform.position.y) < 3 && Vector3.Distance(transform.position, new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z)) < reachedPositionDistance)
                {

                    if (goalObject.GetComponent<PickUp>() != null)
                    {
                        state = State.Idle;
                        inventory.AddItem(goalObject);
                    }
                    else
                        state = State.Puzzled;
                    goalObject = null;
                }
                break;

            case State.BringObject:
                // First move to the object
                agent.SetDestination(new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z));
                if (Vector3.Distance(transform.position, goalObject.transform.position) < reachedObjectPositionDistance)
                {
                    Grab(goalObject);
                    state = State.BringObjectToPlayer;
                }
                break;

            case State.BringObjectToPlayer:
                agent.SetDestination(new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z));
                if (Vector3.Distance(transform.position, playerPosition.transform.position) < reachedObjectPositionDistance)
                {
                    Drop(goalObject);
                    state = State.Idle;
                }
                break;
        }
    }
}
