using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PredicitionsSS
{
    public class UpdateUserScore
    {
        //delcaring private global user secrets
        public string connStringSQL;

        //function app to collect all the queue messages that were sent from finished matches and process points and update user scores
        [FunctionName("UpdateUserScore")]
        public async Task RunAsync([QueueTrigger("qprediction")] string myQueueItem, ILogger log)
        {
            //setting user secrets using environmental variables
            //these have been set inside the local json settings as well as the azure app settings to work on both platforms
            connStringSQL = Environment.GetEnvironmentVariable("connStringSQL");

            //splitting the queue message into the 4 parts of the match results
            string[] parts = myQueueItem.Split(':');

            //validation
            if (parts.Length == 4)
            {
                //set the parts to the values of each part
                int matchID = int.Parse(parts[0]);
                int winningTeam = int.Parse(parts[1]);
                int winningTeamScore = int.Parse(parts[2]);
                int losingTeamScore = int.Parse(parts[3]);

                using (SqlConnection connection = new SqlConnection(connStringSQL))
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        await connection.OpenAsync();
                    }

                    string sql = "SELECT * FROM Predictions WHERE IsChecked = 0 AND MatchID = @MatchID";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@MatchID", matchID);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                        {
                            while (await reader.ReadAsync())
                            {
                                int predictionID = reader.GetInt32(reader.GetOrdinal("PredictionID"));
                                int userID = reader.GetInt32(reader.GetOrdinal("UserID"));
                                int predictedWinningTeam = reader.GetInt32(reader.GetOrdinal("PredictedWinningTeamID"));
                                int homeTeamScorePrediction = reader.GetInt32(reader.GetOrdinal("HomeTeamScorePrediction"));
                                int awayTeamScorePrediction = reader.GetInt32(reader.GetOrdinal("AwayTeamScorePrediction"));

                                Console.WriteLine($"UserID: {userID}, HomeTeamScorePrediction: {homeTeamScorePrediction}, AwayTeamScorePrediction: {awayTeamScorePrediction}");

                                int predictedWinningTeamScore = homeTeamScorePrediction;
                                int predictedLosingTeamScore = awayTeamScorePrediction;

                                if (awayTeamScorePrediction > homeTeamScorePrediction)
                                {
                                    predictedWinningTeamScore = awayTeamScorePrediction;
                                    predictedLosingTeamScore = homeTeamScorePrediction;
                                }

                                int userPoints = CalculatePoints(winningTeam, winningTeamScore, losingTeamScore, predictedWinningTeam, predictedWinningTeamScore, predictedLosingTeamScore);

                                Console.WriteLine($"UserID: {userID}, PredictionPoints: {userPoints}");

                                await UpdateDatabaseAsync(connStringSQL, userID, userPoints, predictionID);
                                Console.WriteLine("User Update Successful.");
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid message format");
            }
        }

        private async Task UpdateDatabaseAsync(string connString, int userID, int userPoints, int predictionID)
        {
            using (SqlConnection updateConnection = new SqlConnection(connString))
            {
                await updateConnection.OpenAsync();

                using (SqlCommand updateCommand = updateConnection.CreateCommand())
                {
                    updateCommand.CommandText = "UPDATE Users SET Score = Score + @Points WHERE UserID = @UserID";
                    updateCommand.Parameters.AddWithValue("@UserID", userID);
                    updateCommand.Parameters.AddWithValue("@Points", userPoints);

                    int rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                    Console.WriteLine($"Rows affected: {rowsAffected}");
                }
            }

            using (SqlConnection updatePredictionConnection = new SqlConnection(connString))
            {
                await updatePredictionConnection.OpenAsync();

                using (SqlCommand updatePredictionCommand = updatePredictionConnection.CreateCommand())
                {
                    updatePredictionCommand.CommandText = "UPDATE Predictions SET IsChecked = 1 WHERE PredictionID = @PredictionID";
                    updatePredictionCommand.Parameters.AddWithValue("@PredictionID", predictionID);

                    int rowsAffected = await updatePredictionCommand.ExecuteNonQueryAsync();

                    Console.WriteLine($"Rows affected: {rowsAffected}");
                }
            }
        }

        public int CalculatePoints(int actualWinningTeam, int actualWinningTeamScore, int actualLosingTeamScore, int predictedWinningTeam, int predictedWinningTeamScore, int predictedLosingTeamScore)
        {
            const int MaxPointsForExactMatch = 20;
            const int PointsForCorrectWinner = 5;

            int totalPoints = 0;

            if (predictedWinningTeam == actualWinningTeam)
            {
                if (predictedWinningTeamScore == actualWinningTeamScore && predictedLosingTeamScore == actualLosingTeamScore)
                {
                    totalPoints = MaxPointsForExactMatch;
                }
                else
                {
                    //calculate points based on the difference between actual and predicted scores
                    int scoreDifference = Math.Abs(predictedWinningTeamScore - actualWinningTeamScore) + Math.Abs(predictedLosingTeamScore - actualLosingTeamScore);

                    //apply the points based on the difference with a maximum threshold
                    totalPoints = PointsForCorrectWinner + Math.Max(0, 11 - Math.Min(10, scoreDifference));
                }
            }

            return totalPoints;
        }
    }
}
