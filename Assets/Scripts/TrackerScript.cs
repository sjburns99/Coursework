using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TrackerScript : MonoBehaviour
{
    //How long the player's time limit is - could vary based on level
    public float timeLimit = 5f;
    private float timeLeft;

    //How often positions are checked - lower values mean more accurate paths, but also longer lists per object
    public float checkpointFrequency = 1f;

    //How often a player may interact with objects
    public float interactCooldownDuration = 0.5f;
    public float interactDistance = 2.0f;

    //Info to be logged
    class logInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public float time;
        public bool interact;
        public string shell;
        public GameObject targetObject;
    }
    List<logInfo> logList = new List<logInfo>();

    //Shells
    public GameObject shellPrefab;
    public GameObject[] shells;

    //Stuff to keep track of the player
    public GameObject playerShell;
    public Camera playerCam;

    //Loop info
    int loopCount = 0;
    public int maxLoops = 3;

    //Bool value that tracks whether recording or playback is currently running, to prevent both happening at once for now
    bool running = false;
    bool keepGoing = true;


    //UI
    public TMP_Text countdown;
    public TMP_Text message;
    public Image screenDarkener;
    public TMP_Text loopsLeft;

    private void Start()
    {
        message.enabled = false;
        screenDarkener.enabled = false;
        Time.timeScale = 1;
        StartLoop();
        timeLimit += MainMenuScript.bonusTime;
    }

    private void Update()
    {
        countdown.text = "";
        if (maxLoops - loopCount != 1)
            loopsLeft.text = (maxLoops - loopCount).ToString() + " Loops Remain";
        else
            loopsLeft.text = (maxLoops - loopCount).ToString() + " Loop Remains";
        if (Input.GetKeyDown(KeyCode.Q) && running)
        {
            timeLeft = 0f;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(WaitAndReset(0));
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Main Menu");
        }
    }

    private void WinLevel()
    {
        //Stop Looping
        keepGoing = false;
        //Display victory message
        StartCoroutine(DisplayMessage("Level Complete", 0));
        //Log level completion
        Scene scene = SceneManager.GetActiveScene();
        switch(scene.name)
        {
            case "ButtonPush": MainMenuScript.level1Complete = true; break;
            case "BasicCube": MainMenuScript.level2Complete = true; break;
            case "ButtonAndCube": MainMenuScript.level3Complete = true; break;
        }
        //Back to Menu
        StartCoroutine(WaitThenMenu());
    }

    private void hitShell()
    {
        //For the first second of the loop, collision with shells is permitted
        Debug.Log(timeLeft + " vs " + (timeLimit - 1));
        if(timeLeft < (timeLimit - 1))
        {
            keepGoing = false;
            StartCoroutine(DisplayMessage("WARNING: TIMELINE UNSTABLE!\nProbable Cause: Contact With Past Self\nPress R to Reset...", 0));
            playerShell.SendMessage("disableMovement");
        }
    }

    private void StartLoop()
    {
        //Simulate start of loop
        if (!running)
        {
            //Ensure current loop doesn't exceed the limit
            if (loopCount < maxLoops)
            {
                //Start recording new loop
                StartCoroutine(WaitThenStart());
            }
            //If it does, prompt to reset
            else
            {
                keepGoing = false;
                StartCoroutine(DisplayMessage("WARNING: TIMELINE UNSTABLE!\nProbable Cause: Timeline Overload\nPress R to Reset...", 0));
                playerShell.SendMessage("disableMovement");
            }
        }
    }

    IEnumerator CountdownThenStart()
    {
        playerShell.SendMessage("disableMovement");
        yield return StartCoroutine(DisplayMessage("3", 1));
        yield return StartCoroutine(DisplayMessage("2", 1));
        yield return StartCoroutine(DisplayMessage("1", 1));
        yield return StartCoroutine(DisplayMessage("BEGIN", 1));
        playerShell.SendMessage("enableMovement");
        Record();
    }

    IEnumerator WaitThenStart()
    {
        //Give one second grace period in case player is holding a button
        playerShell.SendMessage("disableMovement");
        yield return StartCoroutine(DisplayMessage("Press Any Key to Begin Loop", 0));
        yield return new WaitForSeconds(1);

        //Wait for input - any key press but the mouse
        while (!(Input.anyKey && !(Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))))
        {
            yield return null;
        }

        //Input made - dismiss prompt and get started
        playerShell.SendMessage("enableMovement");
        WipeMessage();
        Record();
    }

    IEnumerator WaitThenMenu()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Main Menu");
    }

    private void Record()
    {
        //Record player movements
        Playback();
        StartCoroutine(RecordPlayerPath());

        //Get recording and apply to a new shell
        //When instantiating, position it under the map so it's not visible to the player
        GameObject shell = Instantiate(shellPrefab, new Vector3(0, -50, 0), new Quaternion(0, 0, 0, 0));
        shell.name = "Shell" + loopCount;
        shell.tag = "shell";
    }

    private void Playback()
    {
        shells = GameObject.FindGameObjectsWithTag("shell");
        if (logList.Count > 0)
        {
            StartCoroutine(PlaybackPlayerPath());
        }
    }

    private void sortList()
    {
        logList.Sort((u1, u2) => u2.time.CompareTo(u1.time));
        //foreach (logInfo item in logList)
        //{
        //    Debug.Log(item.shell + " - " + item.time);
        //}
    }

    private void loopBack()
    {
        //Send everything back to the start
        List<GameObject> objectsToLoop = new List<GameObject>();
        objectsToLoop.AddRange(GameObject.FindGameObjectsWithTag("physicsObject"));
        objectsToLoop.AddRange(GameObject.FindGameObjectsWithTag("moving"));
        objectsToLoop.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        foreach (GameObject objectToLoop in objectsToLoop)
        {
            objectToLoop.SendMessage("loopBack");
        }
    }
    IEnumerator RecordPlayerPath()
    {
        //Track what the player's looking at for interactions
        RaycastHit hunter; GameObject hunted;

        //Tracking starts - set running variable to true to prevent it from starting again
        running = true; timeLeft = timeLimit; float interactionCooldown = 0;
        //Subtract checkpoint frequency from remaining time to find the next checkpoint
        float nextCheckpoint = timeLeft - checkpointFrequency;
        //Before countdown starts, add starting position and time to their lists
        //timeList.Add(timeLeft); positionList.Add(player.transform.position);
        logList.Add(new logInfo { position = playerShell.transform.position, rotation = playerShell.transform.rotation, time = timeLeft, interact = false, shell = "Shell" + loopCount, targetObject = null });

        //Start countdown - end when timer is lower than zero
        while (timeLeft >= 0)
        {
            countdown.text = timeLeft.ToString("F2");

            //Prevents the last checkpoint being set to a negative number, which otherwise would be ignored by the while loop here
            if (nextCheckpoint < 0)
            {
                nextCheckpoint = 0;
            }

            //Decrement timer
            timeLeft -= Time.deltaTime;

            //If interaction cooldown is active, tick it down
            if (interactionCooldown > 0)
            {
                interactionCooldown -= Time.deltaTime;
            }

            //If a checkpoint's been reached, note down the time and position
            if (timeLeft <= nextCheckpoint)
            {
                //Set a new checkpoint
                nextCheckpoint = timeLeft - checkpointFrequency;
                //Add to lists
                //timeList.Add(timeLeft); positionList.Add(player.transform.position);
                logList.Add(new logInfo { position = playerShell.transform.position, rotation = playerShell.transform.rotation, time = timeLeft, interact = false, shell = "Shell" + loopCount, targetObject = null });
            }

            //Player interaction - being in the right time and place will be important, here, so note that one down
            if (Physics.Raycast(playerShell.transform.position, playerShell.transform.forward, out hunter, interactDistance))
            {
                hunted = hunter.transform.gameObject;
                //Ensure an interaction can actually take place before running anything
                if (interactionCooldown <= 0)
                {
                    //If looking at an interactive object and mouse 1 is clicked, take note of that
                    if (Input.GetMouseButtonDown(0) && (hunted.CompareTag("physicsObject") || hunted.CompareTag("pushButton")))
                    {
                        //Log usual stuff and what object is being interacted with
                        logList.Add(new logInfo { position = playerShell.transform.position, rotation = playerShell.transform.rotation, time = timeLeft, interact = true, shell = "Shell" + loopCount, targetObject = hunted });
                        //Actually interact with the object
                        hunted.SendMessage("runInteraction", playerShell);
                    }
                }

            }

            yield return null;
        }

        //Tracking ends - set running variable back to false
        running = false;
        loopCount++;

        //Proceed to next loop
        if (keepGoing)
        {
            loopBack();
            StartLoop();
        }
        

        yield break;
    }

    IEnumerator PlaybackPlayerPath()
    {
        sortList();

        //Playback starts
        running = true; float timeLeft = timeLimit;

        int listIndex = 0;

        //Start countdown - end when timer is lower than zero
        while (timeLeft > 0)
        {
            countdown.text = "Playback";
            //When timer reaches a checkpoint, output a log
            if (timeLeft <= logList[listIndex].time)
            {
                //Debug.Log("Time: " + logList[listIndex].time + "\nPosition: " + logList[listIndex].position.ToString() + "\nLoop: " + logList[listIndex].shell);

                //Move shells
                foreach(GameObject shell in shells)
                {
                    if (shell.name == logList[listIndex].shell)
                    {
                        shell.transform.SetPositionAndRotation(logList[listIndex].position, logList[listIndex].rotation);
                        if (logList[listIndex].interact)
                        {
                            logList[listIndex].targetObject.SendMessage("runInteraction", shell);
                        }
                    }
                }


                //Increment the index so it knows to look for the next checkpoint in the list
                listIndex++;
            }

            //Run down timer
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        //Playback ends
        running = false;
        yield break;
    }

    IEnumerator DisplayMessage(string toDisplay, float duration)
    {
        message.text = toDisplay;
        message.enabled = true;
        screenDarkener.enabled = true;
        yield return new WaitForSeconds(duration);
        //Duration of 0 means message should stay
        if (duration == 0)
        {
            yield return null;
        }
        //Otherwise remove from UI once time is elapsed
        else
        {
            WipeMessage();
        }    
    }

    void WipeMessage()
    {
        //Remove the current message from the UI
        message.enabled = false;
        screenDarkener.enabled = false;
    }

    IEnumerator WaitAndReset(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
