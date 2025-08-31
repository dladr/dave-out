using Sirenix.OdinInspector;
using System.Threading.Tasks;
using UnityEngine;

public class RingController : MonoBehaviour
{
    public float CurrentRingXPosition;
    public float RingMaxXPosition;
    public float RingMinXPosition;
    public float MoveDistance;
    public float MoveTime;
    public bool isMoving;

    [Button]
    public async void MoveRingPosition(bool isMovingRight)
    {
        if (isMoving)
        {
            return;
        }

        isMoving = true;

        var moveDirection = isMovingRight ? -1 : 1;
        var moveValue = moveDirection * MoveDistance;
        var timeMoved = 0f;
        var originalPosition = CurrentRingXPosition;
        var targetPosition = CurrentRingXPosition + moveValue;
        if(targetPosition < RingMinXPosition)
        {
            targetPosition = RingMinXPosition;
        }

        if (targetPosition > RingMaxXPosition)
        {
            targetPosition = RingMaxXPosition;
        }

        var percentMoved = 0f;

        while (timeMoved < MoveTime) { 
            timeMoved += Time.deltaTime;
            percentMoved = timeMoved / MoveTime;
            CurrentRingXPosition = Mathf.Lerp(originalPosition, targetPosition, percentMoved);
            transform.localPosition = transform.localPosition.Modify(x: CurrentRingXPosition);
            await Task.Delay(1);
        }

        CurrentRingXPosition = targetPosition;
        transform.localPosition = transform.localPosition.Modify(x: CurrentRingXPosition);
        isMoving = false;
    }
}
