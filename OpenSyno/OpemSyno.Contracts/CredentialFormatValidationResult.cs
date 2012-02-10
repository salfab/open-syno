namespace OpemSyno.Contracts
{
    public enum CredentialFormatValidationResult
    {
        Valid, 
        HostEmpty,
        InvalidHostFormat,
        InvalidPort,
        EmptyUsernamePassword,
        PortIncludedInHostname
    }
}