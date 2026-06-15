using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace WhatsappSimulator
{
    public partial class Main : Form
    {
        private delegate void MessageDeliveryHandler(Message message);

        private Dictionary<int, MessageDeliveryHandler> _brokers;
        private List<Control> _validate;

        public Main()
        {
            InitializeComponent();

            _brokers = new Dictionary<int, MessageDeliveryHandler>
            {
                { (int)BrokerType.Twilio, TwilioSendMessage },
                { (int)BrokerType.BotMaker, BotMakerSendMessage },
                { (int)BrokerType.ChatApi, ChatAPISendMessage },
            };

            _validate = new List<Control>
            {
                cbPhoneFrom,
                tbPhoneTo,
                cbBroker,
                cbWebhook,
                tbMessageText
            };

            BuildControls();
            BindEvents();
            AddValidators();
        }

        private void BuildControls()
        {
            cbBroker.DataSource = Enum.GetNames(typeof(BrokerType))
                .Select(s => new
                {
                    Name = s,
                    Value = (int)Enum.Parse(typeof(BrokerType), s)
                })
                .ToList();

            cbBroker.DisplayMember = "Name";
            cbBroker.ValueMember = "Value";
        }

        private void BindEvents()
        {
            btSend.Click += (s, e) =>
            {
                BlockUI(true, "Enviando mensaje...");

                dynamic broker = cbBroker.SelectedItem;
                var m = new Message
                {
                    Endpoint = cbWebhook.Text,
                    Type = MessageType.Text,
                    PhoneFrom = cbPhoneFrom.Text,
                    PhoneTo = tbPhoneTo.Text,
                    Text = tbMessageText.Text
                };

                _brokers[broker.Value].Invoke(m);
            };
        }

        private void AddValidators()
        {
            foreach (var c in _validate)
                c.KeyUp += (s, e) =>
                {
                    btSend.Enabled = _validate.All(a => a.Text.Trim() != string.Empty);
                };
        }

        private void TwilioSendMessage(Message message)
        {
            var rc = new RestClient();
            var rr = new RestRequest(message.Endpoint);

            rr.AddParameter("MessageSid", BuildSID(), ParameterType.GetOrPost);
            rr.AddParameter("From", message.PhoneFrom, ParameterType.GetOrPost);
            rr.AddParameter("To", message.PhoneTo, ParameterType.GetOrPost);
            rr.AddParameter("Body", message.Text, ParameterType.GetOrPost);
            rr.AddParameter("SmsStatus", "received", ParameterType.GetOrPost);

            var response = rc.Post(rr);

            BlockUI();
        }

        private void BotMakerSendMessage(Message message)
        {
            throw new NotImplementedException();
        }

        private void ChatAPISendMessage(Message message)
        {
            throw new NotImplementedException();
        }


        private string BuildSID()
        {
            var md5 = new MD5CryptoServiceProvider();
            var buffer = Encoding.UTF8.GetBytes(DateTime.Now.ToString("hh:mm:ss.ffff"));
            var hash = md5.ComputeHash(buffer);

            return BitConverter.ToString(hash).Replace("-", "");
        }

        private void MakeRequest()
        {
            var url = cbWebhook.Text;
            var rc = new RestClient();
            var rr = new RestRequest(url);
            var response = rc.Post(rr);
         }

        private void BlockUI(bool flag = true, string status = "...")
        {
            foreach (var c in _validate)
                c.Enabled = flag;

            btSend.Enabled = flag;

            tsLbStatus.Text = status;
        }
    }
}
