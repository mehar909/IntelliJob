-- ============================================
-- Interview Feature - Database Tables
-- Run this script against the IntelliJob database
-- ============================================

-- 1. Interviews: Stores each interview session setup
CREATE TABLE Interviews (
    InterviewId     INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    Role            NVARCHAR(100) NOT NULL,        -- e.g. "Software Engineer"
    Level           NVARCHAR(50) NOT NULL,          -- Junior / Mid / Senior
    InterviewType   NVARCHAR(50) NOT NULL,          -- Technical / Behavioral / Mixed
    TechStack       NVARCHAR(500) NULL,             -- Comma-separated: "C#, ASP.NET, SQL Server"
    QuestionCount   INT NOT NULL DEFAULT 5,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'pending',  -- pending / in-progress / completed
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    CompletedAt     DATETIME NULL,
    CONSTRAINT FK_Interviews_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- 2. InterviewQuestions: Questions generated for each interview
CREATE TABLE InterviewQuestions (
    QuestionId      INT IDENTITY(1,1) PRIMARY KEY,
    InterviewId     INT NOT NULL,
    QuestionText    NVARCHAR(MAX) NOT NULL,
    SortOrder       INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Questions_Interview FOREIGN KEY (InterviewId) REFERENCES Interviews(InterviewId)
);

-- 3. InterviewTranscripts: Stores conversation messages during interview
CREATE TABLE InterviewTranscripts (
    TranscriptId    INT IDENTITY(1,1) PRIMARY KEY,
    InterviewId     INT NOT NULL,
    SpeakerRole     NVARCHAR(20) NOT NULL,          -- 'user' or 'assistant'
    Content         NVARCHAR(MAX) NOT NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Transcripts_Interview FOREIGN KEY (InterviewId) REFERENCES Interviews(InterviewId)
);

-- 4. InterviewFeedback: AI-generated feedback after interview ends
CREATE TABLE InterviewFeedback (
    FeedbackId          INT IDENTITY(1,1) PRIMARY KEY,
    InterviewId         INT NOT NULL,
    UserId              INT NOT NULL,
    TotalScore          INT NOT NULL DEFAULT 0,          -- 0-100
    CommunicationScore  INT NOT NULL DEFAULT 0,
    CommunicationComment NVARCHAR(MAX) NULL,
    TechnicalScore      INT NOT NULL DEFAULT 0,
    TechnicalComment    NVARCHAR(MAX) NULL,
    ProblemSolvingScore INT NOT NULL DEFAULT 0,
    ProblemSolvingComment NVARCHAR(MAX) NULL,
    CulturalFitScore    INT NOT NULL DEFAULT 0,
    CulturalFitComment  NVARCHAR(MAX) NULL,
    ConfidenceScore     INT NOT NULL DEFAULT 0,
    ConfidenceComment   NVARCHAR(MAX) NULL,
    Strengths           NVARCHAR(MAX) NULL,              -- Pipe-separated: "Clear speech|Good examples"
    AreasForImprovement NVARCHAR(MAX) NULL,              -- Pipe-separated
    FinalAssessment     NVARCHAR(MAX) NULL,
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Feedback_Interview FOREIGN KEY (InterviewId) REFERENCES Interviews(InterviewId),
    CONSTRAINT FK_Feedback_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
