create database intellijob

use intellijob

create table Country(
CountryId int primary key identity(1,1),
CountryName varchar(50)
)

Insert into Country values ('Pakistan'),('India'),('Bangladesh'),('United States'),('Turkey'),('China'),('Brazil'),('Saudi Arabia'), ('UAE'),('Kuwait')


CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Password VARCHAR(200) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    Email VARCHAR(50) UNIQUE NOT NULL,
    Address VARCHAR(MAX),
    Country VARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE JobSeekers (
    ProfileId INT PRIMARY KEY,          -- FK to Users.UserId
    Name VARCHAR(50),
    Mobile VARCHAR(50),
    TenthGrade VARCHAR(50),
    TwelfthGrade VARCHAR(50),
    GraduationGrade VARCHAR(50),
    PostGraduationGrade VARCHAR(50),
    Phd VARCHAR(50),
    WorksOn VARCHAR(50),
    Resume VARCHAR(MAX),
    Photo VARCHAR(MAX) DEFAULT 'avatar.png',
    Experience VARCHAR(50),
    CONSTRAINT FK_JobSeekers_Users 
        FOREIGN KEY (ProfileId) REFERENCES Users(UserId)
);

CREATE TABLE Companies (
    CompanyId INT PRIMARY KEY,            -- FK to Users.UserId
    CompanyName VARCHAR(200) NOT NULL,
    Website VARCHAR(100) NULL,
    Description VARCHAR(MAX) NULL,
    CompanyLogo VARCHAR(MAX) DEFAULT 'company_logo.png',
    CompanySize INT,
    CONSTRAINT FK_Companies_Users
        FOREIGN KEY (CompanyId) REFERENCES Users(UserId)
); 


create table Contact(
ContactId int primary key identity(1,1) not null,
Name varchar(50),
Email varchar(50),
Subject varchar(100),
Message varchar(Max)
)

select* from Contact

Create table Jobs(
JobId int primary key identity (1,1),
Title varchar(50),
NoOfPost int,
Description varchar(MAX),
Qualification varchar(50),
Experience varchar(50),
Specialization varchar(MAX),
LastDateToApply Date,
Salary varchar(50),
JobType varchar(50),
CompanyName varchar(200),
CompanyImage varchar(500),
Website varchar(100),
Email varchar(50),
Address varchar(MAX),
Country varchar(50),
State varchar(50),
CreateDate datetime
);

select* from Jobs;

CREATE TABLE FeaturedMarks (
    JobId INT PRIMARY KEY,
    isFeatured BIT NOT NULL DEFAULT 0,
    -- Add foreign key constraint to link with the Jobs table
    FOREIGN KEY (JobId) REFERENCES Jobs(JobId)
);

select* from FeaturedMarks;

create table AppliedJobs(
AppliedJobId int primary key identity,
JobId int,
UserId int,
Shortlisted varchar(3)
)

select* from AppliedJobs


CREATE TABLE UserFavorites (
    FavoriteId INT IDENTITY PRIMARY KEY,
    UserId NVARCHAR(50) NOT NULL,
    JobId INT NULL,
    ExternalJobId NVARCHAR(500) NULL,
    JobType NVARCHAR(50) NOT NULL, -- 'database' or 'linkedin'
    Title NVARCHAR(200),
    CompanyName NVARCHAR(200),
    Location NVARCHAR(200),
    JobUrl NVARCHAR(500),
    CompanyLogo NVARCHAR(500),
	PostedTime NVARCHAR(100),       -- e.g., "3 days ago", from LinkedIn API
    CreateDate NVARCHAR(100),-- actual time the job was added
    AddedOn DATETIME DEFAULT GETDATE()
);

select * from UserFavorites

CREATE TABLE AdminContact (
    ContactId INT PRIMARY KEY IDENTITY,
    UserId INT,
    Name NVARCHAR(100),
    Email NVARCHAR(100),
    Message NVARCHAR(MAX),
    Date DATETIME
);

select * from AdminContact

CREATE TABLE AdminReply (
    ReplyId INT PRIMARY KEY IDENTITY,
    ContactId INT FOREIGN KEY REFERENCES AdminContact(ContactId),
    ReplyMessage NVARCHAR(MAX),
    ReplyDate DATETIME
);

select * from AdminReply


select * from Users

select * from JobSeekers

select * from Companies

select * from Jobs

select * from AppliedJobs


