using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using General_Maintanance.Models.Matches;
using General_Maintanance.Models.Standings;
using General_Maintanance.Models.Teams;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace General_Maintanance
{
    public class DataCollection
    {
        //delcaring private global user secrets
        private string connStringSQL;
        private string connStringAppStorage;
        private string apiKey;
        private string apiHost;

        //list of two types of timer intervals for development and production
        //production: 0 0 * * * *
        //development: */5 * * * * *

        //function app to collect all the data from the API and then update or insert into the database based on existing database records
        [FunctionName("DataCollection")]
        public async Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            //setting user secrets using environmental variables
            //these have been set inside the local json settings as well as the azure app settings to work on both platforms
            connStringSQL = Environment.GetEnvironmentVariable("connStringSQL");
            connStringAppStorage = Environment.GetEnvironmentVariable("connStringAppStorage");
            apiKey = Environment.GetEnvironmentVariable("apiKey");
            apiHost = Environment.GetEnvironmentVariable("apiHost");

            //methods to collect and manage data, along with logging to view successful results

            await ManageTeams();

            log.LogInformation($"All Teams data managed successfully at: {DateTime.Now}");

            await ManageMatches();

            log.LogInformation($"All Matches data managed successfully at: {DateTime.Now}");

            await ManagePools();

            log.LogInformation($"All Pools data managed successfully at: {DateTime.Now}");

            await ManageStandings();

            log.LogInformation($"All Standings data managed successfully at: {DateTime.Now}");
        }

        //method to insert new teams into the database based on the API generated list
        private async Task ManageTeams()
        {
            List<Team> teams = await GetTeamsAsync();

            try
            {
                using (SqlConnection connection = new SqlConnection(connStringSQL))
                {
                    connection.Open();

                    foreach (Team team in teams)
                    {
                        // Check if the team already exists in the database
                        string checkQuery = "SELECT COUNT(*) FROM Teams WHERE TeamID = @TeamID";
                        using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@TeamID", team.id);
                            int existingCount = (int)checkCommand.ExecuteScalar();

                            if (existingCount == 0)
                            {
                                string insertQuery = "INSERT INTO Teams (TeamID, TeamName, Flag, CountryCode) " +
                                                    "VALUES (@TeamID, @TeamName, @Flag, @CountryCode)";
                                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                                {
                                    string flag = team.country.flag ?? team.logo;
                                    string code = !string.IsNullOrEmpty(team.country.code) ? team.country.code : (team.name.Length >= 2 ? team.name.Substring(0, 2).ToUpper() : "");

                                    insertCommand.Parameters.AddWithValue("@TeamID", team.id);
                                    insertCommand.Parameters.AddWithValue("@TeamName", team.name);
                                    insertCommand.Parameters.AddWithValue("@Flag", flag);
                                    insertCommand.Parameters.AddWithValue("@CountryCode", code);

                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        //method to insert new pools into the database based on the API generated list
        private async Task ManagePools()
        {
            List<TeamStanding> teamStandings = await GetStandingsAsync();

            var uniqueGroups = teamStandings.Select(standing => standing.group.name.Last()).Distinct();

            try
            {
                using (SqlConnection connection = new SqlConnection(connStringSQL))
                {
                    connection.Open();

                    foreach (var groupName in uniqueGroups)
                    {
                        // Check if the pool already exists in the database
                        string checkQuery = "SELECT COUNT(*) FROM Pools WHERE PoolID = @PoolID";
                        using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@PoolID", groupName);
                            int existingCount = (int)checkCommand.ExecuteScalar();

                            if (existingCount == 0)
                            {
                                string insertQuery = "INSERT INTO Pools (PoolID) VALUES (@PoolID)";
                                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@PoolID", groupName);

                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        //method to insert new standings into the database or update current standings based on the API generated list
        private async Task ManageStandings()
        {
            List<TeamStanding> teamStandings = await GetStandingsAsync();

            try
            {
                using (SqlConnection connection = new SqlConnection(connStringSQL))
                {
                    connection.Open();

                    foreach (TeamStanding standing in teamStandings)
                    {
                        // Check if the standings record already exists in the database
                        string checkQuery = "SELECT PoolID, TeamID, Played, Wins, Losses, Points, Position FROM Standings WHERE PoolID = @PoolID AND TeamID = @TeamID";
                        using (SqlCommand selectCommand = new SqlCommand(checkQuery, connection))
                        {
                            selectCommand.Parameters.AddWithValue("@PoolID", standing.group.name.Last());
                            selectCommand.Parameters.AddWithValue("@TeamID", standing.team.id);

                            using (SqlDataReader reader = selectCommand.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    // Read the existing values from the database
                                    reader.Read();
                                    string existingPoolID = reader.GetString(reader.GetOrdinal("PoolID"));
                                    int existingTeamID = reader.GetInt32(reader.GetOrdinal("TeamID"));
                                    int existingPlayed = reader.GetInt32(reader.GetOrdinal("Played"));
                                    int existingWins = reader.GetInt32(reader.GetOrdinal("Wins"));
                                    int existingLosses = reader.GetInt32(reader.GetOrdinal("Losses"));
                                    int existingPoints = reader.GetInt32(reader.GetOrdinal("Points"));
                                    int existingPosition = reader.GetInt32(reader.GetOrdinal("Position"));

                                    // Close the reader before executing the update command
                                    reader.Close();

                                    // Compare the existing values with the new values from the API
                                    if (existingPoolID != standing.group.name.Last().ToString() ||
                                        existingTeamID != standing.team.id ||
                                        existingPlayed != standing.games.played ||
                                        existingWins != standing.games.win.total ||
                                        existingLosses != standing.games.lose.total ||
                                        existingPoints != standing.points ||
                                        existingPosition != standing.position)
                                    {
                                        // Values are different, perform the update
                                        string updateQuery = "UPDATE Standings SET Played = @Played, Wins = @Wins, " +
                                                             "Losses = @Losses, Points = @Points, Position = @Position " +
                                                             "WHERE PoolID = @PoolID AND TeamID = @TeamID";

                                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                        {
                                            updateCommand.Parameters.AddWithValue("@PoolID", standing.group.name.Last());
                                            updateCommand.Parameters.AddWithValue("@TeamID", standing.team.id);
                                            updateCommand.Parameters.AddWithValue("@Played", standing.games.played);
                                            updateCommand.Parameters.AddWithValue("@Wins", standing.games.win.total);
                                            updateCommand.Parameters.AddWithValue("@Losses", standing.games.lose.total);
                                            updateCommand.Parameters.AddWithValue("@Points", standing.points);
                                            updateCommand.Parameters.AddWithValue("@Position", standing.position);

                                            updateCommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                                else
                                {
                                    // Standings record does not exist, insert a new one
                                    string insertQuery = "INSERT INTO Standings (PoolID, TeamID, Played, Wins, Losses, Points, Position) " +
                                                        "VALUES (@PoolID, @TeamID, @Played, @Wins, @Losses, @Points, @Position)";
                                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                                    {
                                        insertCommand.Parameters.AddWithValue("@PoolID", standing.group.name.Last());
                                        insertCommand.Parameters.AddWithValue("@TeamID", standing.team.id);
                                        insertCommand.Parameters.AddWithValue("@Played", standing.games.played);
                                        insertCommand.Parameters.AddWithValue("@Wins", standing.games.win.total);
                                        insertCommand.Parameters.AddWithValue("@Losses", standing.games.lose.total);
                                        insertCommand.Parameters.AddWithValue("@Points", standing.points);
                                        insertCommand.Parameters.AddWithValue("@Position", standing.position);

                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        //method to insert new matches into the database or update current matches based on the API generated list
        private async Task ManageMatches()
        {
            List<Match> allMatches = await GetMatchesAsync();

            DateTime minimumDate = DateTime.Parse("2023-09-07T12:00:00");

            List<Match> selectedMatches = allMatches.Where(match => DateTime.Parse(match.Date) > minimumDate).ToList();

            try
            {
                using (SqlConnection connection = new SqlConnection(connStringSQL))
                {
                    connection.Open();

                    foreach (Match match in selectedMatches)
                    {
                        // Check if the match already exists in the database
                        string checkQuery = "SELECT COUNT(*) FROM Matches WHERE MatchID = @MatchID";
                        using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@MatchID", match.Id);
                            int existingCount = (int)checkCommand.ExecuteScalar();

                            if (existingCount == 0)
                            {

                                if (match.Status.Long == "Finished")
                                {
                                    await SendPredictionQueue(match);
                                }

                                string insertQuery = "INSERT INTO Matches (MatchID, HomeTeamID, AwayTeamID, HomeScore, AwayScore, MatchDate, Status) " +
                                                    "VALUES (@MatchID, @HomeTeamID, @AwayTeamID, @HomeScore, @AwayScore, @MatchDate, @Status)";
                                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@MatchID", match.Id);
                                    insertCommand.Parameters.AddWithValue("@HomeTeamID", match.Teams.Home.id);
                                    insertCommand.Parameters.AddWithValue("@AwayTeamID", match.Teams.Away.id);
                                    insertCommand.Parameters.AddWithValue("@HomeScore", match.Scores.Home);
                                    insertCommand.Parameters.AddWithValue("@AwayScore", match.Scores.Away);
                                    insertCommand.Parameters.AddWithValue("@MatchDate", DateTime.Parse(match.Date));
                                    insertCommand.Parameters.AddWithValue("@Status", match.Status.Long);

                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Match already exists, perform update if needed
                                string selectQuery = "SELECT HomeTeamID, AwayTeamID, HomeScore, AwayScore, MatchDate, Status FROM Matches WHERE MatchID = @MatchID";
                                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                                {
                                    selectCommand.Parameters.AddWithValue("@MatchID", match.Id);

                                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            // Read the existing values from the database
                                            reader.Read();
                                            int existingHomeTeamID = reader.GetInt32(reader.GetOrdinal("HomeTeamID"));
                                            int existingAwayTeamID = reader.GetInt32(reader.GetOrdinal("AwayTeamID"));
                                            int existingHomeScore = reader.GetInt32(reader.GetOrdinal("HomeScore"));
                                            int existingAwayScore = reader.GetInt32(reader.GetOrdinal("AwayScore"));
                                            DateTime existingMatchDate = reader.GetDateTime(reader.GetOrdinal("MatchDate"));
                                            string existingStatus = reader.GetString(reader.GetOrdinal("Status"));

                                            // Close the reader before executing the update command
                                            reader.Close();

                                            // Compare the existing values with the new values from the API
                                            if (existingHomeTeamID != match.Teams.Home.id ||
                                                existingAwayTeamID != match.Teams.Away.id ||
                                                existingHomeScore != match.Scores.Home ||
                                                existingAwayScore != match.Scores.Away ||
                                                existingMatchDate != DateTime.Parse(match.Date) ||
                                                existingStatus != match.Status.Long)
                                            {
                                                // Values are different, perform the update
                                                string updateQuery = "UPDATE Matches SET HomeTeamID = @HomeTeamID, AwayTeamID = @AwayTeamID, " +
                                                                     "HomeScore = @HomeScore, AwayScore = @AwayScore, " +
                                                                     "MatchDate = @MatchDate, Status = @Status " +
                                                                     "WHERE MatchID = @MatchID";

                                                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                                {
                                                    updateCommand.Parameters.AddWithValue("@HomeTeamID", match.Teams.Home.id);
                                                    updateCommand.Parameters.AddWithValue("@AwayTeamID", match.Teams.Away.id);
                                                    updateCommand.Parameters.AddWithValue("@HomeScore", match.Scores.Home);
                                                    updateCommand.Parameters.AddWithValue("@AwayScore", match.Scores.Away);
                                                    updateCommand.Parameters.AddWithValue("@MatchDate", DateTime.Parse(match.Date));
                                                    updateCommand.Parameters.AddWithValue("@Status", match.Status.Long);
                                                    updateCommand.Parameters.AddWithValue("@MatchID", match.Id);

                                                    if (match.Status.Long == "Finished")
                                                    {
                                                        await SendPredictionQueue(match);
                                                    }

                                                    updateCommand.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        //method to send a queue message to the storage account if a match is finished so that predictions can be assessed
        private async Task SendPredictionQueue(Match match)
        {
            string queueName = "qprediction";

            //creating a queue client object to make use of queue class methods
            QueueClient queueClient = new QueueClient(connStringAppStorage, queueName);

            //create the queue if it doesnt already exist
            await queueClient.CreateIfNotExistsAsync();

            //set the winning team to the home as default
            //set the winning team score to home as default
            int winningTeamID = match.Teams.Home.id;
            int winningTeamScore = match.Scores.Home;

            //set the away score as the losing team score
            int losingTeamScore = match.Scores.Away;

            //if the away score is greater than the home score then set the away team to winning team
            if (match.Scores.Away > match.Scores.Home)
            {
                winningTeamID = match.Teams.Away.id;
                winningTeamScore = match.Scores.Away;

                losingTeamScore = match.Scores.Home;
            }

            //send the finished match to the queue since its ready for predicition handeling
            string message = $"{match.Id}:{winningTeamID}:{winningTeamScore}:{losingTeamScore}";

            //encode the message to base64 to allow for read and write capabilities within the queue trigger
            string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));

            await queueClient.SendMessageAsync(base64Message);

            Console.WriteLine("Message added successfully.");
        }

        //method to return a list of all Teams from the API request
        private async Task<List<Team>> GetTeamsAsync()
        {
            var teams = new List<Team>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", apiHost);

                    var response = await client.GetAsync("https://api-rugby.p.rapidapi.com/teams?league=69&season=2023");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        var root = JsonConvert.DeserializeObject<Teams>(json);

                        teams = root.response;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }

            return teams;
        }

        //method to return a list of all Matches from the API request
        private async Task<List<Match>> GetMatchesAsync()
        {
            var matches = new List<Match>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", apiHost);

                    var response = await client.GetAsync("https://api-rugby.p.rapidapi.com/games?league=69&season=2023");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        var root = JsonConvert.DeserializeObject<Matches>(json);

                        matches = root.response;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }

            return matches;
        }

        //method to return a list of all Standings from the API reuqets (in this case the text file)
        //this method has used blob storage to get the needed text file since the API data did not continue posting standings once the tournament had ended
        private async Task<List<TeamStanding>> GetStandingsAsync()
        {
            //create a BlobServiceClient object which will be used to create a container client
            var blobServiceClient = new BlobServiceClient(connStringAppStorage);

            //create a BlobContainerClient object which will be used to create and manage containers
            var containerClient = blobServiceClient.GetBlobContainerClient("cworldcupstandings");

            //create a BlobClient object which will be used to download the blob
            var blobClient = containerClient.GetBlobClient("UpdatedWorldCupStandings.txt");

            //download the blob content
            using (var response = await blobClient.OpenReadAsync())
            using (var reader = new StreamReader(response))
            {
                string json = await reader.ReadToEndAsync();

                //use NewtonSoft json nuget to deserialize the json object into the Standing list
                Standings standings = JsonConvert.DeserializeObject<Standings>(json);

                List<TeamStanding> teamStandings = standings.response[0];

                return teamStandings;
            }
        }
    }

    //creating a class with a list of matches to successfully get data from the API request
    public class Matches
    {
        public List<Match> response { get; set; }
    }

    //creating a class with a list of standings to successfully get data from the API request
    public class Standings
    {
        public List<List<TeamStanding>> response { get; set; }
    }

    //creating a class with a list of teams to successfully get data from the API request
    public class Teams
    {
        public List<Team> response { get; set; }
    }
}