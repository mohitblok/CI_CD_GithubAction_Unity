using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bloktopia.Avatar.LipSync
{
    public class RemoteOVRLipSyncContextMorphTarget : MonoBehaviour
    {
        public SkinnedMeshRenderer[] skinnedMeshRenderer = null;
        public OVRLipSyncContextBase lipsyncContext = null;

        // Set the blendshape index to go to (-1 means there is not one assigned)
        [Tooltip("Blendshape index to trigger for each viseme.")]
        public int[] visemeToBlendTargets = Enumerable.Range(0, OVRLipSync.VisemeCount).ToArray();


        // smoothing amount
        [Range(1, 100)] [Tooltip("Smoothing of 1 will yield only the current predicted viseme, 100 will yield an extremely smooth viseme response.")]
        public int smoothAmount = 70;

        private float morphTargetValue = 100;

        private void Start()
        {
            if (lipsyncContext == null)
            {
                Debug.LogError("LipSyncContextMorphTarget.Start Error: " + "No OVRLipSyncContext component on this object!");
                return;
            }
            
            lipsyncContext.Smoothing = smoothAmount;
        }

        private void Update()
        {
            if (!LipSyncAndSkinArePresent())
            {
                return;
            }
            
            OVRLipSync.Frame frame = lipsyncContext.GetCurrentPhonemeFrame();
            if (frame != null)
            {
                SetVisemeToMorphTarget(frame);
            }
            
            UpdateLipSyncSmoothingValue();
        }
        
        public void OnSkinBound(SkinnedMeshRenderer[] skinnedMesh)
        {
            skinnedMeshRenderer = RetrieveSkinnedMeshRenderersWithVisemes(skinnedMesh);
            
            LipSyncBlendShapeIndexBuilder.Build(this, LipSyncBlendShapeIndexBuilder.ReadyPlayerMe_VisemeTargets);
        }


        private bool LipSyncAndSkinArePresent()
        {
            return lipsyncContext != null && skinnedMeshRenderer != null;
        }
        
        private void SetVisemeToMorphTarget(OVRLipSync.Frame frame)
        {
            for (var i = 0; i < visemeToBlendTargets.Length; i++)
            {
                if (visemeToBlendTargets[i] == -1)
                {
                    continue;
                }
                
                ConvertVisemeBlendWeightToPercentage(frame, i);
            }
        }

        private void UpdateLipSyncSmoothingValue()
        {
            if (smoothAmount != lipsyncContext.Smoothing)
            {
                lipsyncContext.Smoothing = smoothAmount;
            }
        }
        
        private SkinnedMeshRenderer[] RetrieveSkinnedMeshRenderersWithVisemes(SkinnedMeshRenderer[] skinnedMesh)
        {
            return skinnedMesh.Where(renderer =>
            {
                if (renderer == null) return false;
                if (renderer.sharedMesh == null) return false;
                return renderer.sharedMesh.GetBlendShapeIndex("viseme_FF") != -1;
            }).ToArray();
        }

        private void ConvertVisemeBlendWeightToPercentage(OVRLipSync.Frame frame, int index)
        {
            foreach (var renderer in skinnedMeshRenderer)
            {
                renderer.SetBlendShapeWeight(visemeToBlendTargets[index], frame.Visemes[index] * morphTargetValue);
            }
        }
    }
}