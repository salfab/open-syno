namespace OpenSyno.Services
{
    using System;

    public class PanoramaItemSwitchingService : IPanoramaItemSwitchingService
    {
        public event EventHandler<ActiveItemChangeRequestedEventArgs> ActiveItemChangeRequested;

        public void RequestActiveItemChange(int index)
        {
            OnActiveItemChangeRequested(new ActiveItemChangeRequestedEventArgs {NewItemIndex = index});
        }

        private void OnActiveItemChangeRequested(ActiveItemChangeRequestedEventArgs eventArgs)
        {
            if (ActiveItemChangeRequested != null)
            {
                ActiveItemChangeRequested(this, eventArgs);
            }
        }
    }
}