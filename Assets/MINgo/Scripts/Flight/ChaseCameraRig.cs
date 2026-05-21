using UnityEngine;

namespace MINgo.Flight
{
    public sealed class ChaseCameraRig : MonoBehaviour
    {
        public Transform target;
        public float followDistance = 6.7f;
        public float followHeight = 2.05f;
        public float sideOffset;
        public float lookAhead = 14f;
        public float lookHeight = -0.1f;
        public float speedPullback = 0.9f;
        public float pullbackAtSpeed = 70f;
        public float smoothTime = 0.08f;
        public float rotationSmooth = 8f;
        public float minFieldOfView = 58f;
        public float maxFieldOfView = 68f;
        public float fieldOfViewAtSpeed = 80f;

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
            Quaternion desiredRotation = target.rotation;

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
                + currentRotation * new Vector3(0f, lookHeight, lookAhead);

            if (!hasSnapped)
            {
                transform.position = desiredPosition;
                hasSnapped = true;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
            }

            transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, currentRotation * Vector3.up);

            if (attachedCamera != null)
            {
                attachedCamera.fieldOfView = Mathf.Lerp(minFieldOfView, maxFieldOfView, Mathf.Clamp01(speed / fieldOfViewAtSpeed));
            }
        }
    }
}
