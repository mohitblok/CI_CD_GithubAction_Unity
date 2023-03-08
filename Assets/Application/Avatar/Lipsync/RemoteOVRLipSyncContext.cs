namespace Bloktopia.Avatar.LipSync
{
    public class RemoteOVRLipSyncContext : OVRLipSyncContextBase
    {
        /// <summary>
        /// Raises the audio filter read event.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="channels">Channels.</param>
        void OnAudioFilterRead(float[] data, int channels)
        {
            ProcessAudioSamples(data, channels);
        }
        
        public void ProcessAudioSamples(float[] data, int channels)
        {
            // Do not process if we are not initialized, or if there is no
            // audio source attached to game object
            if ((OVRLipSync.IsInitialized() != OVRLipSync.Result.Success) ||
                audioSource == null)
            {
                return;
            }

            ProcessAudioSamplesRaw(data, channels);
        }

        /// <summary>
        /// Pass F32 PCM audio buffer to the lip sync module
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="channels">Channels.</param>
        public void ProcessAudioSamplesRaw(float[] data, int channels)
        {
            // Send data into Phoneme context for processing (if context is not 0)
            lock (this)
            {
                if (Context == 0 || OVRLipSync.IsInitialized() != OVRLipSync.Result.Success)
                {
                    return;
                }

                var frame = this.Frame;
                OVRLipSync.ProcessFrame(Context, data, frame, channels == 2);
            }
        }
    }
}