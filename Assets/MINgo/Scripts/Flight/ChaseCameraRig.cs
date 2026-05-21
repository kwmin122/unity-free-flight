using UnityEngine;

namespace MINgo.Flight
{
    public sealed class ChaseCameraRig : MonoBehaviour
    {
        public Transform target;
        public float followDistance = 8f;
        public float followHeight = 2.4f;
        public float sideOffset;
        public float lookAhead = 14f;
        public float lookHeight = 0.25f;
        public float pitchFollow = 0.22f;
        public float speedPullback = 2f;
        public float pullbackAtSpeed = 65f;
        public float smoothTime = 0.08f;
        public float rotationSmooth = 8f;
        public float minFieldOfView = 55f;
        public float maxFieldOfView = 66f;
        public float fieldOfViewAtSpeed = 85f;

        private Camera attachedCamera;
        private Vector3 velocity;
        private Quaternion currentRotation = Quaternion.identity;
        private bool hasSnapped;

        private void Awake()
        {
            attachedCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Rigidbody targetBody = target.GetComponent<Rigidbody>();
            float speed = targetBody == null ? 0f : targetBody.linearVelocity.magnitude;
            Vector3 predictedTargetPosition = target.position + (targetBody == null ? Vector3.zero : targetBody.linearVelocity * Time.fixedDeltaTime);
            Vector3 chaseForward = CalculateChaseForward(
                target.forward,
                targetBody == null ? Vector3.zero : targetBody.linearVelocity,
                pitchFollow);
            Quaternion desiredRotation = Quaternion.LookRotation(chaseForward, Vector3.up);

            if (!hasSnapped)
            {
                currentRotation = desiredRotation;
            }
            else
            {
                currentRotation = Quaternion.Slerp(currentRotation, desiredRotation, 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime));
            }

            float pullback = Mathf.Lerp(0f, speedPullback, Mathf.Clamp01(speed / pullbackAtSpeed));
            Vector3 desiredPosition = predictedTargetPosition
                + currentRotation * new Vector3(sideOffset, followHeight, -(followDistance + pullback));
            Vector3 lookTarget = predictedTargetPosition
                + chaseForward * lookAhead
                + Vector3.up * lookHeight;

            if (!hasSnapped)
            {
                transform.position = desiredPosition;
                hasSnapped = true;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
            }

            transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);

            if (attachedCamera != null)
            {
                attachedCamera.fieldOfView = Mathf.Lerp(minFieldOfView, maxFieldOfView, Mathf.Clamp01(speed / fieldOfViewAtSpeed));
            }
        }

        public static Vector3 CalculateChaseForward(Vector3 aircraftForward, Vector3 velocity, float pitchFollow)
        {
            Vector3 sourceForward = velocity.magnitude > 3f ? velocity.normalized : aircraftForward.normalized;
            Vector3 flatForward = Vector3.ProjectOnPlane(sourceForward, Vector3.up);
            if (flatForward.sqrMagnitude < 0.0001f)
            {
                flatForward = Vector3.ProjectOnPlane(aircraftForward, Vector3.up);
            }

            if (flatForward.sqrMagnitude < 0.0001f)
            {
                return Vector3.forward;
            }

            return Vector3.Slerp(flatForward.normalized, sourceForward.normalized, Mathf.Clamp01(pitchFollow)).normalized;
        }
    }
}
