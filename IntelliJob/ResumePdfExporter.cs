using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliJob
{
    public static class ResumePdfExporter
    {
        private const double PageWidth = 595.0;
        private const double PageHeight = 842.0;
        private const double LeftMargin = 42.0;
        private const double RightMargin = 42.0;
        private const double TopMargin = 48.0;
        private const double BottomMargin = 48.0;

        public static byte[] Build(ResumeEnhancementReportRecord report)
        {
            if (report == null)
                throw new ArgumentNullException("report");

            var result = report.Result ?? new ResumeEnhancementResult();
            var lines = new List<PdfLine>();

            AddHeading(lines, "Enhanced Resume Report", 20, true, 0, 14);
            AddText(lines, string.Format(CultureInfo.InvariantCulture, "{0} at {1}", SafeText(report.JobTitle, "Untitled Role"), SafeText(report.CompanyName, "Unknown Company")), 13, true, 0, 12);
            AddText(lines, string.Format(CultureInfo.InvariantCulture, "Generated: {0}", ToLocalString(report.GeneratedAt)), 10, false, 0, 5);
            AddText(lines, string.Format(CultureInfo.InvariantCulture, "Resume source: {0}", SafeText(report.ResumeSource, "profile")), 10, false, 0, 4);
            AddText(lines, string.Format(CultureInfo.InvariantCulture, "Scores: Overall {0}% | ATS {1}% | Semantic {2}% | Keywords {3}%", result.OverallScore, result.AtsScore, result.SemanticScore, result.KeywordScore), 10, false, 0, 12);

            AddSection(lines, "Resume Summary");
            AddParagraph(lines, result.ResumeSummary, 11);

            AddSection(lines, "Strengths");
            AddBulletList(lines, result.Strengths, 11);

            AddSection(lines, "Gaps To Improve");
            AddBulletList(lines, result.Gaps, 11);

            AddSection(lines, "Priority Keywords");
            AddBulletList(lines, result.PriorityKeywords, 11);

            AddSection(lines, "Final Assessment");
            AddParagraph(lines, result.FinalAssessment, 11);

            AddSection(lines, "Enhanced Resume Preview");
            AddParagraph(lines, SafeText(report.UpdatedResumeText, report.OriginalResumeText), 10);

            AddSection(lines, "Assessment Context");
            AddParagraph(lines, BuildAssessmentContext(report), 10);

            return RenderPdf(lines);
        }

        private static void AddHeading(ICollection<PdfLine> lines, string text, int fontSize, bool bold, double indent, double spacingAfter)
        {
            lines.Add(new PdfLine(text, fontSize, bold, indent, 0, spacingAfter));
        }

        private static void AddSection(ICollection<PdfLine> lines, string title)
        {
            lines.Add(new PdfLine(title, 13, true, 0, 8, 4));
            lines.Add(new PdfLine(string.Empty, 1, false, 0, 0, 0));
        }

        private static void AddText(ICollection<PdfLine> lines, string text, int fontSize, bool bold, double indent, double spacingAfter)
        {
            lines.Add(new PdfLine(SanitizeText(text), fontSize, bold, indent, 0, spacingAfter));
        }

        private static void AddParagraph(ICollection<PdfLine> lines, string text, int fontSize)
        {
            foreach (string paragraph in SplitParagraphs(text))
            {
                if (string.IsNullOrWhiteSpace(paragraph))
                {
                    lines.Add(new PdfLine(string.Empty, fontSize, false, 0, 0, 0));
                    continue;
                }

                foreach (string wrapped in WrapText(SanitizeText(paragraph), fontSize, 0))
                    lines.Add(new PdfLine(wrapped, fontSize, false, 0, 0, 0));

                lines.Add(new PdfLine(string.Empty, fontSize, false, 0, 0, 0));
            }
        }

        private static void AddBulletList(ICollection<PdfLine> lines, IEnumerable<string> items, int fontSize)
        {
            var cleaned = (items ?? Enumerable.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (cleaned.Count == 0)
            {
                AddText(lines, "No items were available for this section.", fontSize, false, 0, 6);
                return;
            }

            foreach (string item in cleaned)
            {
                string text = "- " + SanitizeText(item.Trim());
                foreach (string wrapped in WrapText(text, fontSize, 10))
                    lines.Add(new PdfLine(wrapped, fontSize, false, 10, 0, 0));
                lines.Add(new PdfLine(string.Empty, fontSize, false, 0, 0, 0));
            }
        }

        private static IEnumerable<string> SplitParagraphs(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<string>();

            return text.Replace("\r\n", "\n").Replace("\r", "\n").Split(new[] { '\n' }, StringSplitOptions.None);
        }

        private static IEnumerable<string> WrapText(string text, int fontSize, double indent)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new[] { string.Empty };

            int maxChars = Math.Max(28, (int)Math.Floor((PageWidth - LeftMargin - RightMargin - indent) / Math.Max(4.5, fontSize * 0.52)));
            var lines = new List<string>();

            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
                return new[] { string.Empty };

            var current = new StringBuilder();
            foreach (string word in words)
            {
                if (current.Length == 0)
                {
                    current.Append(word);
                    continue;
                }

                if (current.Length + 1 + word.Length <= maxChars)
                {
                    current.Append(' ').Append(word);
                    continue;
                }

                lines.Add(current.ToString());
                current.Clear();
                current.Append(word);
            }

            if (current.Length > 0)
                lines.Add(current.ToString());

            return lines;
        }

        private static byte[] RenderPdf(IReadOnlyList<PdfLine> lines)
        {
            var pages = new List<string>();
            var current = new StringBuilder();
            double y = PageHeight - TopMargin;
            bool hasContent = false;

            Action flushPage = () =>
            {
                if (hasContent)
                    pages.Add(current.ToString());
                current.Clear();
                y = PageHeight - TopMargin;
                hasContent = false;
            };

            foreach (PdfLine line in lines)
            {
                if (line == null)
                    continue;

                if (line.SpacingBefore > 0)
                    y -= line.SpacingBefore;

                if (string.IsNullOrWhiteSpace(line.Text))
                {
                    y -= Math.Max(8, line.FontSize * 0.55);
                    if (y < BottomMargin)
                        flushPage();
                    continue;
                }

                foreach (string wrapped in WrapText(line.Text, line.FontSize, line.Indent))
                {
                    double lineHeight = Math.Max(12, line.FontSize * 1.4);
                    if (y < BottomMargin + lineHeight)
                        flushPage();

                    string fontName = line.Bold ? "/F2" : "/F1";
                    current.AppendFormat(CultureInfo.InvariantCulture, "BT {0} {1} Tf 1 0 0 1 {2:0.##} {3:0.##} Tm ({4}) Tj ET\n", fontName, line.FontSize, LeftMargin + line.Indent, y, EscapePdfText(wrapped));
                    hasContent = true;
                    y -= lineHeight;
                }

                if (line.SpacingAfter > 0)
                    y -= line.SpacingAfter;
            }

            if (hasContent)
                pages.Add(current.ToString());

            if (pages.Count == 0)
                pages.Add(string.Empty);

            return BuildPdfFile(pages);
        }

        private static byte[] BuildPdfFile(IReadOnlyList<string> pageContents)
        {
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                BuildPagesObject(pageContents.Count),
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>"
            };

            var pageObjectNumbers = new List<int>();
            for (int i = 0; i < pageContents.Count; i++)
            {
                int pageObjectNumber = objects.Count + 1;
                int contentObjectNumber = objects.Count + 2;
                pageObjectNumbers.Add(pageObjectNumber);
                objects.Add(string.Format(CultureInfo.InvariantCulture,
                    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {0} {1}] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {2} 0 R >>",
                    PageWidth, PageHeight, contentObjectNumber));
                objects.Add(BuildContentObject(pageContents[i]));
            }

            objects[1] = BuildPagesObject(pageContents.Count, pageObjectNumbers);

            var output = new MemoryStream();
            var offsets = new List<long> { 0 };
            WriteAscii(output, "%PDF-1.4\n");

            for (int i = 0; i < objects.Count; i++)
            {
                offsets.Add(output.Position);
                WriteAscii(output, string.Format(CultureInfo.InvariantCulture, "{0} 0 obj\n{1}\nendobj\n", i + 1, objects[i]));
            }

            long xrefPosition = output.Position;
            WriteAscii(output, string.Format(CultureInfo.InvariantCulture, "xref\n0 {0}\n", objects.Count + 1));
            WriteAscii(output, "0000000000 65535 f \n");
            for (int i = 1; i < offsets.Count; i++)
                WriteAscii(output, string.Format(CultureInfo.InvariantCulture, "{0:0000000000} 00000 n \n", offsets[i]));

            WriteAscii(output, string.Format(CultureInfo.InvariantCulture, "trailer\n<< /Size {0} /Root 1 0 R >>\nstartxref\n{1}\n%%EOF", objects.Count + 1, xrefPosition));
            return output.ToArray();
        }

        private static string BuildPagesObject(int count, IList<int> pageObjectNumbers)
        {
            var builder = new StringBuilder();
            builder.Append("<< /Type /Pages /Kids [");
            for (int i = 0; i < pageObjectNumbers.Count; i++)
            {
                if (i > 0)
                    builder.Append(' ');
                builder.Append(pageObjectNumbers[i]).Append(" 0 R");
            }
            builder.Append("] /Count ").Append(count).Append(" >>");
            return builder.ToString();
        }

        private static string BuildPagesObject(int count)
        {
            return string.Format(CultureInfo.InvariantCulture, "<< /Type /Pages /Kids [] /Count {0} >>", count);
        }

        private static string BuildContentObject(string content)
        {
            content = content ?? string.Empty;
            return string.Format(CultureInfo.InvariantCulture, "<< /Length {0} >>\nstream\n{1}endstream", Encoding.ASCII.GetByteCount(content), content);
        }

        private static void WriteAscii(Stream stream, string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string SafeText(string value, string fallback)
        {
            string text = string.IsNullOrWhiteSpace(value) ? fallback : value;
            return SanitizeText(text);
        }

        private static string BuildAssessmentContext(ResumeEnhancementReportRecord report)
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Role: {0}", SafeText(report.JobTitle, "Untitled Role")));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Company: {0}", SafeText(report.CompanyName, "Unknown Company")));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Resume source: {0}", SafeText(report.ResumeSource, "profile")));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Keywords: {0}", SafeText(report.KeywordHints, "None")));
            if (!string.IsNullOrWhiteSpace(report.InterviewFeedback))
            {
                builder.AppendLine();
                builder.AppendLine(SafeText(report.InterviewFeedback, string.Empty));
            }
            return builder.ToString().Trim();
        }

        private static string EscapePdfText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }

        private static string SanitizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalized = text
                .Replace("\u00A0", " ")
                .Replace("\u2019", "'")
                .Replace("\u2018", "'")
                .Replace("\u201C", '"'.ToString())
                .Replace("\u201D", '"'.ToString())
                .Replace("\u2013", "-")
                .Replace("\u2014", "-")
                .Replace("•", "-")
                .Replace("→", "->");

            var builder = new StringBuilder(normalized.Length);
            foreach (char character in normalized)
            {
                if (character == '\r' || character == '\n' || character == '\t')
                {
                    builder.Append(' ');
                }
                else if (character >= 32 && character <= 126)
                {
                    builder.Append(character);
                }
                else
                {
                    builder.Append('?');
                }
            }

            return builder.ToString().Replace("  ", " ").Trim();
        }

        private static string ToLocalString(DateTime value)
        {
            if (value == DateTime.MinValue)
                return "Unknown";

            return value.Kind == DateTimeKind.Utc
                ? value.ToLocalTime().ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture)
                : value.ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture);
        }

        private sealed class PdfLine
        {
            public PdfLine(string text, int fontSize, bool bold, double indent, double spacingBefore, double spacingAfter)
            {
                Text = text ?? string.Empty;
                FontSize = fontSize;
                Bold = bold;
                Indent = indent;
                SpacingBefore = spacingBefore;
                SpacingAfter = spacingAfter;
            }

            public string Text { get; }

            public int FontSize { get; }

            public bool Bold { get; }

            public double Indent { get; }

            public double SpacingBefore { get; }

            public double SpacingAfter { get; }
        }
    }
}