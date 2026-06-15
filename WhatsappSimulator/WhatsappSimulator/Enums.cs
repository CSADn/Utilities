using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsappSimulator
{
    public enum BrokerType
    {
        Twilio = 1,
        BotMaker,
        ChatApi
    }

    public enum MessageType
    {
        Text = 1,
        Image,
        Audio,
        Video,
        Sticker,
        Contact,
        Location
    }
}
