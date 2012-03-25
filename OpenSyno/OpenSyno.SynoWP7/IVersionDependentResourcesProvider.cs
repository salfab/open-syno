namespace Synology.AudioStationApi
{
    public interface IVersionDependentResourcesProvider
    {
        string GetAudioSearchWebserviceRelativePath(DsmVersions dsmVersion);
        string GetAudioStreamWebserviceRelativePath(DsmVersions dsmVersion);
        string GetAudioStationWebserviceRelativePath(DsmVersions dsmVersion);
    }
}