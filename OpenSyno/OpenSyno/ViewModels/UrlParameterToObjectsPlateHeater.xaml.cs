namespace OpenSyno
{
    using System;
    using System.Collections.Generic;

    using OpenSyno.ViewModels;

    using Synology.AudioStationApi;

    public class UrlParameterToObjectsPlateHeater : IUrlParameterToObjectsPlateHeater
    {
        /// <summary>
        /// Dictionary holding the mapping between the ticket-strings passed on the URL parameters and the objects that they identify and that need to be passed.
        /// </summary>
        private Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        public object GetObjectForTicket(string artistTicket)
        {
            var linkedObject = _dictionary[artistTicket];
            _dictionary.Remove(artistTicket);
            return linkedObject;
        }

        /// <summary>
        /// Registers the object.
        /// </summary>
        /// <param name="ticket">The ticket by which the object to register will be identified.</param>
        /// <param name="objectToRegister">The object to register.</param>
        public void RegisterObject(string ticket, object objectToRegister)
        {
            //if (!_dictionary.ContainsKey(ticket))
            //{
            //    _dictionary.Add(ticket, objectToRegister);                
            //}
            //else
            //{
                _dictionary[ticket] = objectToRegister;
            // }
        }
    }
}