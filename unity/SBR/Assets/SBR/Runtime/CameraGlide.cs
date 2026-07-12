using System.Collections;
using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// The one camera transition in the game: smoothstep position+rotation (and optionally FOV) over
    /// a wall-clock duration. Extracted from SitSpot for M4 so the couch sit and the desk focus share
    /// the exact glide players already tuned in playtests #3-#5.
    /// </summary>
    internal static class CameraGlide
    {
        /// <summary>Glides cam to the pose over duration seconds. When lens and toFov are given, the
        /// field of view eases along the same curve.</summary>
        public static IEnumerator Go(Transform cam, Vector3 toPosition, Quaternion toRotation,
                                     float duration, Camera lens = null, float toFov = 0f)
        {
            Vector3 fromPosition = cam.position;
            Quaternion fromRotation = cam.rotation;
            bool zoom = lens != null && toFov > 0f;
            float fromFov = zoom ? lens.fieldOfView : 0f;
            duration = Mathf.Max(0.01f, duration);
            float t = 0f;

            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / duration);
                float eased = t * t * (3f - 2f * t); // smoothstep
                cam.SetPositionAndRotation(
                    Vector3.Lerp(fromPosition, toPosition, eased),
                    Quaternion.Slerp(fromRotation, toRotation, eased));
                if (zoom) lens.fieldOfView = Mathf.Lerp(fromFov, toFov, eased);
                yield return null;
            }
        }
    }
}
