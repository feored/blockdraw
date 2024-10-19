using PhotinoNET;
using PhotinoNET.Server;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;


namespace BlockDraw
{
    class Message
    {
        public string Command { get; set; }
        public string Data { get; set; }

        public static string Error(string error)
        {
            return JsonSerializer.Serialize(new Message
            {
                Command = "error",
                Data = error
            });
        }

        public Message(string command = "", string data = "")
        {
            Command = command;
            Data = data;
        }
    };


    class Program
    {
        static JsonSerializerOptions json_options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        static BlockBuilder builder = null;
        [STAThread]
        static void Main(string[] args)
        {

            PhotinoServer
            .CreateStaticFileServer(args, out string baseUrl)
            .RunAsync();

            // Window title declared here for visibility
            string windowTitle = "Blockdraw";

            Console.WriteLine($"Starting {windowTitle}...");
            // Creating a new PhotinoWindow instance with the fluent API
            var window = new PhotinoWindow()
                .SetTitle(windowTitle)
                // Resize to a percentage of the main monitor work area
                .SetUseOsDefaultSize(true)
                // Center window in the middle of the screen
                .Center()
                // Users can resize windows by default.
                // Let's make this one fixed instead.
                .SetResizable(true)
                // .RegisterCustomSchemeHandler("app", (object sender, string scheme, string url, out string contentType) =>
                // {
                //     contentType = "text/javascript";
                //     return new MemoryStream(Encoding.UTF8.GetBytes(@"
                //         (() =>{
                //             window.setTimeout(() => {
                //                 alert(`🎉 Dynamically inserted JavaScript.`);
                //             }, 1000);
                //         })();
                //     "));
                // })
                // Most event handlers can be registered after the
                // PhotinoWindow was instantiated by calling a registration 
                // method like the following RegisterWebMessageReceivedHandler.
                // This could be added in the PhotinoWindowOptions if preferred.
                .RegisterWebMessageReceivedHandler((object sender, string message) =>
                {
                    var window = (PhotinoWindow)sender;

                    // The message argument is coming in from sendMessage.
                    // "window.external.sendMessage(message: string)

                    // Handle the message
                    HandleMessage(window, message);
                    // Send a message back the to JavaScript event handler.
                    // "window.external.receiveMessage(callback: Function)"
                    //window.SendWebMessage(response);
                })
                .Load($"{baseUrl}/index.html"); // Can be used with relative path strings or "new URI()" instance to load a website.


            window.WaitForClose(); // Starts the application event loop
        }

        static void HandleMessage(PhotinoWindow window, string jsonMessage)
        {
            Console.WriteLine($"Received message: {jsonMessage}");
            if (string.IsNullOrWhiteSpace(jsonMessage))
            {
                Console.WriteLine("Received empty message.");
                return;
            }
            Message message = JsonSerializer.Deserialize<Message>(jsonMessage, json_options);
            Console.WriteLine(message);
            switch (message.Command)
            {
                case "exit":
                    Environment.Exit(0);
                    break;
                case "default_map_location":
                    SendMessage(window, "default_map_location", BlockBuilder.GetDefaultMapLocation());
                    break;
                case "open_map":
                    OpenMap(window, message.Data);
                    break;
                default:
                    Console.WriteLine($"Received unknown command: {message.Command}");
                    break;
            }
        }

        static string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, json_options);
        }

        static void SendMessage(PhotinoWindow window, string command, object data)
        {
            window.SendWebMessage(Serialize(new Message(command, Serialize(data))));
        }

        static void SendError(PhotinoWindow window, string error)
        {
            SendMessage(window, "error", error);
        }

        static void OpenMap(PhotinoWindow window, string url)
        {
            try
            {
                builder = new BlockBuilder(url);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error opening map: {e.Message}");
                SendError(window, e.Message);
                return;
            }
            MapInfo mapInfo = builder.GetMapInfo();
            SendMessage(window, "map_info", mapInfo);
        }
    }
}
