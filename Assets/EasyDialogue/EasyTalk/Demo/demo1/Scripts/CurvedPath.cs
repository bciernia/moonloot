using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Demo
{
    /// <summary>
    /// Used to move an object along a curve over time.
    /// </summary>
    public class CurvedPath : MonoBehaviour
    {
        /// <summary>
        /// The object to move along the curve.
        /// </summary>
        [SerializeField]
        private GameObject movedObject;

        /// <summary>
        /// The start point of the curve.
        /// </summary>
        [SerializeField]
        private Transform start;

        /// <summary>
        /// The mid-point of the curve.
        /// </summary>
        [SerializeField]
        private Transform control;

        /// <summary>
        /// The end point of the curve.
        /// </summary>
        [SerializeField]
        private Transform end;

        /// <summary>
        /// The current position (between 0.0 and 1.0) along the path.
        /// </summary>
        [SerializeField]
        private float position = 0.0f;

        /// <summary>
        /// Whether the curved path follower is being played.
        /// </summary>
        [SerializeField]
        private bool isPlaying = false;

        /// <summary>
        /// Returns a Vector3 position which lies on the curve going from start to control to end based on the value provided.
        /// </summary>
        /// <param name="start">The start point of the curve.</param>
        /// <param name="control">The control/mid-point of the curve.</param>
        /// <param name="end">The end point of the curve.</param>
        /// <param name="amount">How far along the curve a position is to be retrieved for (from 0.0 to 1.0).</param>
        /// <returns></returns>
        public static Vector3 GetPoint(Vector3 start, Vector3 control, Vector3 end, float amount)
        {
            return Vector3.Lerp(Vector3.Lerp(start, control, amount), Vector3.Lerp(control, end, amount), amount);
        }

        void Update()
        {
            if (isPlaying)
            {
                Vector3 oldPos = movedObject.transform.position;
                movedObject.transform.position = GetPoint(start.position, control.position, end.position, position);
                Vector3 newPos = movedObject.transform.position;

                movedObject.transform.LookAt((newPos - oldPos) + newPos, movedObject.transform.up);
            }
        }
    }
}