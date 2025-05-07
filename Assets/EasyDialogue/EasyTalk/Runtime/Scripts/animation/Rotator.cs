using UnityEngine;

namespace EasyTalk.Animation
{
    /// <summary>
    /// A component which can be used to automatically rotate an object around an axis over time.
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        /// <summary>
        /// The rotation vector (in Euler angles).
        /// </summary>
        [Tooltip("The rotation vector (in Euler angles).")]
        [SerializeField]
        private Vector3 rotationVector;

        /// <summary>
        /// The rotation speed.
        /// </summary>
        [Tooltip("The rotation speed.")]
        [SerializeField]
        private float rotationSpeed = 1.0f;

        /// <summary>
        /// Updates the rotation of the object.
        /// </summary>
        void Update()
        {
            this.transform.Rotate(rotationVector * rotationSpeed * Time.deltaTime);
        }
    }
}