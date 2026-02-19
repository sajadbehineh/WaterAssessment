using CommunityToolkit.Mvvm.Messaging.Messages;
using User = WaterAssessment.Models.User;

namespace WaterAssessment.Messages
{
    public class LoginSuccessMessage : ValueChangedMessage<User>
    {
        public LoginSuccessMessage(User user) : base(user) { }
    }
}
