using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User = WaterAssessment.Models.User;

namespace WaterAssessment.Messages
{
    public class LoginSuccessMessage : ValueChangedMessage<User>
    {
        public LoginSuccessMessage(User user) : base(user) { }
    }
}
