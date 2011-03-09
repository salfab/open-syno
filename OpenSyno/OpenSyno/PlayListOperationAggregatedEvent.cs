using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Events;

namespace OpenSyno
{
    public class PlayListOperationAggregatedEvent 
    {
        public PlayListOperationAggregatedEvent(PlayListOperation operation, IEnumerable<TrackViewModel> selectedItems)
        {
            Operation = operation;
            Items = selectedItems;
        }

        public PlayListOperation Operation { get; set; }
        public IEnumerable<TrackViewModel> Items { get; set; }
    }
}