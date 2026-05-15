using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace IntelliJob
{
    public static class ResumeTextExtractor
    {
        public static int EstimatePageCount(string resumePath, string extractedText = null)
        {
            string fullPath = ResolvePath(resumePath);
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                return 0;

            string extension = Path.GetExtension(fullPath).ToLowerInvariant();
            try
            {
                if (extension == ".pdf")
                {
                    string raw = Encoding.GetEncoding("ISO-8859-1").GetString(File.ReadAllBytes(fullPath));
                    int count = Regex.Matches(raw, @"/Type\s*/Page\b", RegexOptions.IgnoreCase).Count;
                    return Math.Max(1, count);
                }

                if (extension == ".docx")
                {
                    using (ZipArchive archive = ZipFile.OpenRead(fullPath))
                    {
                        ZipArchiveEntry entry = archive.GetEntry("word/document.xml");
                        if (entry == null)
                            return 1;

                        using (Stream stream = entry.Open())
                        {
                            XDocument document = XDocument.Load(stream);
                            XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
                            int breaks = document.Descendants(w + "br")
                                .Count(n => string.Equals((string)n.Attribute(w + "type"), "page", StringComparison.OrdinalIgnoreCase));
                            breaks += document.Descendants(w + "lastRenderedPageBreak").Count();
                            return Math.Max(1, breaks + 1);
                        }
                    }
                }
            }
            catch
            {
            }

            string text = extractedText ?? ExtractText(fullPath);
            int words = Regex.Matches(text ?? string.Empty, @"\b[\w@.+-]+\b").Count;
            int estimated = (int)Math.Ceiling(words / 750.0);
            return Math.Max(1, estimated);
        }

        public static string ExtractText(string resumePath)
        {
            string fullPath = ResolvePath(resumePath);
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                return string.Empty;

            string extension = Path.GetExtension(fullPath).ToLowerInvariant();
            if (extension == ".docx")
                return NormalizeText(ExtractDocxText(fullPath));

            if (extension == ".pdf")
                return NormalizeText(ExtractPdfText(fullPath));

            return NormalizeText(File.ReadAllText(fullPath));
        }

        private static string ResolvePath(string resumePath)
        {
            if (string.IsNullOrWhiteSpace(resumePath))
                return string.Empty;

            string trimmed = resumePath.Trim();
            if (Path.IsPathRooted(trimmed))
                return trimmed;

            string virtualPath = trimmed.Replace('\\', '/').TrimStart('/');
            if (virtualPath.StartsWith("~/", StringComparison.Ordinal))
                virtualPath = virtualPath.Substring(2);

            if (HttpContext.Current != null)
                return HttpContext.Current.Server.MapPath("~/" + virtualPath);

            return Path.GetFullPath(virtualPath);
        }

        private static string ExtractDocxText(string fullPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(fullPath))
            {
                ZipArchiveEntry entry = archive.GetEntry("word/document.xml");
                if (entry == null)
                    return string.Empty;

                using (Stream stream = entry.Open())
                {
                    XDocument document = XDocument.Load(stream);
                    XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
                    var paragraphs = document.Descendants(w + "p")
                        .Select(p => string.Concat(p.Descendants(w + "t").Select(t => t.Value)))
                        .Where(p => !string.IsNullOrWhiteSpace(p));
                    return string.Join(Environment.NewLine, paragraphs);
                }
            }
        }

        private static string ExtractPdfText(string fullPath)
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            string raw = Encoding.GetEncoding("ISO-8859-1").GetString(bytes);
            var builder = new StringBuilder();

            int streamIndex = 0;
            while (streamIndex >= 0 && streamIndex < raw.Length)
            {
                streamIndex = raw.IndexOf("stream", streamIndex, StringComparison.OrdinalIgnoreCase);
                if (streamIndex < 0)
                    break;

                int streamDataStart = streamIndex + 6;
                if (streamDataStart < raw.Length && raw[streamDataStart] == '\r')
                    streamDataStart++;
                if (streamDataStart < raw.Length && raw[streamDataStart] == '\n')
                    streamDataStart++;
                else if (streamDataStart < raw.Length && raw[streamDataStart - 1] == '\n')
                    streamDataStart = streamIndex + 7;

                int endIndex = raw.IndexOf("endstream", streamDataStart, StringComparison.OrdinalIgnoreCase);
                if (endIndex < 0)
                    break;

                string header = raw.Substring(Math.Max(0, streamIndex - 300), streamIndex - Math.Max(0, streamIndex - 300));
                string chunk = raw.Substring(streamDataStart, Math.Max(0, endIndex - streamDataStart));
                string streamText = TryDecompressPdfStream(bytes, streamDataStart, endIndex, header);
                if (string.IsNullOrWhiteSpace(streamText))
                    streamText = chunk;

                AppendPdfText(builder, streamText);
                streamIndex = endIndex + 9;
            }

            if (builder.Length == 0)
                AppendPdfText(builder, raw);

            return builder.ToString();
        }

        private static string TryDecompressPdfStream(byte[] bytes, int startIndex, int endIndex, string header)
        {
            if (header.IndexOf("FlateDecode", StringComparison.OrdinalIgnoreCase) < 0)
                return string.Empty;

            int length = Math.Max(0, endIndex - startIndex);
            if (length == 0 || startIndex >= bytes.Length)
                return string.Empty;

            int safeLength = Math.Min(length, bytes.Length - startIndex);
            byte[] compressed = new byte[safeLength];
            Array.Copy(bytes, startIndex, compressed, 0, safeLength);

            string text = TryInflate(compressed);
            if (!string.IsNullOrWhiteSpace(text))
                return text;

            if (compressed.Length > 6)
            {
                byte[] trimmed = new byte[compressed.Length - 6];
                Array.Copy(compressed, 2, trimmed, 0, trimmed.Length);
                text = TryInflate(trimmed);
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }

            return string.Empty;
        }

        private static string TryInflate(byte[] data)
        {
            try
            {
                using (MemoryStream input = new MemoryStream(data))
                using (MemoryStream output = new MemoryStream())
                using (DeflateStream deflate = new DeflateStream(input, CompressionMode.Decompress))
                {
                    deflate.CopyTo(output);
                    return Encoding.UTF8.GetString(output.ToArray());
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void AppendPdfText(StringBuilder builder, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            MatchCollection textMatches = Regex.Matches(content, @"\((?<text>(?:\\.|[^\\)])*)\)\s*Tj");
            foreach (Match match in textMatches)
            {
                string decoded = DecodePdfLiteral(match.Groups["text"].Value);
                if (!string.IsNullOrWhiteSpace(decoded))
                    builder.AppendLine(decoded);
            }

            MatchCollection arrayMatches = Regex.Matches(content, @"\[(?<array>.*?)\]\s*TJ", RegexOptions.Singleline);
            foreach (Match match in arrayMatches)
            {
                MatchCollection fragments = Regex.Matches(match.Groups["array"].Value, @"\((?<text>(?:\\.|[^\\)])*)\)");
                foreach (Match fragment in fragments)
                {
                    string decoded = DecodePdfLiteral(fragment.Groups["text"].Value);
                    if (!string.IsNullOrWhiteSpace(decoded))
                        builder.Append(decoded);
                }

                builder.AppendLine();
            }
        }

        private static string DecodePdfLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                if (current != '\\' || i == value.Length - 1)
                {
                    builder.Append(current);
                    continue;
                }

                char next = value[++i];
                switch (next)
                {
                    case 'n': builder.Append('\n'); break;
                    case 'r': builder.Append('\r'); break;
                    case 't': builder.Append('\t'); break;
                    case 'b': builder.Append('\b'); break;
                    case 'f': builder.Append('\f'); break;
                    case '(':
                    case ')':
                    case '\\':
                        builder.Append(next);
                        break;
                    default:
                        builder.Append(next);
                        break;
                }
            }

            return builder.ToString();
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            normalized = Regex.Replace(normalized, "[ \t]{2,}", " ");
            normalized = Regex.Replace(normalized, "\n{3,}", "\n\n");
            return normalized.Trim();
        }
    }
}
