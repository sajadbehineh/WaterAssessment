using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WaterAssessment.Messages
{
    public class AreaDeletedMessage : ValueChangedMessage<Area>
    {
        public AreaDeletedMessage(Area value) : base(value)
        {
        }
    }
}
