using UnityEngine;

public class HandGestureDebugLogger : MonoBehaviour
{
    private bool leftFist;
    private bool rightFist;

    private bool leftThumbUp;
    private bool rightThumbUp;
    private bool rightIndexUp;

    private void Awake()
    {
        Debug.Log("[Hand Gesture] HandGestureDebugLogger initialized. Listening for hand gesture events...");
    }

    public void OnLeftFistPerformed()
    {
        leftFist = true;
        Debug.Log("[Hand Gesture] Left fist detected");

        CheckBothFists();
    }

    public void OnLeftFistEnded()
    {
        leftFist = false;
        Debug.Log("[Hand Gesture] Left fist ended");
    }

    public void OnRightFistPerformed()
    {
        rightFist = true;
        Debug.Log("[Hand Gesture] Right fist detected");

        CheckBothFists();
    }

    public void OnRightFistEnded()
    {
        rightFist = false;
        Debug.Log("[Hand Gesture] Right fist ended");
    }

    public void OnLeftFistThumbUpPerformed()
    {
        leftThumbUp = true;
        Debug.Log("[Hand Gesture] Left fist + thumb up detected");

        CheckBothThumbsUp();
    }

    public void OnLeftFistThumbUpEnded()
    {
        leftThumbUp = false;
        Debug.Log("[Hand Gesture] Left fist + thumb up ended");
    }

    public void OnRightFistThumbUpPerformed()
    {
        rightThumbUp = true;
        Debug.Log("[Hand Gesture] Right fist + thumb up detected");

        CheckBothThumbsUp();
    }

    public void OnRightFistIndexEnded()
    {
        rightIndexUp = false;
        Debug.Log("[Hand Gesture] Right index ended");
    }

    public void OnRightFistIndexPerformed()
    {
        rightIndexUp = true;
        Debug.Log("[Hand Gesture] Right index detected");

        CheckBothThumbsUp();
    }

    public void OnRightFistThumbUpEnded()
    {
        rightThumbUp = false;
        Debug.Log("[Hand Gesture] Right fist + thumb up ended");
    }

    private void CheckBothFists()
    {
        if (leftFist && rightFist)
        {
            Debug.Log("[Hand Gesture] BOTH FISTS detected — drone control would start here");
        }
    }

    private void CheckBothThumbsUp()
    {
        if (leftThumbUp && rightThumbUp)
        {
            Debug.Log("[Hand Gesture] BOTH FISTS + BOTH THUMBS UP detected");
        }
    }
}