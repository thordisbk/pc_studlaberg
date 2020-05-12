using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleColumn : MonoBehaviour
{
    private List<GameObject> neighbors;

    public bool doRipple = false;
    private bool doingRipple = false;
    private bool rippleDone = false;

    private bool goingDown = false;  // if true then going down, if false then going up
    private Vector3 startPos;
    private Vector3 endPos;
    private float moveLength = 1f;
    private float lerpTime = 0.5f;
    private float percStartRipple = 0.5f;
    private float currLerpTime = 0f;
    private bool doneActivatingOthers = false;

    [Tooltip("Reset values to do another ripple. Resetting on one object resets all other objects.")]
    public bool RESET = false;

    public void SetValues(List<GameObject> nb, float ml, float lt, float perc) {
        neighbors = nb;
        moveLength = ml;
        lerpTime = lt;
        percStartRipple = perc;
    }

    public void UpdateValues(float ml, float lt, float perc) {
        moveLength = ml;
        lerpTime = lt;
        percStartRipple = perc;
    }

    private void ResetVariables() {
        doRipple = false;
        doingRipple = false;
        rippleDone = false;
        goingDown = false;
        doneActivatingOthers = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (RESET) {
            ResetVariables();
            RESET = false;
        }

        if (doRipple) {
            doRipple = false;
            if (!doingRipple && !rippleDone) {
                doingRipple = true;
                goingDown = true;
                startPos = transform.position;
                endPos = transform.position - transform.up * moveLength;
                currLerpTime = 0f;
            }
        }

        if (doingRipple && goingDown) {
            LerpStartToEnd();
        }
        else if (doingRipple && !goingDown) {
            LerpEndToStart();
        }
 
    }

    private void LerpStartToEnd() {
        currLerpTime += Time.deltaTime;
        float perc = currLerpTime / lerpTime;
        if (currLerpTime > lerpTime) {
            currLerpTime = lerpTime;
            perc = currLerpTime / lerpTime;
            goingDown = false;
            currLerpTime = 0f;
        }
        perc = SmoothStop2(perc);
        if (perc >= percStartRipple && !doneActivatingOthers) {
            doneActivatingOthers = true;
            RippleNeighbors();
        }
        transform.position = Vector3.Lerp(startPos, endPos, perc);
    }

    private void LerpEndToStart() {
        currLerpTime += Time.deltaTime;
        float perc = currLerpTime / lerpTime;
        if (currLerpTime > lerpTime) {
            currLerpTime = lerpTime;
            perc = currLerpTime / lerpTime;
            doingRipple = false;
            rippleDone = true;
            currLerpTime = 0f;
        }
        //perc = SmoothStop4(perc);
        perc = Crossfade(SmoothStart3, SmoothStop2, perc);

        transform.position = Vector3.Lerp(endPos, startPos, perc);
    }

    private void RippleNeighbors() {
        foreach (GameObject obj in neighbors) {
            obj.GetComponent<RippleColumn>().doRipple = true;
        }
    }


    // from AiG EasingFunctions.cs
    public static float SmoothStart3(float t)
    {
        return t * t * t;
    }
    public static float SmoothStop2(float t) {
        return 1 - (1 - t) * (1 - t);
    }
    // can be used to achieve smoothstep (combine smoothstart and smooth stop)
    public static float Crossfade(System.Func<float, float> a, System.Func<float, float> b, float t)
    {
        return a(t) + t * (b(t) - a(t));
    }
}
