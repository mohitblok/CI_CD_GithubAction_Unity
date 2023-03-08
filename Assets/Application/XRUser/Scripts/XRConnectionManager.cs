using System;
using Bloktopia.Utils.Patterns;
using UnityEngine.InputSystem;
using UnityEngine.XR.Management;

namespace Bloktopia.XR
{
    public class XRConnectionManager : MonoSingletonPersistant<XRConnectionManager>
    {
        public enum XRConnection
        {
            XR_Connected,
            XR_Added,
            XR_Removed,
            XR_Disconnected,
        }

        public event Action<XRConnection> onXRConnectionChange;

        public XRConnection ConnectionState { get; private set; }

        private static readonly string XR_Head_Device_description = "OpenXR Head Tracking"; 
        
        private void OnEnable()
        {
            InputSystem.onDeviceChange += SearchForXRDevice;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= SearchForXRDevice;
        }

        private void SearchForXRDevice(InputDevice device, InputDeviceChange change)
        {
            var newConnectionState = change switch
            {
                InputDeviceChange.Reconnected when CheckDeviceForXRHeadTracking(device) => XRConnection.XR_Connected,
                InputDeviceChange.Added when CheckDeviceForXRHeadTracking(device) => XRConnection.XR_Added,
                InputDeviceChange.Removed when CheckDeviceForXRHeadTracking(device) => XRConnection.XR_Removed,
                InputDeviceChange.Disconnected when CheckDeviceForXRHeadTracking(device) => XRConnection.XR_Disconnected,
                _ => ConnectionState
            };
            
            if (ConnectionState == newConnectionState) { return; }

            ConnectionState = newConnectionState;
            onXRConnectionChange?.Invoke(ConnectionState);
        }

        private static bool CheckDeviceForXRHeadTracking(InputDevice device)
        {
            return device.description.ToString().Contains(XR_Head_Device_description);
        }

        public static void StartXR()
        {
            XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }

        public static void StopXR()
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }   
}