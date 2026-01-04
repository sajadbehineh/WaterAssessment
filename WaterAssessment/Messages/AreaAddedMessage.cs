using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WaterAssessment.Messages
{
    public class AreaAddedMessage : ValueChangedMessage<Area>
    {
        public AreaAddedMessage(Area value) : base(value)
        {
        }
    }
}
