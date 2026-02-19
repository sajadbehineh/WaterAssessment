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
