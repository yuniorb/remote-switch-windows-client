namespace RemoteSwitchClient
{
    using System;
    using Quobject.SocketIoClientDotNet.Client;
    using Newtonsoft.Json.Linq;

    public class SwitchClient
    {
        private readonly Uri serverUrl;
        private readonly Socket socket;
        private const string SwitchEvent = "switch";
        private Action<SwitchEvent> listener;

        public SwitchClient(Uri serverUrl)
        {
            this.serverUrl = serverUrl;
            socket = IO.Socket(serverUrl);

            socket.On(Socket.EVENT_CONNECT, OnConnect);
            socket.On(SwitchEvent, OnSwitchEvent);
        }

        public void RegisterListener(Action<SwitchEvent> eventListener)
        {
            if (eventListener == null)
            {
                throw new ArgumentException("Listener cannot be null");
            }
            this.listener = eventListener;
        }  

        private void OnConnect()
        {
            Console.WriteLine(@"Connected to the server: {0}", serverUrl);
        }

        private void OnSwitchEvent(object data)
        {
            Console.WriteLine(@"Received switch event:" + data);
            var cast = (JObject) data;
            var evt = GetSwitchEvent(cast);
            listener?.Invoke(evt);
        }

        private SwitchEvent GetSwitchEvent(JObject data)
        {
            return new SwitchEvent()
            {
                Id = data["id"].Value<string>(),
                Name = data["name"].Value<string>(),
                EventDate = data["date"].Value<DateTime>(),
                Status = data["status"].Value<bool>()
            };
        }
    }
}
