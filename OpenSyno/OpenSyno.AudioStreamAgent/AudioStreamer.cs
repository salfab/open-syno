using Microsoft.Phone.BackgroundAudio;

namespace OpenSyno.AudioStreamAgent
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;

    using Media;

    using OpenSyno.Contracts.Domain;
    using OpenSyno.Services;

    /// <summary>
    /// A background agent that performs per-track streaming for playback
    /// </summary>
    public class AudioTrackStreamer : AudioStreamingAgent
    {
        /// <summary>
        /// Called when a new track requires audio decoding
        /// (typically because it is about to start playing)
        /// </summary>
        /// <param name="track">
        /// The track that needs audio streaming
        /// </param>
        /// <param name="streamer">
        /// The AudioStreamer object to which a MediaStreamSource should be
        /// attached to commence playback
        /// </param>
        /// <remarks>
        /// To invoke this method for a track set the Source parameter of the AudioTrack to null
        /// before setting  into the Track property of the BackgroundAudioPlayer instance
        /// property set to true;
        /// otherwise it is assumed that the system will perform all streaming
        /// and decoding
        /// </remarks>
        protected override void OnBeginStreaming(AudioTrack track, AudioStreamer streamer)
        {
            // TODO : pass the mp3 uri in the string tag, along with the guid : that's ugly, but there's no way around it. maybe an XML serialization could make things smoother.
            Guid guid = ((GuidToTrackMapping)(new DataContractSerializer(typeof(GuidToTrackMapping)).ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(track.Tag))))).Guid;                            

            Stream audioStream = null;
            int size = 0;
            ReadWriteMemoryStream readWriteMemoryStream = new ReadWriteMemoryStream(size);
            Mp3MediaStreamSource mp3MediaStreamSource = new Mp3MediaStreamSource(readWriteMemoryStream);
            streamer.SetSource(mp3MediaStreamSource);
            NotifyComplete();
        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// The call to base.OnCancel() is necessary to release the background streaming resources
        /// </summary>
        protected override void OnCancel()
        {
            base.OnCancel();
        }
    }
}
