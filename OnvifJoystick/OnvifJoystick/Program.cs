using QuickNV.Onvif;
using QuickNV.Onvif.PTZ;

namespace OnvifJoystick
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Configuración de la cámara
            Console.WriteLine("=== Control PTZ ONVIF ===");
            Console.WriteLine();

            Console.Write("Ingrese la IP de la cámara (ej: 192.168.1.100): ");
            string? host = Console.ReadLine();

            Console.Write("Ingrese el puerto (default 80): ");
            string? portInput = Console.ReadLine();
            int port = string.IsNullOrEmpty(portInput) ? 80 : int.Parse(portInput);

            //Console.Write("Ingrese el usuario: ");
            //string? username = Console.ReadLine();

            //Console.Write("Ingrese la contraseña: ");
            //string? password = ReadPassword();

            Console.WriteLine();
            Console.WriteLine("Conectando a la cámara...");

            var client = new OnvifClient(new OnvifClientOptions()
            {
                Scheme = "http",
                Host = host!,
                Port = port,
                //UserName = username!,
                //Password = password
            });

            try
            {
                using (client)
                {
                    await client.ConnectAsync();
                    Console.WriteLine("✓ Conectado exitosamente");
                    Console.WriteLine($"Dispositivo: {client.DeviceInformation?.Manufacturer} {client.DeviceInformation?.Model}");
                    Console.WriteLine();

                    // Obtener el perfil PTZ
                    using var ptzClient = new PTZClient(client);

                    var configurations = await ptzClient.GetConfigurationsAsync();
                    if (configurations == null || configurations.PTZConfiguration.Length == 0)
                    {
                        Console.WriteLine("✗ No se encontraron configuraciones PTZ. La cámara no soporta PTZ.");
                        return;
                    }

                    Console.WriteLine($"Configuraciones PTZ encontradas: {configurations.PTZConfiguration.Length}");

                    // Obtener el primer perfil de media
                    var mediaClient = new QuickNV.Onvif.Media.MediaClient(client);
                    var profiles = await mediaClient.GetProfilesAsync();

                    if (profiles == null || profiles.Profiles.Length == 0)
                    {
                        Console.WriteLine("✗ No se encontraron perfiles de media.");
                        return;
                    }

                    var profile = profiles.Profiles[0];
                    Console.WriteLine($"Usando perfil: {profile.Name} (Token: {profile.token})");
                    Console.WriteLine();

                    // Obtener las opciones de configuración PTZ
                    var ptzOptions = await ptzClient.GetConfigurationOptionsAsync(configurations.PTZConfiguration[0].token);

                    Console.WriteLine("=== Controles disponibles ===");
                    Console.WriteLine("W - Arriba (Tilt Up)");
                    Console.WriteLine("S - Abajo (Tilt Down)");
                    Console.WriteLine("A - Izquierda (Pan Left)");
                    Console.WriteLine("D - Derecha (Pan Right)");
                    //Console.WriteLine("+ - Zoom In");
                    //Console.WriteLine("- - Zoom Out");
                    Console.WriteLine("ESC - Salir");
                    Console.WriteLine();
                    Console.WriteLine("Presiona y mantén la tecla para mover. Suelta para detener.");
                    Console.WriteLine("================================");
                    Console.WriteLine();

                    bool running = true;
                    bool isMoving = false;

                    while (running)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);

                            if (key.Key == ConsoleKey.Escape)
                            {
                                if (isMoving)
                                {
                                    await StopMovement(ptzClient, profile.token);
                                }
                                running = false;
                                Console.WriteLine("Saliendo...");
                            }
                            else
                            {
                                switch (key.Key)
                                {
                                    case ConsoleKey.W:
                                        Console.WriteLine("↑ Moviendo hacia arriba...");
                                        await MoveUp(ptzClient, profile.token);
                                        isMoving = true;
                                        break;

                                    case ConsoleKey.S:
                                        Console.WriteLine("↓ Moviendo hacia abajo...");
                                        await MoveDown(ptzClient, profile.token);
                                        isMoving = true;
                                        break;

                                    case ConsoleKey.A:
                                        Console.WriteLine("← Moviendo hacia la izquierda...");
                                        await MoveLeft(ptzClient, profile.token);
                                        isMoving = true;
                                        break;

                                    case ConsoleKey.D:
                                        Console.WriteLine("→ Moviendo hacia la derecha...");
                                        await MoveRight(ptzClient, profile.token);
                                        isMoving = true;
                                        break;

                                    //case ConsoleKey.Add:
                                    //case ConsoleKey.OemPlus:
                                    //    Console.WriteLine("🔍+ Zoom In...");
                                    //    await ZoomIn(ptzClient, profile.token);
                                    //    isMoving = true;
                                    //    break;

                                    //case ConsoleKey.Subtract:
                                    //case ConsoleKey.OemMinus:
                                    //    Console.WriteLine("🔍- Zoom Out...");
                                    //    await ZoomOut(ptzClient, profile.token);
                                    //    isMoving = true;
                                    //    break;
                                }
                            }
                        }

                        // Pequeña pausa para no consumir CPU
                        await Task.Delay(50);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
                Console.WriteLine($"Detalles: {ex}");
            }
        }


        static async Task MoveUp(PTZClient ptzClient, string profileToken)
        {
            await ContinuousMove(ptzClient, profileToken, 0f, 0.5f, 0f);
        }

        static async Task MoveDown(PTZClient ptzClient, string profileToken)
        {
            await ContinuousMove(ptzClient, profileToken, 0f, -0.5f, 0f);
        }

        static async Task MoveLeft(PTZClient ptzClient, string profileToken)
        {
            await ContinuousMove(ptzClient, profileToken, -0.5f, 0f, 0f);
        }

        static async Task MoveRight(PTZClient ptzClient, string profileToken)
        {
            await ContinuousMove(ptzClient, profileToken, 0.5f, 0f, 0f);
        }

        static async Task ZoomIn(PTZClient ptzClient, string profileToken)
        {
            await ContinuousMove(ptzClient, profileToken, 0f, 0f, 0.5f);
        }

        static async Task ZoomOut(PTZClient ptzClient, string profileToken)
        {
            await ContinuousMove(ptzClient, profileToken, 0f, 0f, -0.5f);
        }

        static async Task ContinuousMove(PTZClient ptzClient, string profileToken, float panSpeed, float tiltSpeed, float zoomSpeed)
        {
            try
            {
                await ptzClient.ContinuousMoveAsync(
                    ProfileToken: profileToken,
                    Velocity: new PTZSpeed
                    {
                        PanTilt = new Vector2D
                        {
                            x = panSpeed,
                            y = tiltSpeed
                        },
                        Zoom = new Vector1D
                        {
                            x = zoomSpeed
                        }
                    },
                    Timeout: "PT1S" // 1 segundo de timeout
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en movimiento continuo: {ex.Message}");
            }
        }

        static async Task StopMovement(PTZClient ptzClient, string profileToken)
        {
            try
            {
                await ptzClient.StopAsync(
                    ProfileToken: profileToken,
                    PanTilt: true,
                    Zoom:true
                );

                Console.WriteLine("⏹ Movimiento detenido");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al detener: {ex.Message}");
            }
        }

    }
}
