namespace OpenSyno.ViewModels
{
    public interface IUrlParameterToObjectsPlateHeater
    {
        object GetObjectForTicket(string urlParameterTicket);

        /// <summary>
        /// Registers the object.
        /// </summary>
        /// <param name="ticket">The ticket by which the object to register will be identified.</param>
        /// <param name="objectToRegister">The object to register.</param>
        void RegisterObject(string ticket, object objectToRegister);
    }
}