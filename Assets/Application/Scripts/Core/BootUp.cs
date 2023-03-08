using Debugging.Admin;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

/// <summary>
/// Types of user our front end responsds differently to.
/// </summary>
public enum ClientType
{
    User,
    Editor,
    Bot,
}

/// <summary>
/// This is the core class of Bloktopia, Where different essential singleton instances are created for managers. Fusion Network is Initialised and this class also takes in ClientType from inspector to setup player and boots up differet systems accordingly.  
/// </summary>
public class BootUp : MonoBehaviour
{
    private GameObject player;

    /// <summary>
    /// Type of player we need to initialise while bootingUp. 
    /// </summary>
    public ClientType clientType = ClientType.User;

    private void Awake()
    {
        CoroutineUtil.CreateInstance();
        MainThreadDispatcher.CreateInstance();
        
        DomainManager.CreateInstance();
        DomainManager.Instance.Init(() =>
        {
            AssetBundleLoader.CreateInstance();
            WebRequestManager.CreateInstance();
            MainThreadDispatcher.CreateInstance();
            ContentManager.CreateInstance();
            AvatarLoader.CreateInstance();
            SceneLoadingManager.CreateInstance();
            LoadingManager.CreateInstance();
            SceneGraphManager.CreateInstance();
            SetupUI();
            SetUpPlayer();
            InitNetwork();
        });

        InteractionSystem.CreateInstance();
        InteractionSystem.Instance.Init(() =>
        {
            AdminPanelController.CreateInstance();
        });
    }

    private void InitNetwork()
    {
        var fusionNetworkManager = Instantiate(Resources.Load<GameObject>("FusionNetworkManager"));
        var networkLobbyUI = Instantiate(Resources.Load<GameObject>("UI/LobbyUI"));
        LobbyManager.CreateInstance();
    }
    
    private void SetupUI()
    {
        var ui = Instantiate(Resources.Load<GameObject>("UI/SpaceUI"));
    }

    private void SetUpPlayer()
    {
        switch (clientType)
        {
            case ClientType.User:
                InitialiseUser();
                break;
            case ClientType.Editor:
                InitialiseUser();
                break;
            case ClientType.Bot:
                InitialiseBot();
                break;
            default:
                InitialiseUser();
                break;
        }
    }

    private void InitialiseUser()
    {
#if !UNITY_EDITOR_OSX // windows only
        StartCoroutine(InitialiseXRSystem(OnInitialiseXRSystem));
#else
        OnInitialiseXRSystem();
#endif
    }

    private void  OnInitialiseXRSystem()
    {
        GameObject user = null;            

        if (!IsVRDevice())
        {
            //temporary Character controller to be replaced with a robust character later on, with Daves Input System.
            user = Instantiate(Resources.Load<GameObject>("SimpleCharacterController"));
        }
        else
        {
            user = Instantiate(Resources.Load<GameObject>("XROrigin"));
            var origin = user.ForceComponent<XROrigin>();
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;

            user.ForceComponent<LocomotionSystem>();
            user.ForceComponent<TeleportationProvider>();
        }
    }

    //TODO: add on bot code when its ready to be added
    private void InitialiseBot()
    {
        Instantiate(Resources.Load<GameObject>("XROrigin"));
    }

    private IEnumerator InitialiseXRSystem(Action callback)
    {
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Failed to init XR");
        }
        else
        {
            Debug.Log("Starting XR");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
            yield return null;
        }

        callback?.Invoke();
    }

    private bool IsVRDevice()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);
        return devices.Count > 0;
    }
}