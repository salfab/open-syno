namespace OpenSyno.Services
{
    using System;

    public interface IPanoramaItemSwitchingService
    {
        event EventHandler<ActiveItemChangeRequestedEventArgs> ActiveItemChangeRequested;

        void RequestActiveItemChange(int index);
    }

    public class ActiveItemChangeRequestedEventArgs : EventArgs
    {
        public int NewItemIndex { get; set; }
    }
}