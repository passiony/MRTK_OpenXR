// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.ARFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

#if USE_ARFOUNDATION_5_OR_NEWER
using ARSessionOrigin = Unity.XR.CoreUtils.XROrigin;
#else
using ARSessionOrigin = UnityEngine.XR.ARFoundation.ARSessionOrigin;
#endif

namespace Microsoft.MixedReality.OpenXR.Sample
{
    [RequireComponent(typeof(ARAnchorManager))]
    [RequireComponent(typeof(ARSessionOrigin))]
    public class AnchorWordSample : MonoBehaviour
    {
        private ARSessionOrigin m_arSessionOrigin; // Used for ARSessionOrigin.trackablesParent
        private ARAnchorManager m_arAnchorManager;
        private XRAnchorStore m_anchorStore = null;

        private List<ARAnchor> m_anchors = new List<ARAnchor>();
        private Dictionary<TrackableId, string> m_incomingPersistedAnchors = new Dictionary<TrackableId, string>();

        public GameObject m_WorldAnchor;
        public const string WordAnchorName = "TheWordAnchor";

        protected async void OnEnable()
        {
            // Set up references in this script to ARFoundation components on this GameObject.
            m_arSessionOrigin = GetComponent<ARSessionOrigin>();
            if (!TryGetComponent(out m_arAnchorManager) || !m_arAnchorManager.enabled ||
                m_arAnchorManager.subsystem == null)
            {
                Debug.Log(
                    $"ARAnchorManager not enabled or available; sample anchor functionality will not be enabled.");
                return;
            }

            m_arAnchorManager.anchorsChanged += AnchorsChanged;

#if USE_MICROSOFT_OPENXR_PLUGIN_1_9_OR_NEWER
            m_anchorStore = await XRAnchorStore.LoadAnchorStoreAsync(m_arAnchorManager.subsystem);
#else
            m_anchorStore = await XRAnchorStore.LoadAsync(m_arAnchorManager.subsystem);
#endif
            if (m_anchorStore == null)
            {
                Debug.Log("XRAnchorStore not available, sample anchor persistence functionality will not be enabled.");
                return;
            }

            Debug.Log("PersitedName.Count = " + m_anchorStore.PersistedAnchorNames.Count);
            // Request all persisted anchors be loaded once the anchor store is loaded.
            foreach (string name in m_anchorStore.PersistedAnchorNames)
            {
                Debug.Log("PersistedAnchorNames:" + name);
                TrackableId trackableId = m_anchorStore.LoadAnchor(name);
                m_incomingPersistedAnchors.Add(trackableId, name);
            }
        }

        protected void OnDisable()
        {
            if (m_arAnchorManager != null)
            {
                m_arAnchorManager.anchorsChanged -= AnchorsChanged;
                m_anchorStore = null;
                m_incomingPersistedAnchors.Clear();
            }
        }

        private void AnchorsChanged(ARAnchorsChangedEventArgs eventArgs)
        {
            foreach (var added in eventArgs.added)
            {
                Debug.Log(
                    $"Anchor added from ARAnchorsChangedEvent: {added.trackableId}, OpenXR Handle: {added.GetOpenXRHandle()}");
                ProcessAddedAnchor(added);
            }

            foreach (ARAnchor updated in eventArgs.updated)
            {
                if (updated.TryGetComponent(out PersistableAnchorVisuals sampleAnchorVisuals))
                {
                    sampleAnchorVisuals.TrackingState = updated.trackingState;
                }
            }

            foreach (var removed in eventArgs.removed)
            {
                Debug.Log($"Anchor removed: {removed.trackableId}");
                m_anchors.Remove(removed);
            }
        }

        public void OnEndDrag()
        {
            ClearSceneAnchors();
            AnchorStoreClear();
            
            var anchor = AddAnchor(new Pose(m_WorldAnchor.transform.position, m_WorldAnchor.transform.rotation));
            PersistenceAnchor(anchor);
        }

        private void ProcessAddedAnchor(ARAnchor anchor)
        {
            // If this anchor being added was requested from the anchor store, it is recognized here
            if (m_incomingPersistedAnchors.TryGetValue(anchor.trackableId, out string name))
            {
                if (anchor.TryGetComponent(out PersistableAnchorVisuals sampleAnchorVisuals))
                {
                    sampleAnchorVisuals.Name = name;
                    sampleAnchorVisuals.Persisted = true;
                    sampleAnchorVisuals.TrackingState = anchor.trackingState;

                    if (name == WordAnchorName)
                    {
                        FixedWordAnchor(sampleAnchorVisuals);
                    }
                }

                m_incomingPersistedAnchors.Remove(anchor.trackableId);
            }

            m_anchors.Add(anchor);
        }

        void FixedWordAnchor(PersistableAnchorVisuals visualAnchor)
        {
            Debug.Log("恢复世界锚");

            m_WorldAnchor.transform.position = visualAnchor.transform.position;
            m_WorldAnchor.transform.rotation = visualAnchor.transform.rotation;
        }

        public ARAnchor AddAnchor(Pose pose)
        {
            ARAnchor newAnchor = m_arAnchorManager.AddAnchor(pose);
            if (newAnchor == null)
            {
                Debug.Log($"Anchor creation failed");
            }
            else
            {
                Debug.Log($"Anchor created: {newAnchor.trackableId}");
                m_anchors.Add(newAnchor);
            }

            return newAnchor;
        }

        public void PersistenceAnchor(ARAnchor anchor)
        {
            if (m_anchorStore == null)
            {
                Debug.Log($"Anchor Store was not available.");
                return;
            }
            
            anchor.name = WordAnchorName;
            // For the purposes of this sample, randomly generate a name for the saved anchor.
            string newName = anchor.name;
            bool succeeded = m_anchorStore.TryPersistAnchor(anchor.trackableId, newName);
            if (!succeeded)
            {
                Debug.Log($"Anchor could not be persisted: {anchor.trackableId}");
                return;
            }

            ChangeAnchorVisuals(anchor, newName, true);
        }

        private void ChangeAnchorVisuals(ARAnchor anchor, string newName, bool isPersisted)
        {
            PersistableAnchorVisuals sampleAnchorVisuals = anchor.GetComponent<PersistableAnchorVisuals>();
            Debug.Log(isPersisted
                ? $"Anchor {anchor.trackableId} with name {newName} persisted"
                : $"Anchor {anchor.trackableId} with name {sampleAnchorVisuals.Name} unpersisted");
            sampleAnchorVisuals.Name = newName;
            sampleAnchorVisuals.Persisted = isPersisted;
        }

        public void AnchorStoreClear()
        {
            m_anchorStore?.Clear();
            // Change visual for every anchor in the scene
            foreach (ARAnchor anchor in m_anchors)
            {
                ChangeAnchorVisuals(anchor, "", false);
            }
        }

        public void ClearSceneAnchors()
        {
            // Remove every anchor in the scene. This does not affect their persistence
            foreach (ARAnchor anchor in m_anchors)
            {
                m_arAnchorManager.subsystem.TryRemoveAnchor(anchor.trackableId);
            }

            m_anchors.Clear();
        }
    }
}