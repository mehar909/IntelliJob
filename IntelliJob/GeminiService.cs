using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey;
        private readonly string _model;
        private const string GeminiApiBase = "https://generativelanguage.googleapis.com/v1beta/models/";

        // Fallback models to try when primary model hits rate limits
        private static readonly string[] FallbackModels = new[]
        {
            "gemini-3.1-flash-lite", // Current fast workhorse
            "gemini-3-flash",      // Balanced performance
            "gemini-2.5-flash"     // Older but still stable
        };

        public GeminiService()
        {
            _apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                throw new InvalidOperationException("Gemini API key is not configured. Set it in Web.config appSettings under 'GeminiApiKey'.");
            }
            _model = ConfigurationManager.AppSettings["GeminiModel"];
            if (string.IsNullOrEmpty(_model))
                _model = "gemini-3.1-flash-lite";
        }

        #region Question Generation

        /// <summary>
        /// Generates interview questions using Gemini API based on role, level, type, tech stack, and count.
        /// </summary>
        public async Task<List<string>> GenerateQuestionsAsync(string role, string level, string interviewType, string techStack, int questionCount, List<string> previousQuestions = null)
        {
            string avoidSection = "";
            if (previousQuestions != null && previousQuestions.Count > 0)
            {
                avoidSection = "\n\nIMPORTANT: The candidate has already been asked the following questions in previous interviews. You MUST generate completely DIFFERENT questions. Do NOT repeat or rephrase any of these:\n";
                foreach (var q in previousQuestions)
                    avoidSection += $"- {q}\n";
                avoidSection += "\nGenerate fresh, unique questions that cover different aspects of the role and skills.";
            }

            string prompt = $@"Prepare questions for a job interview.
The job role is {role}.
The job experience level is {level}.
The tech stack used in the job is: {(string.IsNullOrEmpty(techStack) ? "General" : techStack)}.
The focus between behavioural and technical questions should lean towards: {interviewType}.
The amount of questions required is: {questionCount}.
Be creative and vary your questions. Each time this prompt is called, generate a different set of questions covering different angles, scenarios, and depths of the topic.
Please return only the questions, without any additional text.
The questions are going to be read by a voice assistant so do not use special characters like / or * which might break the voice assistant.
Return the questions formatted as a JSON array like this:
[""Question 1"", ""Question 2"", ""Question 3""]{avoidSection}";

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
                levelGuidance = "This is a JUNIOR/ENTRY-LEVEL candidate. Be encouraging and lenient. If the candidate shows basic understanding and attempts to answer correctly, give generous scores. Minimum score for any category should be around 30 if the candidate made a reasonable attempt. A junior who answers most questions with basic correctness should score 60-80.";
            }
            else if (level.IndexOf("senior", StringComparison.OrdinalIgnoreCase) >= 0 || 
                     level.IndexOf("expert", StringComparison.OrdinalIgnoreCase) >= 0 || 
                     level.IndexOf("lead", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                levelGuidance = "This is a SENIOR/EXPERT-LEVEL candidate. Evaluate with higher expectations but remain fair. Score across the full 0-100 range based on depth, accuracy, and communication quality.";
            }
            else
            {
                levelGuidance = "This is a MID-LEVEL candidate. Be fair but supportive. If the candidate shows reasonable understanding, give appropriate credit. Minimum score for any category should be around 15 if the candidate made an attempt. A mid-level candidate answering with moderate correctness should score 50-75.";
            }

            string prompt = $@"You are an AI interviewer analyzing an interview. Evaluate the candidate fairly and constructively. Focus on what the candidate demonstrated well, while noting areas for improvement.

Candidate Level: {level}
{levelGuidance}

Transcript:
{formattedTranscript}

Please score the candidate from 0 to 100 in the following areas. Do not add categories other than the ones provided:
- Communication Skills: Clarity, articulation, structured responses.
- Technical Knowledge: Understanding of key concepts for the role.
- Problem-Solving: Ability to analyze problems and propose solutions.
- Cultural & Role Fit: Alignment with company values and job role.
- Confidence & Clarity: Confidence in responses, engagement, and clarity.

Return your response as a JSON object with this exact structure (no markdown, no extra text):
{{
  ""totalScore"": 75,
  ""categoryScores"": [
    {{ ""name"": ""Communication Skills"", ""score"": 80, ""comment"": ""Your comment here"" }},
    {{ ""name"": ""Technical Knowledge"", ""score"": 70, ""comment"": ""Your comment here"" }},
    {{ ""name"": ""Problem Solving"", ""score"": 75, ""comment"": ""Your comment here"" }},
    {{ ""name"": ""Cultural Fit"", ""score"": 78, ""comment"": ""Your comment here"" }},
    {{ ""name"": ""Confidence and Clarity"", ""score"": 72, ""comment"": ""Your comment here"" }}
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
                    TotalScore = 50,
                    CommunicationScore = 50,
                    CommunicationComment = "Unable to parse AI response. Please try again.",
                    TechnicalScore = 50,
                    TechnicalComment = "Unable to parse AI response. Please try again.",
                    ProblemSolvingScore = 50,
                    ProblemSolvingComment = "Unable to parse AI response. Please try again.",
                    CulturalFitScore = 50,
                    CulturalFitComment = "Unable to parse AI response. Please try again.",
                    ConfidenceScore = 50,
                    ConfidenceComment = "Unable to parse AI response. Please try again.",
                    Strengths = new List<string> { "Could not generate feedback" },
                    AreasForImprovement = new List<string> { "Please retake the interview" },
                    FinalAssessment = "The AI feedback could not be parsed. Raw response: " + responseText
                };
            }
        }

        #endregion

        #region Gemini API Call

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
                            int waitSeconds = 20 * (attempt + 1);
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
        public List<string> Strengths { get; set; }
        public List<string> AreasForImprovement { get; set; }
        public string FinalAssessment { get; set; }
    }

    #endregion
}
