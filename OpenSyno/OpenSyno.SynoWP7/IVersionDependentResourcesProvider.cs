namespace Synology.AudioStationApi
{
    internal interface IVersionDependentResourcesProvider
    {
        string GetArtistSearchServiceRelativePath(DsmVersions dsmVersion);
    }
}