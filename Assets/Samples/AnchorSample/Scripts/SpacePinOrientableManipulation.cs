// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.OpenXR.Sample;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Examples
{
    /// <summary>
    /// Component that adds MRTK object manipulation capabilities on top of the auto-orienting <see cref="Microsoft.MixedReality.WorldLocking.Core.SpacePinOrientable"/>.
    /// </summary>
    public class SpacePinOrientableManipulation : MonoBehaviour
    {
        #region Inspector fields

        [SerializeField] private ARAnchorWorldLocking m_ARAnchorLocking;

        [SerializeField] [Tooltip("Proxy renderable to show axis alignment during manipulations.")]
        private GameObject prefab_FeelerRay = null;

        /// <summary>
        /// Proxy renderable to show axis alignment during manipulations.
        /// </summary>
        public GameObject Prefab_FeelerRay
        {
            get { return prefab_FeelerRay; }
            set { prefab_FeelerRay = value; }
        }

        [SerializeField] [Tooltip("Whether to show the MRTK rotation gizmos.")]
        private bool allowRotation = true;

        /// <summary>
        /// Whether to show the MRTK rotation gizmos.
        /// </summary>
        /// <remarks>
        /// Rotating the SpacePinOrientableManipulation object only has any effect when the first
        /// pin is manipulated. Once the second object is manipulated, and ever after, the orientation
        /// is implied by the alignment of the pin objects, and actual orientation of the objects is ignored.
        /// </remarks>
        public bool AllowRotation
        {
            get { return allowRotation; }
            set { allowRotation = value; }
        }

        #endregion Inspector fields

        #region Internal fields

        /// <summary>
        /// Utility helper for setting up MRTK manipulation controls.
        /// </summary>
        private PinManipulator pinManipulator;

        private Pose restorePoseLocal = Pose.identity;

        #endregion Internal fields

        #region Unity methods

        /// <summary>
        /// Start(), and set up MRTK manipulation controls.
        /// </summary>
        protected void Start()
        {
            pinManipulator = new PinManipulator(transform, Prefab_FeelerRay, OnStartManipulation, OnFinishManipulation);
            pinManipulator.UserOriented = AllowRotation;
            pinManipulator.Startup();
        }

        /// <summary>
        /// Give the manipulation controls an update pulse. 
        /// </summary>
        private void Update()
        {
            pinManipulator.Update();
        }

        /// <summary>
        /// Shutdown the manipulation controls.
        /// </summary>
        protected void OnDestroy()
        {
            pinManipulator.Shutdown();
        }

        #endregion Unity methods

        #region Manipulation callback

        /// <summary>
        /// Callback for when the user starts manipulating the target.
        /// </summary>
        protected virtual void OnStartManipulation()
        {
        }

        /// <summary>
        /// Callback for when the user has finished positioning the target.
        /// </summary>
        protected virtual void OnFinishManipulation()
        {
            SetFrozenPose(ExtractpRootPoseFromTransform(), ExtractModelPoseFromTransform());
        }

        public void SetFrozenPose(Pose rootPose, Pose frozenPose)
        {
            var offsetPose = rootPose.Multiply(frozenPose.Inverse());
            m_ARAnchorLocking.SetOffsetPose(offsetPose);
            transform.SetLocalPose(restorePoseLocal);
        }

        protected Pose ExtractpRootPoseFromTransform()
        {
            return transform.root.GetGlobalPose();
        }

        protected Pose ExtractModelPoseFromTransform()
        {
            return transform.GetGlobalPose();
        }

        #endregion Manipulation callback
    }
}