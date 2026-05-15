using System;
using System.Collections.Generic;

namespace IntelliJob
{
    public class ResumePersonalInfo
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string LinkedInUrl { get; set; } = string.Empty;
        public string PortfolioUrl { get; set; } = string.Empty;
    }

    public class ResumeSectionNode
    {
        public string Heading { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new List<string>();
        public List<ResumeSectionNode> Subsections { get; set; } = new List<ResumeSectionNode>();
    }

    public class ResumeEducationEntry
    {
        public string SchoolName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public int? StartMonth { get; set; }
        public int? StartYear { get; set; }
        public int? EndMonth { get; set; }
        public int? EndYear { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string Coursework { get; set; } = string.Empty;
    }

    public class ResumeExperienceEntry
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int? StartMonth { get; set; }
        public int? StartYear { get; set; }
        public int? EndMonth { get; set; }
        public int? EndYear { get; set; }
        public bool IsCurrent { get; set; }
        public List<string> Bullets { get; set; } = new List<string>();
    }

    public class ResumeProjectEntry
    {
        public string ProjectTitle { get; set; } = string.Empty;
        public List<string> TechStack { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
    }

    public class ResumeSkillGroups
    {
        public List<string> ProgrammingLanguages { get; set; } = new List<string>();
        public List<string> FrameworksLibraries { get; set; } = new List<string>();
        public List<string> ToolsCloudDatabaseSkills { get; set; } = new List<string>();
        public List<string> SoftSkillsLanguages { get; set; } = new List<string>();
        public string CustomHeading { get; set; } = string.Empty;
        public List<string> CustomItems { get; set; } = new List<string>();
    }

    public class ResumeMetadata
    {
        public string ResumeType { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFilePath { get; set; } = string.Empty;
        public DateTime ParsedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsValid { get; set; }
        public List<string> ValidationMessages { get; set; } = new List<string>();
    }

    public class ResumeProfileDocument
    {
        public int SchemaVersion { get; set; } = 1;
        public ResumePersonalInfo PersonalInfo { get; set; } = new ResumePersonalInfo();
        public string ProfessionalSummary { get; set; } = string.Empty;
        public List<ResumeEducationEntry> EducationDetails { get; set; } = new List<ResumeEducationEntry>();
        public List<ResumeExperienceEntry> ExperienceDetails { get; set; } = new List<ResumeExperienceEntry>();
        public List<ResumeProjectEntry> ProjectDetails { get; set; } = new List<ResumeProjectEntry>();
        public ResumeSkillGroups SkillGroups { get; set; } = new ResumeSkillGroups();
        public List<ResumeSectionNode> Sections { get; set; } = new List<ResumeSectionNode>();
        public ResumeMetadata Metadata { get; set; } = new ResumeMetadata();

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Headline { get; set; } = string.Empty;
        public string LinkedInUrl { get; set; } = string.Empty;
        public string PortfolioUrl { get; set; } = string.Empty;
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