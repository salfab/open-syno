namespace Synology.AudioStationApi
{
    using System;

    public class VersionDependentResourcesProvider : IVersionDependentResourcesProvider
    {
        public string GetArtistSearchServiceRelativePath(DsmVersions dsmVersion)
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
    }
}