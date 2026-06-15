using System.Collections.Generic;
using System.Collections.Immutable;

namespace CaptchaGen
{
    internal static class Imagenes
    {
        internal static readonly ImmutableDictionary<string, string> Images = new Dictionary<string, string>
        {
            { "Avión", "airplane.png" },
            { "Globos", "balloons.png" },
            { "Camara", "camera.png" },
            { "Auto", "car.png" },
            { "Gato", "cat.png" },
            { "Silla", "chair.png" },
            { "Clip", "clip.png" },
            { "Reloj", "clock.png" },
            { "Nube", "cloud.png" },
            { "Computadora", "computer.png" },
            { "Sobre", "envelope.png" },
            { "Ojo", "eye.png" },
            { "Bandera", "flag.png" },
            { "Carpeta", "folder.png" },
            { "Pie", "foot.png" },
            { "Grafico", "graph.png" },
            { "Casa", "house.png" },
            { "Llave", "key.png" },
            { "Hoja", "leaf.png" },
            { "Bombillo", "light-bulb.png" },
            { "Candado", "lock.png" },
            { "Lupa", "magnifying-glass.png" },
            { "Hombre", "man.png" },
            { "Clave de Sol", "music-note.png" },
            { "Pantalon", "pants.png" },
            { "Lápiz", "pencil.png" },
            { "Impresora", "printer.png" },
            { "Robot", "robot.png" },
            { "Tijera", "scissors.png" },
            { "Anteojos de Sol", "sunglasses.png" },
            { "Etiqueta", "tag.png" },
            { "Arbol", "tree.png" },
            { "Camión", "truck.png" },
            { "Remera", "t-shirt.png" },
            { "Paraguas", "umbrella.png" },
            { "Mujer", "woman.png" },
            { "Mundo", "world.png" }
        }.ToImmutableDictionary();
    }
}
