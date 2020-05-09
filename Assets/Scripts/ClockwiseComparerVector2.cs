using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://answers.unity.com/questions/877169/vector2-array-sort-clockwise.html

public class ClockwiseComparerVector2 : IComparer<Vector2>
{
    // the origin of the clock
    private Vector2 origin;
 
    public ClockwiseComparerVector2(Vector2 point) {
        origin = point;
    }
 
    // Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    public int Compare(Vector2 point1, Vector2 point2) {
        return IsClockwise(point1, point2);
    }

    // return 1 if point1 is before point2 clockwise
    // return -1 if point2 is before point1 clockwise
    // return 0 if points are identical
    public int IsClockwise(Vector2 point1, Vector2 point2) {
        if (point1 == point2) {
            return 0;
        }
 
        Vector2 point1Offset = point1 - origin;
        Vector2 point2Offset = point2 - origin;
 
        float angle1 = Mathf.Atan2(point1Offset.x, point1Offset.y);
        float angle2 = Mathf.Atan2(point2Offset.x, point2Offset.y);
 
        if (angle1 < angle2) return -1;
        else if (angle1 > angle2) return 1;

        // if angle is the same, let the point that is closer to the origin come second in clockwise order
        bool point1IsCloser = point1Offset.sqrMagnitude < point2Offset.sqrMagnitude;
        return point1IsCloser ? 1 : -1;
    }
}