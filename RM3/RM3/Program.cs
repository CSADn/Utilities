using Broadlink.NET;
using Newtonsoft.Json;

namespace RM3
{
    internal class Program
    {
        private static readonly string _commandsFile = @"C:\Users\Inspiron15\AppData\Roaming\Broadlink\Commands.json";
        private static List<RMDevice> _devices = new();

        static async Task Main(string[] args)
        {
            if (!args.Any())
                return;

            var commands = JsonConvert.DeserializeObject<List<Command>>(File.ReadAllText(_commandsFile));
            var command = commands.FirstOrDefault(f => f.Key.Equals(args[0]));

            if (command == null)
            {
                Console.WriteLine($"Command '{args[0]}' not found.");
                return;
            }

            var flag = true;
            var client = new Client();

            client.DeviceHandler += async (sender, e) =>
            {
                var rm = (e as RMDevice);

                if (rm == null)
                    return;

                rm.OnDeviceReady += async (sender, e) =>
                {
                    Console.WriteLine($"Sending command '{command.Key}'...");
                    await rm.SendRemoteCommandAsync(command.Code.HexToBytes());

                    flag = false;
                };

                await rm.AuthorizeAsync();

                _devices.Add(rm);

                Console.WriteLine($"Found: {e} ({e.EndPoint.Address})");
            };

            Console.WriteLine("Looking for devices...");

            await client.DiscoverAsync();

            var n = 0;

            while (flag && n < 5)
            {
                n++;
                await Task.Delay(1000);
            }
        }
    }
}
