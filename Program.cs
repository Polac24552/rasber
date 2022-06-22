using System;
using RestSharp;
using Newtonsoft.Json;
using System.Device.Gpio;
using System.Threading;

namespace rasber {

    class Program {
        public const string uri = "https://shkk.azurewebsites.net/api/SendRoomDataHttpTrigger?";
        public static GpioController controller = new GpioController();
        public static bool corridorState = false;
         
        static void Main() {

            const int GREEN_PIN = 3;
            const int PIR_PIN = 4;

            controller.OpenPin(GREEN_PIN, PinMode.Output);
            controller.OpenPin(PIR_PIN, PinMode.Input);
            controller.Write(GREEN_PIN, PinValue.Low);

            controller.RegisterCallbackForPinValueChangedEvent(PIR_PIN, PinEventTypes.Rising, (sender, args) =>
                {
                    corridorState = !corridorState;

                    if (corridorState == true) {
                        controller.Write(GREEN_PIN, PinValue.High);
                    }
                    else {
                        controller.Write(GREEN_PIN, PinValue.Low);
                    }

                    Console.WriteLine("Movement Detected"); 
                });

            while (true) {
                SendData();
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }

        public static void SendData() {
            RestClient client = new RestClient(uri);
            RestRequest request = new RestRequest();

            Console.WriteLine("------------------------------");
            var body = new {
                name = "Corridor",
                state = corridorState ? 1 : 0
            };
            Console.WriteLine("Corridor state: "+corridorState+", "+body.state);

            request.AddParameter("text/json", body, ParameterType.RequestBody);
            var response = client.Post(request);
            Console.WriteLine(response.Content);
            Console.WriteLine("------------------------------");
        }
    }
}