using System;
using System.Collections.Generic;

namespace IntelliJob
{
    public class ResumeEnhancementResult
    {
        public int OverallScore { get; set; }
        public int AtsScore { get; set; }
        public int SemanticScore { get; set; }
        public int KeywordScore { get; set; }
        public string RawGeminiJson { get; set; }
        public string ResumeSummary { get; set; }
        public string UpdatedResumeText { get; set; }
        public ResumeProfileDocument EnhancedResumeDocument { get; set; }
        public List<string> Strengths { get; set; } = new List<string>();
        public List<string> Gaps { get; set; } = new List<string>();
        public List<string> PriorityKeywords { get; set; } = new List<string>();
        public List<ResumeRewriteSuggestion> RewriteSuggestions { get; set; } = new List<ResumeRewriteSuggestion>();
        public string FinalAssessment { get; set; }
    }

    public class ResumeRewriteSuggestion
    {
        public string SectionName { get; set; }
        public string CurrentObservation { get; set; }
        public string SuggestedRewrite { get; set; }
    }

    public class ResumeEnhancementReportRecord
    {
        public int UserId { get; set; }
        public int AppliedJobId { get; set; }
        public int InterviewId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string JobDescription { get; set; }
        public string ResumePath { get; set; }
        public string ResumeSource { get; set; }
        public string OriginalResumeText { get; set; }
        public string UpdatedResumeText { get; set; }
        public string UpdatedResumeStructuredJson { get; set; }
        public string InterviewFeedback { get; set; }
        public string KeywordHints { get; set; }
        public DateTime GeneratedAt { get; set; }
        public ResumeEnhancementResult Result { get; set; } = new ResumeEnhancementResult();
    }

    public class ApplicationResumeSelection
    {
        public int UserId { get; set; }
        public int AppliedJobId { get; set; }
        public string StoredResumePath { get; set; }
        public string ResumeSource { get; set; }
        public string OriginalFileName { get; set; }
        public string StructuredJson { get; set; }
        public DateTime SavedAt { get; set; }
    }

    public class ApplicationResumeDraftRecord
    {
        public int UserId { get; set; }
        public int JobId { get; set; }
        public string StoredResumePath { get; set; }
        public string ResumeSource { get; set; }
        public string OriginalFileName { get; set; }
        public string StructuredJson { get; set; }
        public string RawText { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime SavedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
