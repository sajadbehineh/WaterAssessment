using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WaterAssessment.Messages
{
    public class LocationTypeAddedMessage:ValueChangedMessage<LocationType>
    {
        public LocationTypeAddedMessage(LocationType value) : base(value)
        {
        }
    }
}
