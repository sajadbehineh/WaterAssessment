using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WaterAssessment.Messages
{
    public class LocationTypeAddedMessage : ValueChangedMessage<LocationType>
    {
        public LocationTypeAddedMessage(LocationType value) : base(value)
        {
        }
    }
}
