using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class Message
    {
        public int Id { get; set; } = GeneratorID.GetMessageID();
        public int SenderId { get; set; } = 0;
        public MessageType MessageType { get; set; } = MessageType.Text;
        public string Content { get; set; } = "";
        public Message() {
            
        }

        public Message(MessageType messageType, int senderId, string content)
        {
            SenderId = senderId;
            MessageType = messageType;
            Content = content;
        }
    }
}
