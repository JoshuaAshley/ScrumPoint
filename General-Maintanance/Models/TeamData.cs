namespace General_Maintanance.Models.Teams
{
    //set of classes that will ensure successful deserialization of API json data for a RESTful Team request

    public class Team
    {
        public int id { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
        public bool national { get; set; }
        public int? founded { get; set; }
        public Arena arena { get; set; }
        public Country country { get; set; }
    }

    public class Arena
    {
        public string name { get; set; }
        public string capacity { get; set; }
        public string location { get; set; }
    }

    public class Country
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string flag { get; set; }
    }
}
