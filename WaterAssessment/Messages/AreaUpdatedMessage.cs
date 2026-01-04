using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WaterAssessment.Messages
{
    public class AreaUpdatedMessage : ValueChangedMessage<Area>
    {
        public AreaUpdatedMessage(Area value) : base(value)
        {
        }
    }
}
