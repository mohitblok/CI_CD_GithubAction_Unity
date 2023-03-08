using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bloktopia.Utilities
{
    public static class SkinnedMeshRendererExtensions
    {
        public static void Transfer(this SkinnedMeshRenderer from, SkinnedMeshRenderer to)
        {
            from.bones = to.bones.ToArray();
            from.rootBone = to.rootBone;
        }

        public static void Rebind(this SkinnedMeshRenderer from, SkinnedMeshRenderer to)
        {
            if (!to) { return; }

            // Create a dictionary
            Dictionary<Transform, Transform> boneTransferMap = new();

            // Get bone lists
            List<Transform> fromBones = GetBoneTransforms(from);
            List<Transform> toBones = GetBoneTransforms(to);

            // Create a list to contain the rematched bones
            List<Transform> toBonesRematched = new();

            // Look through all bones in the "to" SkinnedMeshRenderer and find matching transform names in the "from" SkinnedMeshRenderer
            // Build a dictionary to match them up
            fromBones.ForEach(fromBone =>
            {
                toBones.ForEach(toBone =>
                {
                    if (fromBone.name == toBone.name)
                    {
                        boneTransferMap.Add(fromBone, toBone);
                    }
                });
            });

            // Build the new bone array from the dictionary
            fromBones.ForEach(fromBone => toBonesRematched.Add(boneTransferMap[fromBone]));

            // Apply rematched bones
            from.bones = toBonesRematched.ToArray();

            // Copy the shared material reference
            from.sharedMaterial = to.sharedMaterial;

            // Copy the root bone reference
            from.rootBone = to.rootBone;
        }

        private static List<Transform> GetBoneTransforms(SkinnedMeshRenderer smr)
        {
            if (smr == null) return new Transform[0].ToList();
            else return smr.bones.ToList();
        }
    }
}
