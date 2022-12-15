using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObstacleScript : MonoBehaviour
{
    private Vector3 startPos;
    public float timeToStay = 1f;
    public Vector3 targetPos;
    bool isRunning;

    private Coroutine coroutine;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        isRunning = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void loopBack()
    {
        if (isRunning)
            StopCoroutine(coroutine);
        transform.position = startPos;
        isRunning = false;
    }

    public void doAction()
    {
        if (!isRunning)
        {
            coroutine = StartCoroutine(moveToAndBack());
        }
    }

    public void doActionStay()
    {
        StartCoroutine(moveTo());
    }

    public void doActionStop()
    {
        StartCoroutine(moveBack());
    }

    IEnumerator moveTo()
    {
        float timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, timer / 0.1f);

            yield return null;
        }
        transform.position = targetPos;
    }

    IEnumerator moveBack()
    {
        float timer = 0f;
        while (timer < (timeToStay / 10))
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(targetPos, startPos, timer / 0.1f);

            yield return null;
        }
        transform.position = startPos;
    }

    IEnumerator moveToAndBack()
    {
        isRunning = true;
        yield return (StartCoroutine(moveTo()));

        float timer = 0f;
        while (timer < timeToStay)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        yield return (StartCoroutine(moveBack()));
        isRunning = false;
        yield return null;
    }
}
