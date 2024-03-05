using General_Maintanance.Models.Standings;
using General_Maintanance.Models.Teams;

namespace General_Maintanance.Models.Matches
{
    //set of classes that will ensure successful deserialization of API json data for a RESTful Match request

    public class Match
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string TimeStamp { get; set; }
        public string Timezone { get; set; }
        public string? week { get; set; }
        public Status Status { get; set; }
        public MatchCountry Country { get; set; }
        public LeagueInfo League { get; set; }
        public Teams Teams { get; set; }
        public Scores Scores { get; set; }
    }

    public class Status
    {
        public string Long { get; set; }
        public string Short { get; set; }
    }

    public class MatchCountry
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Teams
    {
        public Team Home { get; set; }
        public Team Away { get; set; }
    }

    public class Scores
    {
        public int Home { get; set; }
        public int Away { get; set; }
        public Periods Periods { get; set; }
    }

    public class Periods
    {
        public Period First { get; set; }
        public Period Second { get; set; }
        public Period Overtime { get; set; }
        public Period SecondOvertime { get; set; }
    }

    public class Period
    {
        private int? home;
        private int? away;

        public int? Home
        {
            get { return home ?? 0; }
            set { home = value; }
        }

        public int? Away
        {
            get { return away ?? 0; }
            set { away = value; }
        }
    }
}
