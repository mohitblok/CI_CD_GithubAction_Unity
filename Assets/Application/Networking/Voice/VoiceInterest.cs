using Photon.Realtime;
using UnityEngine;

namespace Bloktopia.VoiceChat
{
    public class VoiceInterest : MonoBehaviour
    {
        public void Start()
        {
            OnStateUpdateEvent();

            if (VoiceManager.Instance)
                VoiceManager.Instance.voiceConnection.Client.StateChanged += OnStateChanged;
        }

        private void OnStateChanged(ClientState lastClientState, ClientState curClientState)
        {
            if (lastClientState == ClientState.Joining && curClientState == ClientState.Joined)
            {
                OnStateUpdateEvent();
            }
        }

        private void OnStateUpdateEvent()
        {
            // can't updated until voice is connected to a room
            if (!VoiceManager.Instance || !VoiceManager.Instance.voiceConnection.Client.InRoom)
            {
                return;
            }
        }

        private void SetDefaultInterestGroups()
        {
            SetInterestGroups(new byte[0], new[] {(byte) 0});
        }

        private void SetInterestGroups(byte[] groupsToRemove, byte[] groupsToAdd)
        {
            VoiceManager.Instance.voiceConnection.Client.OpChangeGroups(groupsToRemove, groupsToAdd);
        }

        private void OnDestroy()
        {
            VoiceManager.Instance.voiceConnection.Client.StateChanged -= OnStateChanged;
        }
    }
}