namespace OpemSyno.Contracts
{
    public enum CredentialFormatValidationStatus
    {
        Valid, 
        HostEmpty,
        InvalidHostFormat,
        InvalidPort,
        EmptyUsernamePassword,
        PortIncludedInHostname
    }
}