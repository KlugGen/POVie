using System;
using UnityEngine;
using UnityEngine.Events;

public class SwipeDetector : Singleton<SwipeDetector>
{
    [SerializeField] private float minDistanceForSwipe = 20f;
    
    private Vector2 fingerDownPosition;
    private Vector2 fingerUpPosition;

    public UnityAction<SwipeData> OnSwipeStarted;
    public UnityAction<SwipeData> OnSwipe;
    public UnityAction<SwipeData> OnSwipeEnded;

    private SwipeDirection lastDirection;
    private bool swipeStarted = false;
    private float swipeTimer = 0f;

    public void RegisterOnSwipeStarted(UnityAction<SwipeData> action) => OnSwipeStarted += action;

    public void UnregisterOnSwipeStarted(UnityAction<SwipeData> action) => OnSwipeStarted -= action;

    public void RegisterOnSwipe(UnityAction<SwipeData> action) => OnSwipe += action;

    public void UnregisterOnSwipe(UnityAction<SwipeData> action) => OnSwipe -= action;

    public void RegisterOnSwipeEnded(UnityAction<SwipeData> action) => OnSwipeEnded += action;

    public void UnregisterOnSwipeEnded(UnityAction<SwipeData> action) => OnSwipeEnded -= action;

    private void Update()
    {
        if (swipeStarted)
            swipeTimer += Time.deltaTime;

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                fingerUpPosition = touch.position;
                fingerDownPosition = touch.position;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                fingerDownPosition = touch.position;
                DetectSwipe();
            }

            if (touch.phase == TouchPhase.Ended)
            {
                fingerDownPosition = touch.position;
                DetectSwipe();

                if (swipeStarted)
                {
                    SendOnSwipe(lastDirection, SwipeState.Ended);
                    swipeStarted = false;
                    swipeTimer = 0f;
                }
            }
        }
    }

    private void DetectSwipe()
    {
        if (SwipeDistanceCheckMet())
        {
            SwipeDirection direction;

            if (IsVerticalSwipe())
                direction = fingerDownPosition.y - fingerUpPosition.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            else
                direction = fingerDownPosition.x - fingerUpPosition.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;

            lastDirection = direction;

            if (!swipeStarted)
            {
                swipeStarted = true;
                SendOnSwipe(direction, SwipeState.Started);
            }
            else SendOnSwipe(direction, SwipeState.InProgress);

            fingerUpPosition = fingerDownPosition;
        }
    }

    private bool IsVerticalSwipe()
    {
        return VerticalMovementDistance() > HorizontalMovementDistance();
    }

    private bool SwipeDistanceCheckMet()
    {
        return VerticalMovementDistance() > minDistanceForSwipe || HorizontalMovementDistance() > minDistanceForSwipe;
    }

    private float VerticalMovementDistance() => Mathf.Abs(fingerDownPosition.y - fingerUpPosition.y);

    private float HorizontalMovementDistance() => Mathf.Abs(fingerDownPosition.x - fingerUpPosition.x);

    private void SendOnSwipe(SwipeDirection direction, SwipeState state)
    {
        SwipeData swipeData = new SwipeData()
        {
            Direction = direction,
            StartPosition = fingerDownPosition,
            EndPosition = fingerUpPosition,
            SwipeTime = swipeTimer
        };

        switch (state)
        {
            case SwipeState.Started:
                OnSwipeStarted?.Invoke(swipeData);
                break;
            case SwipeState.InProgress:
                OnSwipe?.Invoke(swipeData);
                break;
            case SwipeState.Ended:
                OnSwipeEnded?.Invoke(swipeData);
                break;
        }
    }

    [NaughtyAttributes.Button("Simulate")]
    public void SwipeSimulation()
    {
        SwipeData sdata = new SwipeData();
        sdata.SwipeTime = 1f;
        OnSwipe?.Invoke(sdata);
    }

    private enum SwipeState
    {
        Started,
        InProgress,
        Ended
    }
}

public struct SwipeData
{
    public Vector2 StartPosition;
    public Vector2 EndPosition;
    public SwipeDirection Direction;
    public float SwipeTime;
}

public enum SwipeDirection
{
    Up,
    Down,
    Left,
    Right
}
