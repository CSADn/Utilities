using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SubdivxRipper
{
    public class Helpers
    {
        public static string ApplyRegex(string input, string pattern, int groupIdx = 1, RegexOptions options = RegexOptions.None)
        {
            var r = Regex.Match(input, pattern, options);

            if (r.Groups.Count < (groupIdx + 1))
                return string.Empty;
            else
                return r.Groups[groupIdx].Value;
        }


        public static string ParseCode(string input)
        {
            var r = Regex.Match(input, @"^.+\/\/www.subdivx.com\/([a-zA-Z0-9]+)-.*.html");

            if (r.Groups.Count < 2)
                return string.Empty;

            return r.Groups[1].Value;
        }

        public static int ParseYear(string input)
        {
            var r = Regex.Match(input, @"\((1[8-9][0-9][0-9]|20[0-2][0-9])\)");

            if (!r.Success)
                r = Regex.Match(input, @"1[8-9][0-9][0-9]|20[0-2][0-9]");

            if (!r.Success)
                return -1;
            else
            {
                var año = 0;

                do
                {
                    var value = r.Value;

                    if (r.Groups.Count == 2)
                        value = r.Groups[1].Value;

                    if (input.StartsWith(value))
                        r = r.NextMatch();
                    else if (int.TryParse(value, out int parse))
                    {
                        if (parse > 1900 || parse < DateTime.Now.Year)
                            año = parse;

                        r = r.NextMatch();
                    }
                    else
                        r = r.NextMatch();
                }
                while (r.Success);

                if (año > 0)
                    return año;
                else
                    return -1;
            }
        }

        public static int ParseInteger(string input)
        {
            if (int.TryParse(input.Replace(",", string.Empty), out int value))
                return value;
            else
                return -1;
        }

        public static double ParseDouble(string input)
        {
            if (double.TryParse(input.Replace(".", ","), out double value))
                return value;
            else
                return -1.0;
        }

        public static DateTime? ParseDate(string input)
        {
            if (DateTime.TryParseExact(input.Trim(), "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime fecha))
                return fecha;
            else
                return null;
        }

        public static DateTime? ParseDateTime(string input)
        {
            if (DateTime.TryParseExact(input.Trim(), "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                return fecha;
            else
                return null;
        }

        public static (int, int) ParseSeasson(string input)
        {
            var r = Regex.Match(input, @"s(\d\d*)(.*e(\d\d*))?", RegexOptions.IgnoreCase);

            if (r.Groups.Count != 4)
                return (-1, -1);
            else
            {
                var s = -1;
                var e = -1;

                if (!string.IsNullOrWhiteSpace(r.Groups[1].Value))
                    int.TryParse(r.Groups[1].Value, out s);

                if (!string.IsNullOrWhiteSpace(r.Groups[3].Value))
                    int.TryParse(r.Groups[3].Value, out e);

                return (s, e);
            }
        }

        public static (int, HtmlNode) ParseId(HtmlNode body)
        {
            var data = body.QuerySelector("div#buscador_detalle_sub_datos");
            var a = data.QuerySelectorAll("a[rel=nofollow]");
            var zip = a.Last().Attributes["href"].Value;
            var id = ParseInteger(ApplyRegex(zip, @"id=(\d+)"));

            return (id, data);
        }

        public static string ParseAKA(string input)
        {
            if (input.ToLower().Contains("aka"))
                return ApplyRegex(input, " aka (.+)$", options: RegexOptions.IgnoreCase);
            else
                return null;
        }

        public static Subtitulo ParseItem(int id, HtmlNode head, HtmlNode body, HtmlNode data)
        {
            var head_a = head.QuerySelector("a.titulo_menu_izq");
            var title = head_a.InnerText.Replace("Subtitulo de ", string.Empty);
            var aka = ParseAKA(title);
            var year = ParseYear(title);
            var season = ParseSeasson(title);
            var href = head_a.Attributes["href"].Value;
            var code = ParseCode(href);

            var detail = body.QuerySelector("div#buscador_detalle_sub").InnerText;

            var downloads = ParseInteger(ApplyRegex(data.InnerText, @"Downloads: ([\d,]+) "));
            var cds = ParseInteger(ApplyRegex(data.InnerText, @"Cds: ([\d,]+) "));
            var comments = ParseInteger(ApplyRegex(data.InnerText, @"Comentarios: ([\d,]+) "));
            var format = ApplyRegex(data.InnerText, @"Formato: (\S+) ");

            var uploader = data.QuerySelector("a.link1");
            var nick = uploader.InnerText;
            var profile = uploader.Attributes["href"].Value;
            var flag = data.QuerySelector("img").Attributes["src"].Value;

            var a = data.QuerySelectorAll("a[rel=nofollow]");
            var zip = a.Last().Attributes["href"].Value;
            var uploaded = ParseDate(ApplyRegex(data.InnerText, @" el (\d{2}\/\d{2}\/\d{4})"));

            var type = (
                season.Item1 > 0 && season.Item2 > 0
                    ? (int)TipoSubtitulo.Serie
                    : (int)TipoSubtitulo.Pelicula
            );

            return new Subtitulo
            {
                Titulo = title,
                Aka = aka,
                Año = year,
                Temporada = season.Item1,
                Episodio = season.Item2,
                Codigo = code,
                Url = href,

                Detalle = detail,
                Downloads = downloads,
                Cds = cds,
                Comentarios = comments,
                Formato = format,
                SubidoPor = nick,
                SubidoFecha = uploaded,
                UrlPerfilUsuario = profile,
                UrlBandera = flag,
                UrlDescarga = zip,
                IdDescarga = id,
                Tipo = type
            };
        }

        public static Subtitulo ParseDetail(Subtitulo item, HtmlNode documentNode)
        {
            var data = documentNode.QuerySelector("div#detalle_datos");

            if (data == null)
                return item;

            item.UrlPoster = data.QuerySelector("img.detalle_foto").Attributes["src"].Value;

            var span = data.Element("span").InnerText;
            item.Calificacion = ParseDouble(ApplyRegex(span, @"Calificación: ([\d.]+)"));
            item.FrameRate = ParseDouble(ApplyRegex(span, @"Frame Rate: ([0-9\.]+|n\/a) fps", options: RegexOptions.IgnoreCase));

            var div = data.QuerySelector("div#detalle_datos_derecha")?.InnerText;

            if (div != null)
            {
                var uploaded = ParseDateTime(ApplyRegex(div, @"- el (\d{2}\/\d{2}\/\d{4} \d{2}:\d{2}:\d{2} (am|pm)) -"));

                if (uploaded != null)
                    item.SubidoFecha = uploaded;
            }

            var comments = documentNode.QuerySelectorAll("div[id=detalle_comentarios]");

            foreach (var c in comments)
            {
                var text = c.QuerySelector("div#detalle_reng_coment1").InnerText;
                var a = c.QuerySelector("a.detalle_link");
                var nick = a?.InnerText ?? string.Empty;
                var profile = a?.Attributes["href"].Value ?? string.Empty;
                var flag = c.QuerySelector("img")?.Attributes["src"].Value;

                if (item.ListaComentarios == null)
                    item.ListaComentarios = new List<Comentario>();

                item.ListaComentarios.Add(new Comentario
                {
                    Detalle = text,
                    Usuario = nick,
                    UrlPerfil = profile,
                    UrlBandera = flag
                });
            }

            return item;
        }

        public static string ParseFilePath(Subtitulo item)
        {
            if (item == null)
                throw new Exception();

            var title = item.Titulo.ToUpper().PadRight(3);

            if (title.StartsWith("THE "))
                title = Regex.Replace(title, "^THE ", string.Empty);

            var first = CharMapping(title[0]);
            var second = CharMapping(title[1]);
            var third = CharMapping(title[2]);

            var type = (
                item.Tipo == 1
                    ? "Peliculas"
                    : "Series"
            );

            return Path.Combine(type, first, second, third);
        }


        private static string CharMapping(char input)
        {
            var map = new Dictionary<char, char>
            {
                { 'À', 'A' },
                { 'Á', 'A' },
                { 'Â', 'A' },
                { 'Ã', 'A' },
                { 'Ä', 'A' },
                { 'Å', 'A' },
                { 'Ǻ', 'A' },
                { 'Ā', 'A' },
                { 'Ă', 'A' },
                { 'Ą', 'A' },
                { 'Ǎ', 'A' },
                { 'Α', 'A' },
                { 'Ά', 'A' },
                { 'Ả', 'A' },
                { 'Ạ', 'A' },
                { 'Ầ', 'A' },
                { 'Ẫ', 'A' },
                { 'Ẩ', 'A' },
                { 'Ậ', 'A' },
                { 'Ằ', 'A' },
                { 'Ắ', 'A' },
                { 'Ẵ', 'A' },
                { 'Ẳ', 'A' },
                { 'Ặ', 'A' },
                { 'А', 'A' },
                { 'Æ', 'A' },
                { 'Б', 'B' },
                { 'Ç', 'C' },
                { 'Ć', 'C' },
                { 'Ĉ', 'C' },
                { 'Ċ', 'C' },
                { 'Č', 'C' },
                { 'Ч', 'C' },
                { 'Д', 'D' },
                { 'Ð', 'D' },
                { 'Ď', 'D' },
                { 'Đ', 'D' },
                { 'Δ', 'D' },
                { 'È', 'E' },
                { 'É', 'E' },
                { 'Ê', 'E' },
                { 'Ë', 'E' },
                { 'Ē', 'E' },
                { 'Ĕ', 'E' },
                { 'Ė', 'E' },
                { 'Ę', 'E' },
                { 'Ě', 'E' },
                { 'Ε', 'E' },
                { 'Έ', 'E' },
                { 'Ẽ', 'E' },
                { 'Ẻ', 'E' },
                { 'Ẹ', 'E' },
                { 'Ề', 'E' },
                { 'Ế', 'E' },
                { 'Ễ', 'E' },
                { 'Ể', 'E' },
                { 'Ệ', 'E' },
                { 'Е', 'E' },
                { 'Э', 'E' },
                { 'Ф', 'F' },
                { 'Ƒ', 'F' },
                { 'Ĝ', 'G' },
                { 'Ğ', 'G' },
                { 'Ġ', 'G' },
                { 'Ģ', 'G' },
                { 'Γ', 'G' },
                { 'Г', 'G' },
                { 'Ґ', 'G' },
                { 'Ĥ', 'H' },
                { 'Ħ', 'H' },
                { 'Ì', 'I' },
                { 'Í', 'I' },
                { 'Î', 'I' },
                { 'Ï', 'I' },
                { 'Ĩ', 'I' },
                { 'Ī', 'I' },
                { 'Ĭ', 'I' },
                { 'Ǐ', 'I' },
                { 'Į', 'I' },
                { 'I', 'I' },
                { 'Η', 'I' },
                { 'Ή', 'I' },
                { 'Ί', 'I' },
                { 'Ι', 'I' },
                { 'Ϊ', 'I' },
                { 'Ỉ', 'I' },
                { 'Ị', 'I' },
                { 'И', 'I' },
                { 'Ы', 'I' },
                { 'Ї', 'I' },
                { 'Ĳ', 'I' },
                { 'Ĵ', 'J' },
                { 'Ķ', 'K' },
                { 'Κ', 'K' },
                { 'К', 'K' },
                { 'Ξ', 'K' },
                { 'Ĺ', 'L' },
                { 'Ļ', 'L' },
                { 'Ľ', 'L' },
                { 'Ŀ', 'L' },
                { 'Ł', 'L' },
                { 'Λ', 'L' },
                { 'Л', 'L' },
                { 'Ñ', 'N' },
                { 'Ń', 'N' },
                { 'Ņ', 'N' },
                { 'Ň', 'N' },
                { 'ŉ', 'N' },
                { 'Ν', 'N' },
                { 'Н', 'N' },
                { 'Ö', 'O' },
                { 'Ò', 'O' },
                { 'Ó', 'O' },
                { 'Ô', 'O' },
                { 'Õ', 'O' },
                { 'Ō', 'O' },
                { 'Ŏ', 'O' },
                { 'Ǒ', 'O' },
                { 'Ő', 'O' },
                { 'Ơ', 'O' },
                { 'Ø', 'O' },
                { 'Ǿ', 'O' },
                { 'º', 'O' },
                { 'Ο', 'O' },
                { 'Ό', 'O' },
                { 'Ω', 'O' },
                { 'Ώ', 'O' },
                { 'Ỏ', 'O' },
                { 'Ọ', 'O' },
                { 'Ồ', 'O' },
                { 'Ố', 'O' },
                { 'Ỗ', 'O' },
                { 'Ổ', 'O' },
                { 'Ộ', 'O' },
                { 'Ờ', 'O' },
                { 'Ớ', 'O' },
                { 'Ỡ', 'O' },
                { 'Ở', 'O' },
                { 'Ợ', 'O' },
                { 'О', 'O' },
                { 'Œ', 'O' },
                { 'П', 'P' },
                { 'Ψ', 'P' },
                { 'Ŕ', 'R' },
                { 'Ŗ', 'R' },
                { 'Ř', 'R' },
                { 'Ρ', 'R' },
                { 'Р', 'R' },
                { 'Ś', 'S' },
                { 'Ŝ', 'S' },
                { 'Ş', 'S' },
                { 'Ș', 'S' },
                { 'Š', 'S' },
                { 'S', 'S' },
                { 'Σ', 'S' },
                { 'С', 'S' },
                { 'Ш', 'S' },
                { 'Щ', 'S' },
                { 'ß', 'S' },
                { 'Ț', 'T' },
                { 'Ţ', 'T' },
                { 'Ť', 'T' },
                { 'Ŧ', 'T' },
                { 'Τ', 'T' },
                { 'Ц', 'T' },
                { 'Ü', 'U' },
                { 'Ù', 'U' },
                { 'Ú', 'U' },
                { 'Û', 'U' },
                { 'Ũ', 'U' },
                { 'Ū', 'U' },
                { 'Ŭ', 'U' },
                { 'Ů', 'U' },
                { 'Ű', 'U' },
                { 'Ų', 'U' },
                { 'Ư', 'U' },
                { 'Ǔ', 'U' },
                { 'Ǖ', 'U' },
                { 'Ǘ', 'U' },
                { 'Ǚ', 'U' },
                { 'Ǜ', 'U' },
                { 'Ủ', 'U' },
                { 'Ụ', 'U' },
                { 'Ừ', 'U' },
                { 'Ứ', 'U' },
                { 'Ữ', 'U' },
                { 'Ử', 'U' },
                { 'Ự', 'U' },
                { 'У', 'U' },
                { 'В', 'V' },
                { 'Ŵ', 'W' },
                { 'Ý', 'Y' },
                { 'Ÿ', 'Y' },
                { 'Ŷ', 'Y' },
                { 'Υ', 'Y' },
                { 'Ύ', 'Y' },
                { 'Ỳ', 'Y' },
                { 'Ỹ', 'Y' },
                { 'Ỷ', 'Y' },
                { 'Ỵ', 'Y' },
                { 'Й', 'Y' },
                { 'Ё', 'Y' },
                { 'Є', 'Y' },
                { 'Ю', 'Y' },
                { 'Я', 'Y' },
                { 'Ź', 'Z' },
                { 'Ż', 'Z' },
                { 'Ž', 'Z' },
                { 'Ζ', 'Z' },
                { 'З', 'Z' },
                { 'Ж', 'Z' }
            };

            var c = input.ToString().ToUpper()[0];

            if (map.ContainsKey(c))
                c = map[c];

            if (Regex.IsMatch(c.ToString(), "[0-9A-Z]", RegexOptions.CultureInvariant))
                return c.ToString();
            else if (c == ' ')
                return "_";
            else
                return "$";
        }
    }
}
