using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Events;

namespace OpenSyno
{
    using OpemSyno.Contracts;

    using OpenSyno.ViewModels;

    public class PlayListOperationAggregatedEvent 
    {
        public PlayListOperationAggregatedEvent(PlayListOperation operation, IEnumerable<ITrackViewModel> selectedItems)
        {
            Operation = operation;
            Items = selectedItems;
        }

        public PlayListOperation Operation { get; set; }
        public IEnumerable<ITrackViewModel> Items { get; set; }
    }
}