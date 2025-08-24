using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public Transform ObjectToFollow;
    public Vector3 FollowOffset;
    public bool SetStartingRelativePositionToOffset;
    public bool MatchRotation;
    public bool FollowZ = true;
    public bool FollowY = true;
    public bool FollowX = true;

    public float FollowDelay = 0f; // Delay in seconds

    private struct MovementState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 LossyScale; // Added to handle scale
        public float Timestamp;

        public MovementState(Vector3 position, Quaternion rotation, Vector3 lossyScale, float timestamp)
        {
            Position = position;
            Rotation = rotation;
            LossyScale = lossyScale; // Store scale
            Timestamp = timestamp;
        }
    }

    private Queue<MovementState> movementHistory = new Queue<MovementState>();

    private void Awake()
    {
        if (ObjectToFollow != null && SetStartingRelativePositionToOffset)
        {
            // Store FollowOffset as a point in ObjectToFollow's local space
            FollowOffset = ObjectToFollow.InverseTransformPoint(transform.position);
        }
        // If SetStartingRelativePositionToOffset is false, FollowOffset is used as a world-space offset as before.
    }

    void LateUpdate()
    {
        if (ObjectToFollow == null)
        {
            return;
        }

        if (FollowDelay <= 0f)
        {
            // Immediate follow logic
            Vector3 immediateFollowTargetPosition;
            if (SetStartingRelativePositionToOffset)
            {
                // Transform local offset point to world space
                immediateFollowTargetPosition = ObjectToFollow.TransformPoint(FollowOffset);
            }
            else
            {
                // Apply world space offset
                immediateFollowTargetPosition = ObjectToFollow.position + FollowOffset;
            }

            var currentPosition = transform.position;
            transform.position = new Vector3(
                FollowX ? immediateFollowTargetPosition.x : currentPosition.x,
                FollowY ? immediateFollowTargetPosition.y : currentPosition.y,
                FollowZ ? immediateFollowTargetPosition.z : currentPosition.z);

            if (MatchRotation)
                transform.rotation = ObjectToFollow.rotation;
        }
        else
        {
            // Delayed follow logic
            movementHistory.Enqueue(new MovementState(ObjectToFollow.position, ObjectToFollow.rotation, ObjectToFollow.lossyScale, Time.time));

            bool applyUpdate = false;
            Vector3 historicalTargetPosition = Vector3.zero;
            Quaternion historicalTargetRotation = Quaternion.identity;
            Vector3 historicalTargetLossyScale = Vector3.one; // Default to one

            while (movementHistory.Count > 0 && movementHistory.Peek().Timestamp <= Time.time - FollowDelay)
            {
                MovementState stateToConsider = movementHistory.Dequeue();
                historicalTargetPosition = stateToConsider.Position;
                historicalTargetRotation = stateToConsider.Rotation;
                historicalTargetLossyScale = stateToConsider.LossyScale; // Get historical scale
                applyUpdate = true;
            }

            if (applyUpdate)
            {
                Vector3 delayedFollowTargetPosition;
                if (SetStartingRelativePositionToOffset)
                {
                    // Reconstruct historical transform and apply local offset
                    Matrix4x4 historicalTransformMatrix = Matrix4x4.TRS(historicalTargetPosition, historicalTargetRotation, historicalTargetLossyScale);
                    delayedFollowTargetPosition = historicalTransformMatrix.MultiplyPoint3x4(FollowOffset);
                }
                else
                {
                    // Apply world space offset to historical position
                    delayedFollowTargetPosition = historicalTargetPosition + FollowOffset;
                }
                
                var currentPosition = transform.position;

                transform.position = new Vector3(
                    FollowX ? delayedFollowTargetPosition.x : currentPosition.x,
                    FollowY ? delayedFollowTargetPosition.y : currentPosition.y,
                    FollowZ ? delayedFollowTargetPosition.z : currentPosition.z);

                if (MatchRotation)
                    transform.rotation = historicalTargetRotation; // Match historical world rotation
            }
            // If applyUpdate is false, it means no state is old enough yet, so the object doesn't move.
        }
    }
}
