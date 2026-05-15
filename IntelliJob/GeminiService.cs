using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelliJob
{
    /// <summary>
    /// Service class for interacting with Google Gemini API.
    /// Handles question generation and interview feedback analysis.
    /// </summary>
    public class GeminiService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            // Hard timeout: if Gemini doesn't respond in 20 s, fail fast.
            // Without this the default is 100 s per attempt × 5 models × 2 retries
            // = up to 1000 s of page hang.
            Timeout = System.TimeSpan.FromSeconds(20)
        };
        private readonly string _apiKey;
        private readonly string _model;
        private const string GeminiApiBase = "https://generativelanguage.googleapis.com/v1beta/models/";

        // Fallback models — tried in order when the primary hits rate limits.
        // Updated March 2026: gemini-2.0-x and gemini-1.5-x are fully retired.
        // Free-tier available models as of March 2026: 2.5-flash-lite (15 RPM),
        // 2.5-flash (10 RPM), 2.5-pro (5 RPM).
        private static readonly string[] FallbackModels = new[]
        {
            "gemini-2.5-flash-lite",  // Highest throughput on free tier (15 RPM)
            "gemini-2.5-flash",       // Mid-tier, good balance (10 RPM)
            "gemini-2.5-pro"          // Highest quality, lowest quota (5 RPM)
        };

        public GeminiService()
        {
            _apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured. Set it in Web.config appSettings under 'GeminiApiKey'.");
            }
            _model = ConfigurationManager.AppSettings["GeminiModel"];
            if (string.IsNullOrEmpty(_model))
                _model = "gemini-2.5-flash"; // Default: current free-tier model
        }

        #region Question Generation

        /// <summary>
        /// Generates interview questions using Gemini API based on role, level, type, tech stack, and count.
        /// </summary>
        public async Task<List<string>> GenerateQuestionsAsync(string role, string level, string interviewType, string techStack, int questionCount, List<string> previousQuestions = null, string resumeText = null)
        {
            string avoidSection = "";
            if (previousQuestions != null && previousQuestions.Count > 0)
            {
                avoidSection = "\n\nIMPORTANT: The candidate has already been asked the following questions in previous interviews. You MUST generate completely DIFFERENT questions. Do NOT repeat or rephrase any of these:\n";
                foreach (var q in previousQuestions)
                    avoidSection += $"- {q}\n";
                avoidSection += "\nGenerate fresh, unique questions that cover different aspects of the role and skills.";
            }

            string resumeContext = "";
            if (!string.IsNullOrWhiteSpace(resumeText))
            {
                resumeContext = $"\n\nCANDIDATE RESUME:\n{resumeText}\n\nIf the resume contains clearly separated sections such as Professional Summary, Education, Experience, Projects, Skills, Certifications, or Languages, treat them as structured data. Base the most personalized questions on the strongest and most relevant experience and project entries, not on duplicate or weaker entries.\n";
            }

            string prompt = $@"Prepare questions for a job interview.
The job role is {role}.
The job experience level is {level}.
The tech stack used in the job is: {(string.IsNullOrEmpty(techStack) ? "General" : techStack)}.
The focus between behavioural and technical questions should lean towards: {interviewType}.
The amount of questions required is: {questionCount}.

CRITICAL CRITERIA:
Note: The candidate has APPLIED for this job, they are not currently working there. Frame questions appropriately.
Distribute the questions exactly as follows (or as close as possible given the question count):
- 15% from skills, projects, tools, and certifications mentioned in their resume.
- 50% related to the role and job description to judge what they can do for the role.
- 10% from their past experiences to evaluate if they have a valid experience level.
- 10% scenario-based questions (tailored to the role, resume, and tech stack).
- 10% technical questions (about the tech stack, OOP, DB, DSA, algorithms, etc.).
- The remaining questions (approx 5%) should be about personal interests, future goals, or general industry thought-provoking questions (e.g., ""AI is taking our jobs. What do you think about this?"").
If no resume is provided, distribute the resume-specific percentages into the role-specific questions.

Be creative and vary your questions. Each time this prompt is called, generate a different set of questions covering different angles, scenarios, and depths of the topic.
Please return only the questions, without any additional text.
The questions are going to be read by a voice assistant so do not use special characters like / or * which might break the voice assistant.
Return the questions formatted as a JSON array like this:
[""Question 1"", ""Question 2"", ""Question 3""]{avoidSection}{resumeContext}";

            string responseText = await CallGeminiAsync(prompt, temperature: 1.0);

            // Parse the JSON array from the response
            return ParseQuestionArray(responseText);
        }

        private List<string> ParseQuestionArray(string responseText)
        {
            // Clean up: Gemini sometimes wraps in markdown code blocks
            string cleaned = responseText.Trim();
            if (cleaned.StartsWith("```json"))
                cleaned = cleaned.Substring(7);
            else if (cleaned.StartsWith("```"))
                cleaned = cleaned.Substring(3);
            if (cleaned.EndsWith("```"))
                cleaned = cleaned.Substring(0, cleaned.Length - 3);
            cleaned = cleaned.Trim();

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(cleaned);
            }
            catch
            {
                // Fallback: split by newlines if JSON parsing fails
                var lines = new List<string>();
                foreach (string line in cleaned.Split('\n'))
                {
                    string trimmed = line.Trim().TrimStart('-', '*', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ')').Trim();
                    trimmed = trimmed.Trim('"', ',', '[', ']');
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        lines.Add(trimmed);
                }
                return lines;
            }
        }

        #endregion

        #region Feedback Generation

        /// <summary>
        /// Generates structured interview feedback using Gemini API.
        /// </summary>
        public async Task<InterviewFeedbackResult> GenerateFeedbackAsync(List<TranscriptMessage> transcript, string level = "Mid-Level")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var msg in transcript)
            {
                sb.AppendLine($"- {msg.Role}: {msg.Content}");
            }
            string formattedTranscript = sb.ToString();

            // Determine scoring guidance based on level
            string levelGuidance;
            if (level.IndexOf("junior", StringComparison.OrdinalIgnoreCase) >= 0 ||
                level.IndexOf("entry", StringComparison.OrdinalIgnoreCase) >= 0 ||
                level.IndexOf("fresher", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                levelGuidance = @"This is a JUNIOR/ENTRY-LEVEL candidate. While you should be encouraging:
- ONLY award points for actual answers that address the question
- If the candidate gives no answer or only says 'ok', 'I don't know', or similar, score that category 0-10
- Basic correct answers should score 40-60
- Good answers with some depth should score 60-80
- Excellent, well-articulated answers can score 80-100";
            }
            else if (level.IndexOf("senior", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     level.IndexOf("expert", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     level.IndexOf("lead", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                levelGuidance = @"This is a SENIOR/EXPERT-LEVEL candidate. Evaluate with high expectations:
- ONLY award points for substantial, detailed answers
- Shallow or generic answers should score 20-40
- Competent answers with good depth should score 50-70
- Excellent, insightful answers with strong technical depth should score 70-100
- No answer or 'I don't know' should score 0-5";
            }
            else
            {
                levelGuidance = @"This is a MID-LEVEL candidate. Be fair and accurate:
- ONLY award points for answers that actually address the question
- If the candidate gives no answer or only says 'ok', 'I don't know', or similar, score that category 0-10
- Basic correct answers should score 30-50
- Solid answers with reasonable depth should score 50-70
- Strong, well-explained answers should score 70-100";
            }

            string prompt = $@"You are an AI interviewer analyzing an interview transcript. Your job is to provide ACCURATE scoring STRICTLY based on what the candidate ACTUALLY said.

CRITICAL SCORING RULES - READ CAREFULLY:
1. If a candidate does NOT answer a question (says 'ok', 'let's start', 'I don't know', ends call, or gives irrelevant responses), that question receives a score of 0-5 for all categories.
2. If the candidate barely participates or gives minimal effort, the overall score should be 0-20, NOT 70-80.
3. ONLY give credit for actual substantive answers that address the questions asked.
4. Look at the ENTIRE transcript - if the candidate answered only 1 out of 5 questions, they should score around 10-20, not 70.
5. Empty responses, one-word answers, or refusals to answer MUST result in very low scores (0-10 per category).

Candidate Level: {level}
{levelGuidance}

Transcript:
{formattedTranscript}

BEFORE SCORING: Count how many questions were asked and how many were ACTUALLY answered with substance.
If the candidate answered less than 50% of questions with real answers, the total score MUST be below 30.
If the candidate answered NONE of the questions, the total score MUST be 0-10.

Please score the candidate from 0 to 100 in the following areas. Do not add categories other than the ones provided:
- Communication Skills: Clarity, articulation, structured responses. (0 if no real communication occurred)
- Technical Knowledge: Understanding of key concepts for the role. (0 if no technical answers provided)
- Problem-Solving: Ability to analyze problems and propose solutions. (0 if no problem-solving demonstrated)
- Cultural & Role Fit: Alignment with company values and job role. (0 if no substantive interaction)
- Confidence & Clarity: Confidence in responses, engagement, and clarity. (0 if candidate disengaged or didn't participate)

- Experience Validity: Based on their answers, do they demonstrate genuine past experience? (Score 1 to 10, where 1 is no real experience and 10 is highly validated experience)

Return your response as a JSON object with this exact structure (no markdown, no extra text):
{{
  ""totalScore"": 75,
  ""categoryScores"": [
    {{ ""name"": ""Communication Skills"", ""score"": 80, ""comment"": ""Your comment here"" }},
    {{ ""name"": ""Technical Knowledge"", ""score"": 70, ""comment"": ""Your comment here"" }},
    {{ ""name"": ""Problem Solving"", ""score"": 75, ""comment"": ""Your comment here"" }},
    {{ ""name"": ""Cultural Fit"", ""score"": 78, ""comment"": ""Your comment here"" }},
    {{ ""name"": ""Confidence and Clarity"", ""score"": 72, ""comment"": ""Your comment here"" }},
    {{ ""name"": ""Experience Validity"", ""score"": 8, ""comment"": ""Your comment here"" }}
  ],
  ""strengths"": [""Strength 1"", ""Strength 2"", ""Strength 3""],
  ""areasForImprovement"": [""Area 1"", ""Area 2"", ""Area 3""],
  ""finalAssessment"": ""Your detailed overall assessment here.""
}}";

            string responseText = await CallGeminiAsync(prompt, "You are a professional interviewer analyzing an interview. Your task is to evaluate the candidate based on structured categories. Always respond with valid JSON only.");

            return ParseFeedbackResponse(responseText);
        }

        private InterviewFeedbackResult ParseFeedbackResponse(string responseText)
        {
            // Clean up markdown code fences if present
            string cleaned = responseText.Trim();
            if (cleaned.StartsWith("```json"))
                cleaned = cleaned.Substring(7);
            else if (cleaned.StartsWith("```"))
                cleaned = cleaned.Substring(3);
            if (cleaned.EndsWith("```"))
                cleaned = cleaned.Substring(0, cleaned.Length - 3);
            cleaned = cleaned.Trim();

            try
            {
                JObject json = JObject.Parse(cleaned);
                var result = new InterviewFeedbackResult();

                result.TotalScore = json["totalScore"]?.Value<int>() ?? 0;

                var categories = json["categoryScores"] as JArray;
                if (categories != null)
                {
                    foreach (JObject cat in categories)
                    {
                        string name = cat["name"]?.Value<string>() ?? "";
                        int score = cat["score"]?.Value<int>() ?? 0;
                        string comment = cat["comment"]?.Value<string>() ?? "";

                        if (name.Contains("Communication"))
                        {
                            result.CommunicationScore = score;
                            result.CommunicationComment = comment;
                        }
                        else if (name.Contains("Technical"))
                        {
                            result.TechnicalScore = score;
                            result.TechnicalComment = comment;
                        }
                        else if (name.Contains("Problem"))
                        {
                            result.ProblemSolvingScore = score;
                            result.ProblemSolvingComment = comment;
                        }
                        else if (name.Contains("Cultural") || name.Contains("Fit"))
                        {
                            result.CulturalFitScore = score;
                            result.CulturalFitComment = comment;
                        }
                        else if (name.Contains("Confidence"))
                        {
                            result.ConfidenceScore = score;
                            result.ConfidenceComment = comment;
                        }
                        else if (name.Contains("Experience") || name.Contains("Validity"))
                        {
                            result.ExperienceValidityScore = score;
                            result.ExperienceValidityComment = comment;
                        }
                    }
                }

                var strengths = json["strengths"] as JArray;
                if (strengths != null)
                {
                    result.Strengths = new List<string>();
                    foreach (var s in strengths)
                        result.Strengths.Add(s.Value<string>());
                }

                var areas = json["areasForImprovement"] as JArray;
                if (areas != null)
                {
                    result.AreasForImprovement = new List<string>();
                    foreach (var a in areas)
                        result.AreasForImprovement.Add(a.Value<string>());
                }

                result.FinalAssessment = json["finalAssessment"]?.Value<string>() ?? "";

                return result;
            }
            catch (Exception ex)
            {
                // If parsing fails, return a fallback result with the raw text
                return new InterviewFeedbackResult
                {
                    TotalScore = 0,
                    CommunicationScore = 0,
                    CommunicationComment = "Unable to parse AI response. Please use the Regenerate button to retry.",
                    TechnicalScore = 0,
                    TechnicalComment = "Unable to parse AI response. Please use the Regenerate button to retry.",
                    ProblemSolvingScore = 0,
                    ProblemSolvingComment = "Unable to parse AI response. Please use the Regenerate button to retry.",
                    CulturalFitScore = 0,
                    CulturalFitComment = "Unable to parse AI response. Please use the Regenerate button to retry.",
                    ConfidenceScore = 0,
                    ConfidenceComment = "Unable to parse AI response. Please use the Regenerate button to retry.",
                    Strengths = new List<string> { "Could not generate feedback - parsing error" },
                    AreasForImprovement = new List<string> { "Please use the Regenerate button to retry feedback generation" },
                    FinalAssessment = "ERROR: The AI feedback could not be parsed. Exception: " + ex.Message + ". Please use the Regenerate button."
                };
            }
        }

        #endregion
        
        #region Resume Enhancement

        public async Task<ResumeProfileDocument> ClassifyResumeTextToStructuredJsonAsync(string resumeText, string originalFileName, string storedFilePath)
        {
            if (string.IsNullOrWhiteSpace(resumeText))
                return null;

            string prompt = $@"You are a strict resume field classifier.
Extract fields from the provided resume text into the exact JSON schema below.

CRITICAL RULES:
1) Do not rewrite, summarize, translate, or correct facts.
2) Keep the same words and numbers from the resume text. Do not add, remove, or replace words/tokens.
3) Allowed formatting cleanup only: remove unnecessary extra spaces and apply smart sentence case.
4) Do not change names, emails, phone numbers, usernames, IDs, dates, company names, school names, degree titles, or numeric values.
5) If a value does not exist, return empty string for text fields, [] for arrays, and null for nullable month/year fields.
6) Return JSON only, no markdown.

JSON schema:
{{
  ""personalInfo"": {{
    ""fullName"": """",
    ""email"": """",
    ""mobile"": """",
    ""address"": """",
    ""country"": """",
    ""linkedinUrl"": """",
    ""portfolioUrl"": """"
  }},
  ""professionalSummary"": """",
  ""education"": [
    {{
      ""schoolName"": """",
      ""location"": """",
      ""degree"": """",
      ""startMonth"": null,
      ""startYear"": null,
      ""endMonth"": null,
      ""endYear"": null,
      ""grade"": """",
      ""coursework"": """"
    }}
  ],
  ""experience"": [
    {{
      ""jobTitle"": """",
      ""company"": """",
      ""location"": """",
      ""startMonth"": null,
      ""startYear"": null,
      ""endMonth"": null,
      ""endYear"": null,
      ""isCurrent"": false,
      ""bullets"": []
    }}
  ],
  ""projects"": [
    {{
      ""projectTitle"": """",
      ""techStack"": [],
      ""description"": """"
    }}
  ],
  ""skills"": {{
    ""programmingLanguages"": [],
    ""frameworksLibraries"": [],
    ""toolsCloudDatabase"": [],
    ""softSkillsLanguages"": [],
    ""customSection"": {{
      ""heading"": """",
      ""items"": []
    }}
  }},
  ""metadata"": {{
    ""resumeType"": ""profile"",
    ""originalFileName"": ""{EscapeForPrompt(originalFileName)}"",
    ""storedFilePath"": ""{EscapeForPrompt(storedFilePath)}"",
    ""parsedAt"": null,
    ""updatedAt"": null,
    ""isValid"": true,
    ""validationMessages"": []
  }}
}}

Resume text:
{resumeText}";

            string response = await CallGeminiAsync(prompt, "You are a deterministic JSON extractor. Keep words and numbers unchanged, but normalize spacing and sentence case. Output valid JSON only.", 0.0).ConfigureAwait(false);
            string cleaned = CleanJsonPayload(response);
            ResumeProfileDocument parsed = ResumeProfileService.DeserializeDocument(cleaned);
            if (parsed == null)
                return null;

            parsed.OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? parsed.OriginalFileName : originalFileName;
            parsed.StoredFilePath = string.IsNullOrWhiteSpace(storedFilePath) ? parsed.StoredFilePath : storedFilePath;
            parsed.RawText = resumeText;
            return parsed;
        }

        public async Task<ResumeEnhancementResult> GenerateResumeEnhancementAsync(
            string resumeText,
            string jobTitle,
            string jobDescription,
            string interviewFeedback,
            string mandatoryKeywords)
        {
            string prompt = $@"You are an AI resume enhancer for job seekers.

Your task: produce an optimized, one-page-friendly resume by analysing the candidate's resume against the target role, job description, interview feedback, and keyword hints.

RULES:
- Do NOT invent facts. Only use details already present in the resume.
- When there are multiple Experience entries, select the best-fitting 1 to 3 only (strongest, most relevant to the role, measurable impact preferred).
- When there are multiple Projects, select the best-fitting 1 to 2 only.
- Skills: provide a SINGLE comma-separated list of all relevant skills (no category headers).
- For bullets in Experience and Projects: To make a word bold wrap it like *word*. To make it italic wrap it like _word_ (no spaces inside markers).
- Keep contact details (email, address, LinkedIn, portfolio) exactly as provided, but if a URL is very long shorten it to its display form (e.g. linkedin.com/in/username rather than full https URL).
- Address should be kept concise (should be city/country only if full address is long).
- Preserve section boundaries. Do not merge or skip sections that have content.

Target role: {jobTitle}
Soft keyword hints: {mandatoryKeywords}

Job description:
{jobDescription}

Interview feedback:
{interviewFeedback}

Resume text:
{resumeText}

Return ONLY valid JSON with this EXACT structure. No markdown, no extra keys:
{{
  ""overallScore"": 78,
  ""atsScore"": 74,
  ""semanticScore"": 80,
  ""keywordScore"": 68,
  ""resumeSummary"": ""Short summary of the resume fit for this role."",
  ""enhancedResumeDocument"": {{
     ""personalInfo"": {{
        ""fullName"": ""Candidate's full name unchanged"",
        ""email"": ""Candidate's email address unchanged"",
        ""mobile"": ""Candidate's mobile number unchanged"",
        ""address"": ""Candidate's address (shortened if too long but do not invent or change)"",
        ""country"": ""Candidate's country unchanged"",
        ""linkedinUrl"": ""Shortened LinkedIn URL but the username must be preserved exactly as in the original resume"",
        ""portfolioUrl"": ""Shortened portfolio URL but the username must be preserved exactly as in the original resume""
      }},
      ""professionalSummary"": ""A concise professional summary tailored to the job description and interview feedback."",
      ""education"": [
        {{
          ""schoolName"": ""Name of the educational institution unchanged"",
          ""location"": ""City, Country unchanged"",
          ""degree"": ""Degree information unchanged"",
          ""startMonth"": 1 (unchanged),
          ""startYear"": 2020 (unchanged),
          ""endMonth"": 12 (unchanged),
          ""endYear"": 2024 (unchanged),
          ""grade"": ""Grade or GPA if available, unchanged"",
          ""coursework"": ""Relevant coursework or achievements (concise, tailored to the role, and only if present in the original resume)""
        }},
        {{
          ""schoolName"": ""Name of the second educational institution unchanged (add if multiple entries are present, otherwise omit this object)"",
          ""location"": ""City, Country unchanged"",
          ""degree"": ""Degree information unchanged"",
          ""startMonth"": 1 (unchanged),
          ""startYear"": 2020 (unchanged),
          ""endMonth"": 12 (unchanged),
          ""endYear"": 2024 (unchanged),
          ""grade"": ""Grade or GPA if available, unchanged"",
          ""coursework"": ""Relevant coursework or achievements (concise, tailored to the role, and only if present in the original resume)""
        }}
      ],
      ""experience"": [
        {{
          ""jobTitle"": ""Title of the job experience unchanged (add similar other objects but only for the top 1 to 3 most relevant/strongest experiences if multiple are present)"",
          ""company"": ""Company name unchanged"",
          ""location"": ""City, Country unchanged"",
          ''startMonth'': 1 (unchanged),
          ''startYear'': 2020 (unchanged),
          ''endMonth'': 12 (unchanged),
          ''endYear'': 2024 (unchanged),
          ''isCurrent'': false (unchanged),
          ''bullets'': [
            ''*Key achievement or responsibility #1 with measurable impact*'',
            ''_Key achievement_ or responsibility #2 with relevance to the role'',
            ''Add up to 3 concise, tailored bullet points per experience, but only if they are present in the original resume. Focus on measurable achievements and relevance to the target role. Do not add bullets that are not in the original resume._'' 
          ]
        }}
      ],
      ''projects'': [
        {{
          ''projectTitle'': ''Title of the project unchanged (add similar more objects only if there are multiple projects and the chosen ones must also strong and relevant, 1 to 3 max)''',
          ''techStack'': [''Tech1'', ''Tech2'', ''Tech3''],
          ''description'': ''A concise description of the project with emphasis on achievements and relevance to the role.''
        }}
      ],
      ''skills'': {{
        ''programmingLanguages'': [''Skill1'', ''Skill2'', ''Skill3'', ''add upto 5 max, only if they are present in the original resume and relevant to the role. Similarly for all skill categories, only include if they are present in the original resume and relevant to the role. Do not add skills that are not in the original resume.''],
        ''frameworksLibraries'': [''Skill1'', ''Skill2'', ''Skill3''],
        ''toolsCloudDatabase'': [''Skill1'', ''Skill2'', ''Skill3''],
        ''softSkillsLanguages'': [''Skill1'', ''Skill2'', ''Skill3''],
        ''customSection'': {{
            ''Custom Heading'',
           ''items'': [''Item1'', ''Item2'', ''Item3'']
        }}
    }},
  }},
}},

  ""strengths"": [""Strength 1"", ""Strength 2""],
  ""gaps"": [""Gap 1"", ""Gap 2""],
  ""priorityKeywords"": [""Keyword 1"", ""Keyword 2""],
  ""rewriteSuggestions"": [
    {{
      ""sectionName"": ""Professional Summary"",
      ""currentObservation"": ""What is currently weak or missing"",
      ""suggestedRewrite"": ""A concise rewrite suggestion the user can apply""
    }},
    {{
      ""sectionName"": ""Experience, and similarly add Projects or other sections if suggested (focus on entries which are provided in this JSON only)"",
      ""currentObservation"": ""What is currently weak or missing in this experience entry"",
      ""suggestedRewrite"": ""A concise rewrite suggestion for this experience entry that the user can apply""
    }}
  ],
  ""finalAssessment"": ""A detailed but concise overall assessment.""
}}";

            string responseText = await CallGeminiAsync(prompt, "You are a resume optimization assistant. Always respond with valid JSON only.", 0.35);
            return ParseResumeEnhancementResponse(responseText);
        }

        private ResumeEnhancementResult ParseResumeEnhancementResponse(string responseText)
        {
            string cleaned = responseText.Trim();
            if (cleaned.StartsWith("```json"))
                cleaned = cleaned.Substring(7);
            else if (cleaned.StartsWith("```"))
                cleaned = cleaned.Substring(3);
            if (cleaned.EndsWith("```"))
                cleaned = cleaned.Substring(0, cleaned.Length - 3);
            cleaned = cleaned.Trim();

            try
            {
                JObject json = JObject.Parse(cleaned);
                var result = new ResumeEnhancementResult();
                result.RawGeminiJson = cleaned;

                result.OverallScore = json["overallScore"]?.Value<int>() ?? 0;
                result.AtsScore = json["atsScore"]?.Value<int>() ?? result.OverallScore;
                result.SemanticScore = json["semanticScore"]?.Value<int>() ?? result.OverallScore;
                result.KeywordScore = json["keywordScore"]?.Value<int>() ?? result.OverallScore;
                result.ResumeSummary = json["resumeSummary"]?.Value<string>() ?? string.Empty;
                result.FinalAssessment = json["finalAssessment"]?.Value<string>() ?? string.Empty;

                var strengths = json["strengths"] as JArray;
                if (strengths != null)
                {
                    result.Strengths = new List<string>();
                    foreach (var item in strengths)
                        result.Strengths.Add(item.Value<string>());
                }

                var gaps = json["gaps"] as JArray;
                if (gaps != null)
                {
                    result.Gaps = new List<string>();
                    foreach (var item in gaps)
                        result.Gaps.Add(item.Value<string>());
                }

                var keywords = json["priorityKeywords"] as JArray;
                if (keywords != null)
                {
                    result.PriorityKeywords = new List<string>();
                    foreach (var item in keywords)
                        result.PriorityKeywords.Add(item.Value<string>());
                }

                var rewrites = json["rewriteSuggestions"] as JArray;
                if (rewrites != null)
                {
                    result.RewriteSuggestions = new List<ResumeRewriteSuggestion>();
                    foreach (JObject item in rewrites)
                    {
                        result.RewriteSuggestions.Add(new ResumeRewriteSuggestion
                        {
                            SectionName = item["sectionName"]?.Value<string>() ?? string.Empty,
                            CurrentObservation = item["currentObservation"]?.Value<string>() ?? string.Empty,
                            SuggestedRewrite = item["suggestedRewrite"]?.Value<string>() ?? string.Empty
                        });
                    }
                }

                // Parse the structured enhanced document (same shape as enhanced-resume-format.json / ResumeProfileService exact JSON)
                var docToken = json["enhancedResumeDocument"];
                if (docToken != null && docToken.Type == JTokenType.Object)
                {
                    result.EnhancedResumeDocument = ParseEnhancedDocumentJson((JObject)docToken);
                    if (string.IsNullOrWhiteSpace(result.UpdatedResumeText) && result.EnhancedResumeDocument != null)
                        result.UpdatedResumeText = ResumeProfileService.BuildResumeText(result.EnhancedResumeDocument);
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ResumeEnhancementResult
                {
                    OverallScore = 0,
                    AtsScore = 0,
                    SemanticScore = 0,
                    KeywordScore = 0,
                    RawGeminiJson = string.Empty,
                    ResumeSummary = "Resume enhancement could not be parsed.",
                    UpdatedResumeText = string.Empty,
                    Strengths = new List<string> { "The AI response could not be parsed." },
                    Gaps = new List<string> { "Please try again after the AI service recovers." },
                    PriorityKeywords = new List<string>(),
                    RewriteSuggestions = new List<ResumeRewriteSuggestion>(),
                    FinalAssessment = "ERROR: The AI resume enhancement response could not be parsed. Exception: " + ex.Message
                };
            }
        }

        private ResumeProfileDocument ParseEnhancedDocumentJson(JObject doc)
        {
            if (doc == null)
                return null;
            // Delegates to the same path as profile/application structured JSON (personalInfo, education[], experience[], etc.)
            return ResumeProfileService.DeserializeDocument(doc.ToString());
        }

        #endregion

        #region Gemini API Call

        private static string CleanJsonPayload(string payload)
        {
            string cleaned = (payload ?? string.Empty).Trim();
            if (cleaned.StartsWith("```json"))
                cleaned = cleaned.Substring(7);
            else if (cleaned.StartsWith("```"))
                cleaned = cleaned.Substring(3);
            if (cleaned.EndsWith("```"))
                cleaned = cleaned.Substring(0, cleaned.Length - 3);
            return cleaned.Trim();
        }

        private static string EscapeForPrompt(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private async Task<string> CallGeminiAsync(string prompt, string systemInstruction = null, double temperature = 0.7)
        {
            var requestBody = new JObject();

            // System instruction (optional)
            if (!string.IsNullOrEmpty(systemInstruction))
            {
                requestBody["system_instruction"] = new JObject
                {
                    ["parts"] = new JArray
                    {
                        new JObject { ["text"] = systemInstruction }
                    }
                };
            }

            // User prompt
            requestBody["contents"] = new JArray
            {
                new JObject
                {
                    ["parts"] = new JArray
                    {
                        new JObject { ["text"] = prompt }
                    }
                }
            };

            // Generation config
            requestBody["generationConfig"] = new JObject
            {
                ["temperature"] = temperature,
                ["maxOutputTokens"] = 8192
            };

            string jsonPayload = requestBody.ToString();

            // Build list of models to try: primary first, then fallbacks
            var modelsToTry = new List<string> { _model };
            foreach (string fb in FallbackModels)
            {
                if (!modelsToTry.Contains(fb))
                    modelsToTry.Add(fb);
            }

            Exception lastException = null;

            foreach (string model in modelsToTry)
            {
                string url = $"{GeminiApiBase}{model}:generateContent?key={_apiKey}";

                // Retry up to 2 times per model with backoff
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    try
                    {
                        HttpResponseMessage response = await _httpClient.PostAsync(
                            url, new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                        ).ConfigureAwait(false);
                        string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        if (response.StatusCode == (HttpStatusCode)429)
                        {
                            // Check if it's a daily limit ("PerDay" in the error) — skip retries, try next model
                            if (responseBody.Contains("PerDay"))
                                break;

                            // Per-minute limit: wait and retry
                            int waitSeconds = 5 * (attempt + 1); // keep retries short
                            var retryMatch = Regex.Match(responseBody, @"retryDelay"":\s*""(\d+)");
                            if (retryMatch.Success)
                                waitSeconds = int.Parse(retryMatch.Groups[1].Value) + 3;

                            await Task.Delay(waitSeconds * 1000).ConfigureAwait(false);
                            continue;
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            lastException = new Exception($"Gemini API error ({response.StatusCode}) with model {model}: {responseBody}");
                            break; // Try next model
                        }

                        // Extract text from Gemini response
                        JObject responseJson = JObject.Parse(responseBody);
                        string text = responseJson["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.Value<string>();

                        if (string.IsNullOrEmpty(text))
                        {
                            lastException = new Exception("Empty response from Gemini API (model: " + model + ")");
                            break;
                        }

                        return text;
                    }
                    catch (Exception ex) when (!(ex is InvalidOperationException))
                    {
                        lastException = ex;
                    }
                }
            }

            throw lastException ?? new Exception("All Gemini models rate-limited. Please wait a few minutes and use the Regenerate button.");
        }

        #endregion
    }

    #region Data Models

    public class TranscriptMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class InterviewFeedbackResult
    {
        public int TotalScore { get; set; }
        public int CommunicationScore { get; set; }
        public string CommunicationComment { get; set; }
        public int TechnicalScore { get; set; }
        public string TechnicalComment { get; set; }
        public int ProblemSolvingScore { get; set; }
        public string ProblemSolvingComment { get; set; }
        public int CulturalFitScore { get; set; }
        public string CulturalFitComment { get; set; }
        public int ConfidenceScore { get; set; }
        public string ConfidenceComment { get; set; }
        public int ExperienceValidityScore { get; set; }
        public string ExperienceValidityComment { get; set; }
        public List<string> Strengths { get; set; }
        public List<string> AreasForImprovement { get; set; }
        public string FinalAssessment { get; set; }
    }

    #endregion
}
