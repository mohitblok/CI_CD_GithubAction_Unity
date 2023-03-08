using System.Collections.Generic;
using UnityEngine;

namespace Bloktopia.Avatar.LipSync
{
    /// <summary>
    /// Links <see cref="OVRLipSyncContextMorphTarget"/> to ReadyPlayerMe avatars.
    /// <para>This class finds the viseme BlendShapes on a SkinnedMeshRenderer and assigns the correct indexes to the <see cref="OVRLipSyncContextMorphTarget"/> </para>
    /// </summary>
    public static class LipSyncBlendShapeIndexBuilder
    {
        public static List<string> ReadyPlayerMe_VisemeTargets
        {
            get
            {
                List<string> newblendShapeNames = new();

                newblendShapeNames.Add("viseme_sil");
                newblendShapeNames.Add("viseme_PP");
                newblendShapeNames.Add("viseme_FF");
                newblendShapeNames.Add("viseme_TH");
                newblendShapeNames.Add("viseme_DD");
                newblendShapeNames.Add("viseme_kk");
                newblendShapeNames.Add("viseme_CH");
                newblendShapeNames.Add("viseme_SS");
                newblendShapeNames.Add("viseme_nn");
                newblendShapeNames.Add("viseme_RR");
                newblendShapeNames.Add("viseme_aa");
                newblendShapeNames.Add("viseme_E");
                newblendShapeNames.Add("viseme_I");
                newblendShapeNames.Add("viseme_O");
                newblendShapeNames.Add("viseme_U");

                return newblendShapeNames;
            }
        }

        /// <summary>
        /// Provide a <see cref="OVRLipSyncContextMorphTarget"/> component with a referenced skinnedMeshRenderer and this method will build the  <see cref="OVRLipSyncContextMorphTarget.visemeToBlendTargets"/> using custom viseme names
        /// </summary>
        /// <param name="visemeTargets">Custom list of viseme BlendShape names - Likely found on a SkinnedMeshRenderer</param>
        public static void Build(OVRLipSyncContextMorphTarget OVRLipSyncContextMorphTarget, List<string> visemeTargets)
        {
            List<int> blendShapeIndexes = new List<int>();

            visemeTargets.ForEach(name =>
                blendShapeIndexes.Add(
                    OVRLipSyncContextMorphTarget.skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(name)));

            OVRLipSyncContextMorphTarget.visemeToBlendTargets = blendShapeIndexes.ToArray();
        }

        public static void Build(RemoteOVRLipSyncContextMorphTarget OVRLipSyncContextMorphTarget,
            List<string> visemeTargets)
        {
            List<int> blendShapeIndexes = new List<int>();

            if (OVRLipSyncContextMorphTarget.skinnedMeshRenderer.Length == 0)
            {
                Debug.LogError("No SkinnedMeshRenderer found on RemoteOVRLipSyncContextMorphTarget");
                return;
            }

            visemeTargets.ForEach(name =>
                blendShapeIndexes.Add(OVRLipSyncContextMorphTarget.skinnedMeshRenderer[0].sharedMesh
                    .GetBlendShapeIndex(name)));

            OVRLipSyncContextMorphTarget.visemeToBlendTargets = blendShapeIndexes.ToArray();
        }
    }
}