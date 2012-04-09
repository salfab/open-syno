namespace Synology.AudioStationApi
{
    using System;

    public class VersionDependentResourcesProvider : IVersionDependentResourcesProvider
    {
        public string GetAudioSearchWebserviceRelativePath(DsmVersions dsmVersion)
        {
            switch (dsmVersion)
            {
                case DsmVersions.V4_0:
                    return "/webman/3rdparty/AudioStation/webUI/audio_browse.cgi";
                    break;
                case DsmVersions.V3_2:
                    return "/audio/webUI/audio_browse.cgi";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dsmVersion");
            }
        }

        public string GetAudioStreamWebserviceRelativePath(DsmVersions dsmVersion)
        {
            switch (dsmVersion)
            {
                case DsmVersions.V4_0:
                    return "/webman/3rdparty/AudioStation/webUI/audio_stream.cgi";
                    break;
                case DsmVersions.V3_2:
                    return "/audio/webUI/audio_stream.cgi";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dsmVersion");
            }
        }

        public string GetAudioStationWebserviceRelativePath(DsmVersions dsmVersion)
        {
            switch (dsmVersion)
            {
                case DsmVersions.V4_0:
                    return "/webman/3rdparty/AudioStation/webUI/audio.cgi";
                    break;
                case DsmVersions.V3_2:
                    return "/webman/modules/AudioStation/webUI/audio.cgi";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dsmVersion");
            }
        }
    }
}