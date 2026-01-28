using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WaterAssessment.Messages
{
    public class LocationTypeUpdatedMessage : ValueChangedMessage<LocationType>
    {
        public LocationTypeUpdatedMessage(LocationType value) : base(value)
        {
        }
    }
}
