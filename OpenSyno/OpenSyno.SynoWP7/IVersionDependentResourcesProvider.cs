namespace Synology.AudioStationApi
{
    public interface IVersionDependentResourcesProvider
    {
        string GetAudioSearchWebserviceRelativePath(DsmVersions dsmVersion);        
    }
}