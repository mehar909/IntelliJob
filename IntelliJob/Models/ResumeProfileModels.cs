using System;
using System.Collections.Generic;

namespace IntelliJob
{
    public class ResumeProfileDocument
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Headline { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new List<string>();
        public List<string> Education { get; set; } = new List<string>();
        public List<string> Experience { get; set; } = new List<string>();
        public List<string> Projects { get; set; } = new List<string>();
        public List<string> Certifications { get; set; } = new List<string>();
        public List<string> Languages { get; set; } = new List<string>();
        public string RawText { get; set; } = string.Empty;
        public string ValidationMessage { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFilePath { get; set; } = string.Empty;
        public DateTime ParsedAt { get; set; }
    }

    public class ResumeImportResult
    {
        public bool IsSuccess { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public string StoredPhysicalPath { get; set; } = string.Empty;
        public string StoredRelativePath { get; set; } = string.Empty;
        public ResumeProfileDocument Document { get; set; }
    }
}