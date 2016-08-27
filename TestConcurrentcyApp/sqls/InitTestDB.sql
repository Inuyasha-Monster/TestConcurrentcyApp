CREATE TABLE [Student] (
  [ID] INTEGER NOT NULL PRIMARY KEY identity, 
  [Name] NVARCHAR(20), 
  [Age] INT, 
  [Sex] INT);

CREATE TABLE [Course] (
  [ID] INTEGER NOT NULL PRIMARY KEY identity, 
  [Name] NVARCHAR(20), 
  [TeacherID] INT) 

CREATE TABLE [Score] (
  [Score] float,   
  [StudentID] INT,
  [CourseID] INT) 

CREATE TABLE [Teacher] (
  [ID] INTEGER NOT NULL PRIMARY KEY identity,  
  [Name] NVARCHAR(20)
)