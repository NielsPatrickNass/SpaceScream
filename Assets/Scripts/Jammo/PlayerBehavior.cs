using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Whisper;
using Whisper.Utils;

/// <summary>
/// This class is used to control the behavior of our Robot (State Machine and Utility function)
/// </summary>
public class PlayerBehavior : MonoBehaviour, WhisperInterface
{
    public Interactable currentlyInteractingWith;

    public Inventory inventory;

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
        Hide, // Be Puzzled
        MoveTo, // Move to a pillar
        UseInteract, // Move to then activate interact
        PickUp, // Move to then pickup
        BringObject, // Step one of bring object (move to it and grab it)
        BringObjectToPlayer // Step two of bring object (move to player and drop the object)
    }

    [Header("Robot Brain")]
    public SentenceSimilarity jammoBrain;

    [Header("Robot list of actions")]
    public List<Actions> actionsList;
    public List<Actions> actionsListOfCurrentRoom;
    
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

        // Take all the possible actions in actionsList
        foreach (PlayerBehavior.Actions actions in actionsList)
        {
            sentences.Add(actions.sentence);
        }
        sentencesArray = sentences.ToArray();
    }

    /// <summary>
    /// Rotate the agent towards the camera
    /// </summary>
    private void RotateTo()
    {
        var _lookRotation = Quaternion.LookRotation(cam.transform.position);
        agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation, _lookRotation, 360);
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
        if (maxScore < 0.35f)
        {
            state = State.Puzzled;
        }
        else
        {
            Debug.Log("maxScoreIndex: "+ maxScoreIndex);
            if (state == State.Idle && maxScoreIndex >= actionsList.Count)
            {
                state = State.Puzzled;
                goalObject = null;
                return;
            }

            string verb = actionsList[maxScoreIndex].verb;
            //Debug.Log("goalobject: " + actionsList[maxScoreIndex].noun.ToLower().Replace(".", "") + "| state: " + (State)System.Enum.Parse(typeof(State), verb, true));

            if (currentlyInteractingWith != null && (verb=="back" ||verb =="stop" || verb == "exit" ||verb=="let's go"))
            {
                currentlyInteractingWith.EndInteraction();
                actionsList = actionsListOfCurrentRoom;
            }

            object isState = null;
            // Set the Robot State == verb
            if (Enum.TryParse(typeof(State), verb, out isState))
                state = (State)System.Enum.Parse(typeof(State), verb, true);

            // Get the verb and noun (if there is one)
            if (state == State.PickUp)
            {
                goalObject = PickUp.GetPickUp(actionsList[maxScoreIndex].noun.ToLower().Replace(".", "")).gameObject;
            }
            else if (state == State.UseInteract)
            {
                goalObject = Interactable.GetInteractable(actionsList[maxScoreIndex].noun.ToLower().Replace(".", "")).gameObject;

            }
            else if (actionsList[maxScoreIndex].noun != "")
                goalObject = GameObject.Find(actionsList[maxScoreIndex].noun.ToLower().Replace(".", ""));
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
        }
    }

    public void OnSegmentFinished(string segment)
    {
        OnOrderGiven(segment);
    }


    /// <summary>
    /// When the user finished to type the order
    /// </summary>
    /// <param name="prompt"></param>
    public void OnOrderGiven(string prompt)
    {
        sentences = new List<string>();
        int i = 0;

        while (i < actionsList.Count)
        {
            if (actionsList[i].verb == "PickUp" || actionsList[i].verb == "UseInteract" || actionsList[i].verb == "MoveTo")
            {
                actionsList.RemoveAt(i);
            }
            else
                i++;
        }

        foreach (PlayerBehavior.Actions actions in actionsList)
        {
            sentences.Add(actions.sentence);
        }

        sentences.AddRange(PickUp.GetPossibleSentences());
        sentences.AddRange(Interactable.GetPossibleSentences());
        
        actionsList.AddRange(PickUp.GetPossibleActions());
        actionsList.AddRange(Interactable.GetPossibleActions());

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

    private void Update()
    {

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

            case State.Hide:
                agent.SetDestination(transform.position);
                RotateTo();
                anim.SetTrigger("hide");
                state = State.Idle;
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
                agent.SetDestination(goalObject.transform.position);

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
                    else
                        state = State.Idle;
                }
                break;

            case State.UseInteract:
                agent.SetDestination(goalObject.transform.position);


                if (agent.velocity.magnitude < 0.3f && Mathf.Abs(transform.position.y - goalObject.transform.position.y) < 3 && Vector3.Distance(transform.position, new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z)) < reachedPositionDistance)
                {
                    if (goalObject.name == "audiencepos")
                        state = State.Hello;
                    else
                        state = State.Idle;
                    Interactable interactable = goalObject.GetComponent<Interactable>();
                    if (interactable != null)
                    {
                        currentlyInteractingWith = interactable;
                        List<PlayerBehavior.Actions> newAtions = interactable.StartInteraction();
                        if (newAtions.Count > 0)
                            actionsList = newAtions;
                    }
                }
                break;

            case State.PickUp:
                agent.SetDestination(goalObject.transform.position);

                if (agent.velocity.magnitude < 0.3f && Mathf.Abs(transform.position.y - goalObject.transform.position.y) < 3 && Vector3.Distance(transform.position, new Vector3(goalObject.transform.position.x, transform.position.y, goalObject.transform.position.z)) < reachedPositionDistance)
                {

                    if (goalObject.tag == "Item")
                    {
                        state = State.Idle;
                        inventory.AddItem(goalObject);
                    }
                    else
                        state = State.Puzzled;
                }
                break;

            case State.BringObject:
                // First move to the object
                agent.SetDestination(goalObject.transform.position);
                if (Vector3.Distance(transform.position, goalObject.transform.position) < reachedObjectPositionDistance)
                {
                    Grab(goalObject);
                    state = State.BringObjectToPlayer;
                }
                break;

            case State.BringObjectToPlayer:
                agent.SetDestination(playerPosition.transform.position);
                if (Vector3.Distance(transform.position, playerPosition.transform.position) < reachedObjectPositionDistance)
                {
                    Drop(goalObject);
                    state = State.Idle;
                }
                break;
        }
    }
}
