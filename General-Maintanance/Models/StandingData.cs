namespace General_Maintanance.Models.Standings
{
    //set of classes that will ensure successful deserialization of API json data for a RESTful Staning request

    public class TeamStanding
    {
        public int position { get; set; }
        public string stage { get; set; }
        public Group group { get; set; }
        public TeamInfo team { get; set; }
        public LeagueInfo league { get; set; }
        public CountryInfo country { get; set; }
        public GamesInfo games { get; set; }
        public GoalsInfo goals { get; set; }
        public int points { get; set; }
        public string form { get; set; }
        public string description { get; set; }
    }

    public class Group
    {
        public string name { get; set; }
    }

    public class TeamInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
    }

    public class LeagueInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string logo { get; set; }
        public int season { get; set; }
    }

    public class CountryInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string flag { get; set; }
    }

    public class GamesInfo
    {
        public int played { get; set; }
        public GameResult win { get; set; }
        public GameResult draw { get; set; }
        public GameResult lose { get; set; }
    }

    public class GameResult
    {
        public int total { get; set; }
        public string percentage { get; set; }
    }

    public class GoalsInfo
    {
        public int goalsfor { get; set; }
        public int goalsagainst { get; set; }
    }
}
