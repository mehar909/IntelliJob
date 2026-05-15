using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliJob
{
    public static class ResumePdfExporter
    {
        private const double PageWidth   = 595.0;
        private const double PageHeight  = 842.0;
        private const double LeftMargin  = 72.0;   // 1 inch
        private const double RightMargin = 72.0;
        private const double TopMargin   = 72.0;
        private const double BottomMargin = 54.0;

        /// <summary>
        /// Builds a one-page professional resume PDF matching the MY_RESUME template.
        /// Times-Roman / Times-Bold, centered header with real PNG icons, clickable links.
        /// </summary>
        public static byte[] BuildResume(ResumeProfileDocument doc, string iconsFolder)
        {
            if (doc == null) throw new ArgumentNullException("doc");
            iconsFolder = iconsFolder ?? string.Empty;

            const double CW   = PageWidth - LeftMargin - RightMargin;
            const int    NS   = 18;    // name font size
            const int    BS   = 10;    // body font size
            const int    HS   = 12;    // heading font size
            const double LH   = 13.5; // body line height (pts)
            const double ICON = 8.5;   // icon size (0.3 cm ≈ 8.5 pt)
            const double IGAP = 3.0;   // gap after icon

            // ── Decode PNG icons → raw RGB + Alpha via System.Drawing ──
            var imgRgb   = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            var imgAlpha = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            var imgDims  = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);

            foreach (string fn in new[] {
                "email_icon.png", "phone_icon.png", "location_icon.png",
                "linkedin_icon.png", "github_icon.png", "portfolio_icon.png" })
            {
                string fp = Path.Combine(iconsFolder, fn);
                if (!File.Exists(fp)) continue;
                try
                {
                    using (var bmp = new Bitmap(fp))
                    {
                        int w = bmp.Width, h = bmp.Height;
                        var rgb = new byte[w * h * 3];
                        var alp = new byte[w * h];
                        int ri = 0, ai = 0;
                        // PDF image coordinate origin is bottom-left, so flip rows
                        for (int row = h - 1; row >= 0; row--)
                            for (int col = 0; col < w; col++)
                            {
                                Color c = bmp.GetPixel(col, row);
                                rgb[ri++] = c.R; rgb[ri++] = c.G; rgb[ri++] = c.B;
                                alp[ai++] = c.A;
                            }
                        imgRgb[fn]   = rgb;
                        imgAlpha[fn] = alp;
                        imgDims[fn]  = new[] { w, h };
                    }
                }
                catch { /* skip corrupt / missing icon */ }
            }

            var sb     = new StringBuilder();
            var annots = new List<PdfLinkAnnotation>();
            double y   = PageHeight - TopMargin;

            // Estimate text width: Times-Roman avg char ≈ 0.5 * fontSize
            Func<string, int, double> tw = (t, s) => (t ?? "").Replace("*", "").Replace("_", "").Length * s * 0.5;

            // Append a PDF text-show operator with inline bold/italic parsing
            Action<string, double, double, bool, int> wt = (txt, x, ty, forceBold, sz) =>
            {
                var res = new StringBuilder();
                bool isBold = forceBold, isItalic = false;
                
                Func<string> currentFont = () => {
                    if (isBold && isItalic) return "F4";
                    if (isBold) return "F2";
                    if (isItalic) return "F3";
                    return "F1";
                };
                
                res.AppendFormat(CultureInfo.InvariantCulture, "/{0} {1} Tf (", currentFont(), sz);
                for (int i = 0; i < txt.Length; i++) {
                    char c = txt[i];
                    if (c == '*' || c == '_') {
                        res.Append(") Tj ");
                        if (c == '*') isBold = !isBold;
                        if (c == '_') isItalic = !isItalic;
                        res.AppendFormat(CultureInfo.InvariantCulture, "/{0} {1} Tf (", currentFont(), sz);
                    } else {
                        if (c == '(' || c == ')' || c == '\\') res.Append('\\');
                        res.Append(c);
                    }
                }
                res.Append(") Tj");
                
                sb.AppendFormat(CultureInfo.InvariantCulture, "BT 1 0 0 1 {0:0.##} {1:0.##} Tm {2} ET\n", x, ty, res.ToString());
            };

            // Draw a full-width horizontal rule at y-coordinate ry
            Action<double> hRule = ry =>
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "0.5 w 0 0 0 RG {0:0.##} {1:0.##} m {2:0.##} {1:0.##} l S\n",
                    LeftMargin, ry, PageWidth - RightMargin);

            // ── NAME (centered, 18 pt bold) ──
            string nm = SanitizeText(doc.FullName ?? "");
            if (!string.IsNullOrWhiteSpace(nm))
            {
                double nx = LeftMargin + (CW - tw(nm, NS)) / 2.0;
                wt(nm, nx, y, true, NS);
                y -= NS * 1.5;
            }

            // ── CONTACT LINE (centered, icons + text, separated by " | ") ──
            var cItems = new List<string[]>(); // [icon, display, url]

            if (!string.IsNullOrWhiteSpace(doc.Email))
                cItems.Add(new[] { "email_icon.png",
                    SanitizeText(doc.Email.Trim()), "mailto:" + doc.Email.Trim() });

            if (!string.IsNullOrWhiteSpace(doc.Mobile))
                cItems.Add(new[] { "phone_icon.png",
                    SanitizeText(doc.Mobile.Trim()), "" });

            if (!string.IsNullOrWhiteSpace(doc.Address))
                cItems.Add(new[] { "location_icon.png",
                    SanitizeText(doc.Address.Trim()), "" });

            if (!string.IsNullOrWhiteSpace(doc.LinkedInUrl))
                cItems.Add(new[] { "linkedin_icon.png",
                    GetDisplayUrl(doc.LinkedInUrl), NormalizeUrl(doc.LinkedInUrl) });

            if (!string.IsNullOrWhiteSpace(doc.PortfolioUrl))
            {
                bool gh = doc.PortfolioUrl.IndexOf("github.com",
                              StringComparison.OrdinalIgnoreCase) >= 0;
                cItems.Add(new[] {
                    gh ? "github_icon.png" : "portfolio_icon.png",
                    GetDisplayUrl(doc.PortfolioUrl), NormalizeUrl(doc.PortfolioUrl) });
            }

            if (cItems.Count > 0)
            {
                const string SEP = "  |  ";
                double sepW = tw(SEP, BS);

                var lines = new List<List<string[]>>();
                var currLine = new List<string[]>();
                double currW = 0;

                foreach (var item in cItems)
                {
                    double itemW = tw(item[1], BS);
                    if (imgRgb.ContainsKey(item[0])) itemW += ICON + IGAP;

                    if (currLine.Count > 0 && currW + sepW + itemW > CW)
                    {
                        lines.Add(currLine);
                        currLine = new List<string[]>();
                        currW = 0;
                    }

                    if (currLine.Count > 0) currW += sepW;
                    currW += itemW;
                    currLine.Add(item);
                }
                if (currLine.Count > 0) lines.Add(currLine);

                foreach (var line in lines)
                {
                    double tot = 0;
                    for (int i = 0; i < line.Count; i++)
                    {
                        if (i > 0) tot += sepW;
                        if (imgRgb.ContainsKey(line[i][0])) tot += ICON + IGAP;
                        tot += tw(line[i][1], BS);
                    }

                    double cx = LeftMargin + (CW - tot) / 2.0;
                    if (cx < LeftMargin) cx = LeftMargin;

                    for (int i = 0; i < line.Count; i++)
                    {
                        if (i > 0) { wt(SEP, cx, y, false, BS); cx += sepW; }

                        string icon = line[i][0];
                        string disp = line[i][1];
                        string url  = line[i][2];

                        if (imgRgb.ContainsKey(icon))
                        {
                            string ikey = icon.Replace(".png", "").Replace("_", "");
                            sb.AppendFormat(CultureInfo.InvariantCulture,
                                "q {0:0.##} 0 0 {0:0.##} {1:0.##} {2:0.##} cm /{3} Do Q\n",
                                ICON, cx, y - 1.5, ikey);
                            cx += ICON + IGAP;
                        }

                        double dtw = tw(disp, BS);
                        wt(disp, cx, y, false, BS);

                        if (!string.IsNullOrWhiteSpace(url))
                            annots.Add(new PdfLinkAnnotation
                                { X = cx, Y = y - 2, Width = dtw, Height = BS + 2, Url = url });

                        cx += dtw;
                    }
                    y -= BS * 1.5;
                }
            }

            y -= 7;

            // ── SECTION HEADING: Title Case bold 12pt + rule ──
            Action<string> sec = title =>
            {
                if (y < BottomMargin + 20) return;
                y -= 5;
                string tcTitle = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLowerInvariant());
                wt(tcTitle, LeftMargin, y, true, HS);
                y -= 4;
                hRule(y);
                y -= HS * 0.85;
            };

            // ── PARAGRAPH ──
            Action<string> para = text =>
            {
                if (string.IsNullOrWhiteSpace(text)) return;
                foreach (string ln in WrapText(SanitizeText(text), BS, 0))
                {
                    if (y < BottomMargin + 8) return;
                    wt(ln, LeftMargin, y, false, BS);
                    y -= LH;
                }
            };

            // ── BULLET ITEM (indented 20pt, WinAnsi bullet \225) ──
            Action<string> bul = text =>
            {
                if (string.IsNullOrWhiteSpace(text) || y < BottomMargin + 8) return;
                string clean = SanitizeText(text.TrimStart('-', '*', '\u2022', ' ').Trim());
                if (string.IsNullOrWhiteSpace(clean)) return;

                var wrappedLines = WrapText(clean, BS, 20.0).ToList();
                for (int li = 0; li < wrappedLines.Count; li++)
                {
                    if (y < BottomMargin + 8) return;
                    if (li == 0)
                    {
                        // \225 is octal for WinAnsi bullet •
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "BT /F1 {0} Tf 1 0 0 1 {1:0.##} {2:0.##} Tm (\\225) Tj ET\n",
                            BS, LeftMargin + 8, y);
                        wt(wrappedLines[li], LeftMargin + 20, y, false, BS);
                    }
                    else
                    {
                        wt(wrappedLines[li], LeftMargin + 20, y, false, BS);
                    }
                    y -= LH;
                }
            };

            // ── SKILL LINE: bold "Category:" then regular values ──
            Action<string> skillLine = text =>
            {
                if (string.IsNullOrWhiteSpace(text) || y < BottomMargin + 8) return;
                string clean = SanitizeText(text.Trim());
                int colon = clean.IndexOf(':');
                if (colon > 0)
                {
                    string label  = clean.Substring(0, colon + 1);
                    string values = clean.Substring(colon + 1).TrimStart();
                    double lx     = LeftMargin;
                    wt(label, lx, y, true, BS);
                    lx += tw(label, BS) + BS * 0.3;
                    var vlines = WrapText(values, BS, 0).ToList();
                    for (int li = 0; li < vlines.Count; li++)
                    {
                        if (y < BottomMargin + 8) return;
                        wt(vlines[li], li == 0 ? lx : LeftMargin + tw(label, BS) + BS * 0.3, y, false, BS);
                        if (li < vlines.Count - 1) y -= LH;
                    }
                }
                else
                {
                    wt(clean, LeftMargin, y, false, BS);
                }
                y -= LH;
            };

            // ── RENDER SECTIONS ──

            if (!string.IsNullOrWhiteSpace(doc.Summary))
            {
                sec("Professional Summary");
                para(doc.Summary);
            }

            if (doc.Education != null && doc.Education.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                sec("Education");
                foreach (var e in doc.Education.Where(x => !string.IsNullOrWhiteSpace(x)))
                    bul(e);
            }

            if (doc.Experience != null && doc.Experience.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                sec("Experience");
                foreach (var e in doc.Experience.Where(x => !string.IsNullOrWhiteSpace(x)))
                    bul(e);
            }

            if (doc.Projects != null && doc.Projects.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                sec("Projects");
                foreach (var e in doc.Projects.Where(x => !string.IsNullOrWhiteSpace(x)))
                    bul(e);
            }

            if (doc.Skills != null && doc.Skills.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                sec("Skills");
                para(string.Join(", ", doc.Skills.Where(x => !string.IsNullOrWhiteSpace(x))));
            }

            if (doc.Certifications != null && doc.Certifications.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                sec("Certifications");
                foreach (var e in doc.Certifications.Where(x => !string.IsNullOrWhiteSpace(x)))
                    bul(e);
            }

            if (doc.Languages != null && doc.Languages.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                sec("Languages");
                para(string.Join(", ", doc.Languages
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => SanitizeText(x.Trim()))));
            }

            return BuildResumePdfFile(sb.ToString(), imgRgb, imgAlpha, imgDims, annots);
        }

        // ────────────────────────────────────────────────────────────────────────
        // PDF file builder – clean single-xref structure
        // ────────────────────────────────────────────────────────────────────────
        private static byte[] BuildResumePdfFile(
            string pageContent,
            Dictionary<string, byte[]> imgRgb,
            Dictionary<string, byte[]> imgAlpha,
            Dictionary<string, int[]>  imgDims,
            List<PdfLinkAnnotation>    annots)
        {
            // Collect all PDF objects as (dictionary-string, optional-binary-data) pairs.
            // Index in list == objectNumber - 1 (1-based in PDF).
            var objDicts = new List<string>();
            var objData  = new List<byte[]>(); // null = non-stream object

            // Helper: add a non-stream object
            Action<string> addObj = dict => { objDicts.Add(dict); objData.Add(null); };

            // Helper: add a stream object
            Action<string, byte[]> addStream = (dict, data) =>
            {
                string full = string.Format(CultureInfo.InvariantCulture,
                    "{0}\nstream\n", dict);
                objDicts.Add(full);
                objData.Add(data);
            };

            // obj 1: Catalog (Pages ref filled after we know page obj number)
            addObj("<< /Type /Catalog /Pages 2 0 R >>");       // placeholder

            // obj 2: Pages (Kids filled after we know page obj number)
            addObj("<< /Type /Pages /Kids [] /Count 0 >>");    // placeholder

            // obj 3: Times-Roman
            addObj("<< /Type /Font /Subtype /Type1 /BaseFont /Times-Roman " +
                   "/Encoding /WinAnsiEncoding >>");

            // obj 4: Times-Bold
            addObj("<< /Type /Font /Subtype /Type1 /BaseFont /Times-Bold " +
                   "/Encoding /WinAnsiEncoding >>");

            // obj 5: Times-Italic
            addObj("<< /Type /Font /Subtype /Type1 /BaseFont /Times-Italic " +
                   "/Encoding /WinAnsiEncoding >>");

            // obj 6: Times-BoldItalic
            addObj("<< /Type /Font /Subtype /Type1 /BaseFont /Times-BoldItalic " +
                   "/Encoding /WinAnsiEncoding >>");

            // Image XObjects (SMask first, then RGB)
            var imgObjMap   = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var smaskObjMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in imgRgb)
            {
                string fn  = kv.Key;
                byte[] rgb = kv.Value;
                int[] dims = imgDims[fn];
                int w = dims[0], h = dims[1];

                // SMask (alpha channel)
                if (imgAlpha.ContainsKey(fn))
                {
                    byte[] alp = imgAlpha[fn];
                    string smd = string.Format(CultureInfo.InvariantCulture,
                        "<< /Type /XObject /Subtype /Image /Width {0} /Height {1}" +
                        " /ColorSpace /DeviceGray /BitsPerComponent 8 /Length {2} >>",
                        w, h, alp.Length);
                    addStream(smd, alp);
                    smaskObjMap[fn] = objDicts.Count; // 1-based
                }

                // RGB image
                string smRef = smaskObjMap.ContainsKey(fn)
                    ? string.Format(CultureInfo.InvariantCulture,
                        "/SMask {0} 0 R ", smaskObjMap[fn])
                    : "";
                string imd = string.Format(CultureInfo.InvariantCulture,
                    "<< /Type /XObject /Subtype /Image /Width {0} /Height {1}" +
                    " /ColorSpace /DeviceRGB /BitsPerComponent 8 {2}/Length {3} >>",
                    w, h, smRef, rgb.Length);
                addStream(imd, rgb);
                imgObjMap[fn] = objDicts.Count;
            }

            // Build XObject resource fragment
            var xres = new StringBuilder();
            if (imgObjMap.Count > 0)
            {
                xres.Append("/XObject << ");
                foreach (var kv in imgObjMap)
                {
                    string key = kv.Key.Replace(".png", "").Replace("_", "");
                    xres.AppendFormat(CultureInfo.InvariantCulture,
                        "/{0} {1} 0 R ", key, kv.Value);
                }
                xres.Append(">>");
            }

            // Content stream
            byte[] contentBytes = Encoding.ASCII.GetBytes(pageContent);
            string csDict = string.Format(CultureInfo.InvariantCulture,
                "<< /Length {0} >>", contentBytes.Length);
            addStream(csDict, contentBytes);
            int contentObjNum = objDicts.Count;

            // Annotation objects
            var annotNums = new List<int>();
            foreach (var a in annots)
            {
                string ad = string.Format(CultureInfo.InvariantCulture,
                    "<< /Type /Annot /Subtype /Link" +
                    " /Rect [{0:0.##} {1:0.##} {2:0.##} {3:0.##}]" +
                    " /Border [0 0 0]" +
                    " /A << /Type /Action /S /URI /URI ({4}) >> >>",
                    a.X, a.Y, a.X + a.Width, a.Y + a.Height,
                    EscapePdfText(a.Url));
                addObj(ad);
                annotNums.Add(objDicts.Count);
            }

            // Page object
            var annotArr = new StringBuilder();
            if (annotNums.Count > 0)
            {
                annotArr.Append("/Annots [");
                foreach (int n in annotNums)
                    annotArr.AppendFormat(CultureInfo.InvariantCulture, " {0} 0 R", n);
                annotArr.Append(" ]");
            }

            string pageDict = string.Format(CultureInfo.InvariantCulture,
                "<< /Type /Page /Parent 2 0 R" +
                " /MediaBox [0 0 {0} {1}]" +
                " /Resources << /Font << /F1 3 0 R /F2 4 0 R /F3 5 0 R /F4 6 0 R >> {2} >>" +
                " /Contents {3} 0 R {4} >>",
                PageWidth, PageHeight, xres, contentObjNum, annotArr);
            addObj(pageDict);
            int pageObjNum = objDicts.Count;

            // Fix Catalog (obj 1) and Pages (obj 2)
            objDicts[0] = string.Format(CultureInfo.InvariantCulture,
                "<< /Type /Catalog /Pages 2 0 R >>");
            objDicts[1] = string.Format(CultureInfo.InvariantCulture,
                "<< /Type /Pages /Kids [{0} 0 R] /Count 1 >>", pageObjNum);

            // ── Serialize ──
            var ms      = new MemoryStream();
            var offsets = new List<long>();

            WriteAscii(ms, "%PDF-1.4\n%\xe2\xe3\xcf\xd3\n");

            for (int i = 0; i < objDicts.Count; i++)
            {
                offsets.Add(ms.Position);
                int num = i + 1;

                if (objData[i] == null)
                {
                    // Non-stream object
                    WriteAscii(ms, string.Format(CultureInfo.InvariantCulture,
                        "{0} 0 obj\n{1}\nendobj\n", num, objDicts[i]));
                }
                else
                {
                    // Stream object: dictionary string already ends with "\nstream\n"
                    WriteAscii(ms, string.Format(CultureInfo.InvariantCulture,
                        "{0} 0 obj\n{1}", num, objDicts[i]));
                    ms.Write(objData[i], 0, objData[i].Length);
                    WriteAscii(ms, "\nendstream\nendobj\n");
                }
            }

            // xref table
            long xrefPos = ms.Position;
            int  total   = objDicts.Count + 1; // +1 for free entry 0
            WriteAscii(ms, string.Format(CultureInfo.InvariantCulture,
                "xref\n0 {0}\n", total));
            WriteAscii(ms, "0000000000 65535 f \n");
            foreach (long off in offsets)
                WriteAscii(ms, string.Format(CultureInfo.InvariantCulture,
                    "{0:0000000000} 00000 n \n", off));

            WriteAscii(ms, string.Format(CultureInfo.InvariantCulture,
                "trailer\n<< /Size {0} /Root 1 0 R >>\nstartxref\n{1}\n%%EOF",
                total, xrefPos));

            return ms.ToArray();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string GetDisplayUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            string s = url.Trim()
                .Replace("https://www.", "")
                .Replace("http://www.",  "")
                .Replace("https://",     "")
                .Replace("http://",      "")
                .TrimEnd('/');
            return SanitizeText(s);
        }

        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            string s = url.Trim();
            if (!s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                s = "https://" + s;
            return s;
        }

        private sealed class PdfLinkAnnotation
        {
            public double X      { get; set; }
            public double Y      { get; set; }
            public double Width  { get; set; }
            public double Height { get; set; }
            public string Url    { get; set; }
        }

        private static void WriteAscii(Stream stream, string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string EscapePdfText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text
                .Replace("\\", "\\\\")
                .Replace("(",  "\\(")
                .Replace(")",  "\\)");
        }

        private static string SanitizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            string n = text
                .Replace("\u00A0", " ")
                .Replace("\u2019", "'")
                .Replace("\u2018", "'")
                .Replace("\u201C", "\"")
                .Replace("\u201D", "\"")
                .Replace("\u2013", "-")
                .Replace("\u2014", "-")
                .Replace("\u2022", "-")   // bullet → dash (raw text only)
                .Replace("\u2192", "->");

            var sb = new StringBuilder(n.Length);
            foreach (char c in n)
            {
                if (c == '\r' || c == '\n' || c == '\t') sb.Append(' ');
                else if (c >= 32 && c <= 126)            sb.Append(c);
                else                                     sb.Append('?');
            }
            return sb.ToString().Replace("  ", " ").Trim();
        }

        private static IEnumerable<string> WrapText(string text, int fontSize, double indent)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new[] { string.Empty };

            double usableWidth = PageWidth - LeftMargin - RightMargin - indent;
            int maxChars = Math.Max(28, (int)Math.Floor(usableWidth / Math.Max(4.5, fontSize * 0.52)));

            var lines = new List<string>();
            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return new[] { string.Empty };

            Func<string, int> cleanLen = s => s.Count(c => c != '*' && c != '_');
            var cur = new StringBuilder();
            foreach (string word in words)
            {
                if (cur.Length == 0) { cur.Append(word); continue; }
                if (cleanLen(cur.ToString()) + 1 + cleanLen(word) <= maxChars)
                { cur.Append(' ').Append(word); continue; }
                lines.Add(cur.ToString());
                cur.Clear();
                cur.Append(word);
            }
            if (cur.Length > 0) lines.Add(cur.ToString());
            return lines;
        }
    }
}