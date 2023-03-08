using System;
using System.Linq;
using Bloktopia.Avatar.LipSync;
using Bloktopia.Utilities;
using RootMotion.FinalIK;
using UnityEngine;

/// <summary>
/// An essential building block for building a working <see cref="ReadyPlayerMe"/> avatar, used to construct the avatar for each networked user, binding the <see cref="VRIK"/>
/// </summary>
/// <remarks>
/// This class relies heavily on the <see cref="Transform"/> offsets supplied in the editor window. The <see cref="headOffset"/>, <see cref="leftHandOffset"/> and <see cref="rightHandOffset"/>
/// are used to populate the <see cref="VRIK.solver"/> with correct references, enabling the <see cref="VRIK"/> to function in a light as expected. 
/// </remarks>
public class Avatar : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform headOffset;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform leftHandOffset;
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform rightHandOffset;
    [SerializeField] private RuntimeAnimatorController animatorController;

    private const string MASCULINE_ANIMATION_AVATAR_NAME = "AnimationAvatars/MasculineAnimationAvatar";
    private const string FEMININE_ANIMATION_AVATAR_NAME = "AnimationAvatars/FeminineAnimationAvatar";

    private Action OnAvatarLoadedCallback;
    
    /// <summary>
    /// Used to subscribe a callback to the private <see cref="OnAvatarLoadedCallback"/> action
    /// </summary>
    /// <param name="callback"> the <see cref="Action"/> subscribing to the <see cref="OnAvatarLoadedCallback"/></param>
    public void AddAvatarLoadedCallback(Action callback)
    {
        OnAvatarLoadedCallback += callback;
    }
    
    /// <summary>
    /// Used to unsubscribe a callback to the private <see cref="OnAvatarLoadedCallback"/> action
    /// </summary>
    /// <param name="callback"> the <see cref="Action"/> unsubscribing to the <see cref="OnAvatarLoadedCallback"/></param>
    public void RemoveAvatarLoadedCallback(Action callback)
    {
        OnAvatarLoadedCallback -= callback;
    }

    /// <summary>
    /// Requires the URL of the <see cref="ReadyPlayerMe"/> avatar to load and loads it through <see cref="AvatarLoader.LoadAvatar"/>
    /// </summary>
    /// <param name="url">The URL of the <see cref="ReadyPlayerMe"/> avatar to load</param>
    public void LoadAvatar(string url)
    {
        Debug.Log(url);
        AvatarLoader.Instance.LoadAvatar(url, this);
    }

    /// <summary>
    /// Used by the <see cref="AvatarLoader.AvatarItem"/> to populate the Lods of the avatar and bind necessary components
    /// </summary>
    /// <remarks>
    /// This method is heavily tied to the <see cref="AvatarLoader"/> & <see cref="AvatarLoader.AvatarItem"/> classes and is used to bind the lods loaded from
    /// the <see cref="AvatarLoader.AvatarItem"/>.uri to the provided references, <see cref="VRIK"/>, <see cref="OVRLipSync"/>, and provided animation sets.
    ///
    /// It exists within this class with public accessibility as it requires access to components and methods within this class to function as anticipated.
    /// As this is a method callback function, it's use outside of this is limited and it's wise to not access this method unless a new system is to be drawn up. 
    /// </remarks>
    /// <param name="avatarItem"></param>
    public void OnAvatarLoaded(AvatarLoader.AvatarItem avatarItem)
    {
        ParentLods(avatarItem.Lods);
        RebindSkin(avatarItem.Lods);
        SetupLods(avatarItem.Lods);
        SetupAnimator(avatarItem);
        SetupLipSync();
        SetupVrIk(transform.Search("Model").gameObject);
        OnAvatarLoadedCallback?.Invoke();
    }

    private void ParentLods(GameObject[] lods)
    {
        for (var index = 0; index < lods.Length; index++)
        {
            var lod = lods[index];
            lod.transform.parent = transform.Search("Model");
            lod.transform.localPosition = Vector3.zero;
            lod.SetActive(true);
        }
    }

    private void SetupAnimator(AvatarLoader.AvatarItem avatarItem)
    {
        var animationAvatarSource = avatarItem.json["outfitGender"].Value == "masculine"
            ? MASCULINE_ANIMATION_AVATAR_NAME
            : FEMININE_ANIMATION_AVATAR_NAME;
        UnityEngine.Avatar animationAvatar = Resources.Load<UnityEngine.Avatar>(animationAvatarSource);
        var animator = transform.Search("Model").gameObject.transform.AddComponent<Animator>();
        animator.avatar = animationAvatar;
        animator.applyRootMotion = true;
        animator.runtimeAnimatorController = animatorController;
    }

    //TODO: move to network user and init on avatar ready callback
    private void SetupLipSync()
    {
        var speaker = transform.Search("Speaker")?.gameObject;
        
        if(speaker == null)
            return;

        RemoteOVRLipSyncContext remoteOVRLipSyncContext = speaker.ForceComponent<RemoteOVRLipSyncContext>();
        RemoteOVRLipSyncContextMorphTarget remoteOvrLipSyncContextMorphTarget =
            speaker.ForceComponent<RemoteOVRLipSyncContextMorphTarget>();

        remoteOvrLipSyncContextMorphTarget.lipsyncContext = remoteOVRLipSyncContext;
        remoteOVRLipSyncContext.audioSource = speaker.GetComponent<AudioSource>();

        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        remoteOvrLipSyncContextMorphTarget.OnSkinBound(skinnedMeshRenderers);
    }
    
    //TODO: move to network user and init on avatar ready callback
    private void SetupVrIk(GameObject avatarGO)
    {
        VRIK vrik = avatarGO.AddComponent<VRIK>();
        vrik.AutoDetectReferences();
        vrik.solver.spine.headTarget = headOffset;
        vrik.solver.leftArm.target = leftHandOffset;
        vrik.solver.rightArm.target = rightHandOffset;
        vrik.solver.locomotion.mode = IKSolverVR.Locomotion.Mode.Animated;
        vrik.solver.locomotion.weight = 0.5f;
    }

    private void RebindSkin(GameObject[] lods)
    {
        var mainSkin = lods[0].GetComponentsInChildren<SkinnedMeshRenderer>(true);

        for (var index = 1; index < lods.Length; index++)
        {
            var otherSkin = lods[index].GetComponentsInChildren<SkinnedMeshRenderer>(true);
            ApplyLoddedAvatarVisualsToTemplate(mainSkin, otherSkin);
        }
    }

    private void SetupLods(GameObject[] lods)
    {
        LODGroup lodGroup = gameObject.ForceComponent<LODGroup>();

        lodGroup.SetLODs(new[]
        {
            new LOD(0.90f, lods[0].GetComponentsInChildren<Renderer>()),
            new LOD(0.70f, lods[1].GetComponentsInChildren<Renderer>()),
            new LOD(0.20f, lods[2].GetComponentsInChildren<Renderer>()),
        });
    }

    private void ApplyLoddedAvatarVisualsToTemplate(SkinnedMeshRenderer[] mainSkinnedLod,
        SkinnedMeshRenderer[] otherLods)
    {
        // Loop through all lod SkinnedMeshRenderers and find the matchings primary SkinnedMeshRenderer, then rebind the LOD-SMR to the primary SMR
        foreach (var lod in otherLods)
        {
            var targetPrimary = mainSkinnedLod.FirstOrDefault(primary => lod.name.Contains(primary.name));

            if (targetPrimary)
            {
                lod.Rebind(targetPrimary);
            }
        }
    }
}