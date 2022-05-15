using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleColumn : MonoBehaviour
{
    private List<GameObject> neighbors;

    public bool doRipple = false;
    private bool doingRipple = false;
    private bool rippleDone = false;

    public bool doPathing = false;
    private bool doingPathing = false;
    private bool pathingDone = false;

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

    public void SetValues(List<GameObject> nb, float ml, float lt, float perc) 
    {
        neighbors = nb;
        moveLength = ml;
        lerpTime = lt;
        percStartRipple = perc;
    }

    public void UpdateValues(float ml, float lt, float perc)
    {
        moveLength = ml;
        lerpTime = lt;
        percStartRipple = perc;
    }

    private void ResetVariables() 
    {
        doRipple = false;
        doingRipple = false;
        rippleDone = false;
        goingDown = false;
        doneActivatingOthers = false;

        doPathing = false;
        doingPathing = false;
        pathingDone = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (RESET) 
        {
            ResetVariables();
            RESET = false;
        }

        if (doRipple) 
        {
            doRipple = false;
            doPathing = false;  // cannot do both at the same time
            if (!doingRipple && !rippleDone) 
            {
                doingRipple = true;
                goingDown = true;
                startPos = transform.position;
                endPos = transform.position - transform.up * moveLength;
                currLerpTime = 0f;
            }
        }

        if (doingRipple && goingDown) 
            LerpStartToEnd();
        else if (doingRipple && !goingDown)
            LerpEndToStart();

        if (doPathing) 
        {
            doPathing = false;
            Debug.Log(transform.position);
            if (!doingPathing && !pathingDone) 
            {
                doingPathing = true;
                goingDown = true;
                startPos = transform.position;
                endPos = transform.position - transform.up * moveLength;
                currLerpTime = 0f;
            }
        }

        if (doingPathing && goingDown)
            LerpStartToEnd();
        else if (doingPathing && !goingDown)
            LerpEndToStart();
 
    }

    private void LerpStartToEnd() 
    {
        currLerpTime += Time.deltaTime;
        float perc = currLerpTime / lerpTime;
        if (currLerpTime > lerpTime) 
        {
            currLerpTime = lerpTime;
            perc = currLerpTime / lerpTime;
            goingDown = false;
            currLerpTime = 0f;
        }
        perc = Crossfade(SmoothStart3, SmoothStop2, perc);

        if (perc >= percStartRipple && !doneActivatingOthers) 
        {
            doneActivatingOthers = true;
            if (doingPathing) RippleOneNeighbor();
            else RippleNeighbors();
        }

        transform.position = Vector3.Lerp(startPos, endPos, perc);
    }

    private void LerpEndToStart() 
    {
        currLerpTime += Time.deltaTime;
        float perc = currLerpTime / lerpTime;
        if (currLerpTime > lerpTime) 
        {
            currLerpTime = lerpTime;
            perc = currLerpTime / lerpTime;
            doingRipple = false;
            doingPathing = false;
            rippleDone = true;
            pathingDone = true;
            currLerpTime = 0f;
        }
        
        perc = Crossfade(SmoothStart3, SmoothStop2, perc);

        transform.position = Vector3.Lerp(endPos, startPos, perc);
    }

    private void RippleNeighbors() 
    {
        // set doRipple to true for all neighbors
        foreach (GameObject obj in neighbors)
            obj.GetComponent<RippleColumn>().doRipple = true;
    }

    private void RippleOneNeighbor() 
    {
        Debug.Log("ripple a neigbor");
        // set doRipple to true for one neighbor, the one with the lowest y-value if transform.position
        float lowestY = Mathf.Infinity;
        int lowestYidx = 0;
        bool someOneFound = false;
        for (int i = 0; i < neighbors.Count; i++) 
        {
            RippleColumn rp_nb = neighbors[i].GetComponent<RippleColumn>();
            if (neighbors[i].transform.position.y < lowestY && !rp_nb.doingPathing && !rp_nb.pathingDone) 
            {
                lowestY = neighbors[i].transform.position.y;
                lowestYidx = i;
                someOneFound = true;
            }
        }
        if (someOneFound) 
        {
            // then do the pathing, if this is false that means every columns is done with the pathing
            neighbors[lowestYidx].GetComponent<RippleColumn>().doPathing = true;
            Debug.Log("found! at " + neighbors[lowestYidx].transform.position);
        }
        Debug.Log("ripple a neigbor done");
    }

    // from AiG EasingFunctions.cs
    public static float SmoothStart3(float t)
    {
        return t * t * t;
    }

    public static float SmoothStop2(float t) 
    {
        return 1 - (1 - t) * (1 - t);
    }

    // can be used to achieve smoothstep (combine smoothstart and smooth stop)
    public static float Crossfade(System.Func<float, float> a, System.Func<float, float> b, float t)
    {
        return a(t) + t * (b(t) - a(t));
    }
}
