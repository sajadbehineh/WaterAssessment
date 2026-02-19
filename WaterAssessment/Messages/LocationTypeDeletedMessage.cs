using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WaterAssessment.Messages
{
    public class LocationTypeDeletedMessage : ValueChangedMessage<LocationType>
    {
        public LocationTypeDeletedMessage(LocationType value) : base(value)
        {
        }
    }
}
