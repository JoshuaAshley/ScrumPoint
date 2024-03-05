-- Create the Scrumpoint database
CREATE DATABASE Scrumpoint;

-- Use the Scrumpoint database
USE Scrumpoint;

-- Create the Users table
CREATE TABLE Users
(
  UserID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  Username VARCHAR(20) NOT NULL,
  Firstname VARCHAR(50) NOT NULL,
  Surname VARCHAR(50) NOT NULL,
  Email VARCHAR(100) NOT NULL,
  ContactNum VARCHAR(10) NOT NULL,
  Country VARCHAR(75) NOT NULL,
  Password VARCHAR(255) NOT NULL,
  Score FLOAT NOT NULL
);

-- Create the Team table
CREATE TABLE Teams
(
  TeamID INT NOT NULL PRIMARY KEY,
  TeamName VARCHAR(50) NOT NULL,
  Flag VARCHAR(100) NOT NULL,
  CountryCode VARCHAR(5) NOT NULL
);


CREATE TABLE Matches
(
  MatchID INT NOT NULL PRIMARY KEY,
  HomeTeamID INT NOT NULL,
  AwayTeamID INT NOT NULL,
  HomeScore INT NOT NULL,
  AwayScore INT NOT NULL,
  MatchDate DATE NOT NULL,
  Status VARCHAR(20)
);

-- Create the Prediction table
CREATE TABLE Predictions
(
  PredictionID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  MatchID INT NOT NULL,
  UserID INT NOT NULL,
  PredictedWinningTeamID INT NOT NULL,
  HomeTeamScorePrediction INT NOT NULL,
  AwayTeamScorePrediction INT NOT NULL,
  IsChecked BIT
  FOREIGN KEY (MatchID) REFERENCES Matches(MatchID),
  FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Create the junction table between Teams and Matches
CREATE TABLE TeamMatches
(
  TeamMatchID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  TeamID INT NOT NULL,
  MatchID INT NOT NULL,
  FOREIGN KEY (TeamID) REFERENCES Teams(TeamID),
  FOREIGN KEY (MatchID) REFERENCES Matches(MatchID)
);

CREATE TABLE Pools
(
	PoolID VARCHAR(1) PRIMARY KEY NOT NULL
);

CREATE TABLE Standings
(
	PoolID VARCHAR(1) NOT NULL,
	TeamID INT NOT NULL,
	Played INT NOT NULL,
	Wins INT NOT NULL,
	Losses INT NOT NULL,
	Points INT NOT NULL,
	Position INT NOT NULL,
	PRIMARY KEY(PoolID, TeamID),
	FOREIGN KEY (PoolID) REFERENCES Pools(PoolID),
	FOREIGN KEY (TeamID) REFERENCES Teams(TeamID)
);

INSERT INTO Users (Username, Firstname, Surname, Email, ContactNum, Country, Password, Score)
VALUES ('sample_user', 'John', 'Doe', 'john.doe@example.com', '1234567890', 'United States', 'hashed_password', 0.0);

INSERT INTO Users (Username, Firstname, Surname, Email, ContactNum, Country, Password, Score)
VALUES ('sample_user1', 'Mary', 'Sue', 'mary.sue@example.com', '1234567890', 'South Africa', 'hashed_password', 0.0);

INSERT INTO Users (Username, Firstname, Surname, Email, ContactNum, Country, Password, Score)
VALUES ('sample_user2', 'Jane', 'Smith', 'jane.smith@example.com', '1234567890', 'New Zealand', 'hashed_password', 0.0);

INSERT INTO Predictions (MatchID, UserID, PredictedWinningTeamID, HomeTeamScorePrediction, AwayTeamScorePrediction, IsChecked)
VALUES (43599, 3, 467, 11, 12, 0);

INSERT INTO Predictions (MatchID, UserID, PredictedWinningTeamID, HomeTeamScorePrediction, AwayTeamScorePrediction, IsChecked)
VALUES (43567, 3, 467, 20, 10, 0);

INSERT INTO Predictions (MatchID, UserID, PredictedWinningTeamID, HomeTeamScorePrediction, AwayTeamScorePrediction, IsChecked)
VALUES (43598, 3, 460, 10, 5, 0);

INSERT INTO Predictions (MatchID, UserID, PredictedWinningTeamID, HomeTeamScorePrediction, AwayTeamScorePrediction, IsChecked)
VALUES (43599, 4, 467, 7, 20, 0);

INSERT INTO Predictions (MatchID, UserID, PredictedWinningTeamID, HomeTeamScorePrediction, AwayTeamScorePrediction, IsChecked)
VALUES (43567, 4, 467, 5, 4, 0);

INSERT INTO Predictions (MatchID, UserID, PredictedWinningTeamID, HomeTeamScorePrediction, AwayTeamScorePrediction, IsChecked)
VALUES (43598, 4, 460, 43, 12, 0);

INSERT INTO Predictions (MatchID, UserID, PredictedWinningTeamID, HomeTeamScorePrediction, AwayTeamScorePrediction, IsChecked)
VALUES (43599, 5, 467, 29, 30, 0);

INSERT INTO Predictions (MatchID, UserID, PredictedWinningTeamID, HomeTeamScorePrediction, AwayTeamScorePrediction, IsChecked)
VALUES (43567, 5, 467, 4, 0, 0);

INSERT INTO Predictions (MatchID, UserID, PredictedWinningTeamID, HomeTeamScorePrediction, AwayTeamScorePrediction, IsChecked)
VALUES (43598, 5, 460, 1, 0, 0);








select * from matches

select * from Users

select * from Predictions

select * from standings
order by poolID, position

update standings
set points = 0, position = 0, played = 0, wins = 0, losses = 0;

update predictions
set IsChecked = 0;

update users
set score = 0

update matches
set Status = 'Not Started', HomeTeamID = 0, AwayTeamID = 0, HomeScore = 0, AwayScore = 0;