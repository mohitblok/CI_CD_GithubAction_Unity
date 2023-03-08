using Bloktopia.XR;
using Photon.Voice.Unity;

namespace Bloktopia
{
    /// <summary>
    /// Manager class for the voice chat, needs to live on the Network Runner.
    /// </summary>
    public class VoiceManager : MonoSingleton<VoiceManager>
    {
        public VoiceConnection voiceConnection;
        public Recorder recorder;

        /// <summary>
        /// This stops and starts voice transmission depending on the status of the XR connection.
        /// </summary>
        /// <param name="connection"></param>
        private void StopTransmission(XRConnectionManager.XRConnection connection)
        {
            switch (connection)
            {
                case XRConnectionManager.XRConnection.XR_Disconnected or XRConnectionManager.XRConnection.XR_Removed:
                    recorder.TransmitEnabled = false;
                    recorder.RecordingEnabled = false;
                    break;
                case XRConnectionManager.XRConnection.XR_Added:
                    recorder.TransmitEnabled = true;
                    recorder.RecordingEnabled = true;
                    break;
            }
        }

        private void OnEnable()
        {
            if (XRConnectionManager.Instance)
                XRConnectionManager.Instance.onXRConnectionChange += StopTransmission;
        }

        private void OnDisable()
        {
            if (XRConnectionManager.Instance)
                XRConnectionManager.Instance.onXRConnectionChange -= StopTransmission;
        }
    }
}