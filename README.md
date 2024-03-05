# ScrumPoint
<p align = "center">
</p>

<p align="center">
  <img width="400" height="auto" src="https://github.com/JoshuaAshley/ScrumPoint/blob/main/DIAGRAMS/ScrumPoint_Logo.jpg" alt="ScrumPoint Logo">
</p>


## Online Rugby World Cup Tournament Tracker
### Description
It’s 2023 and that means another year of the Rugby World Cup. Four years ago, the
Springboks made our country proud and won the tournament and are now back
defending their title. With the inclusion of APIs and the advancement of technology
within the sporting world, Team ScrumPoint has made it their goal to unite rugby fans
to keep up to date with the current season of the Rugby World Cup.
ScrumPoint is a Rugby World Cup web-based Tracker that you will be able to view
all events of the World Cup and see the scores of previous games and the current
game. Users of the app can be in different leagues with their friends, and they will be
able to make predictions for every match which will be scored depending on how
close their prediction was to the actual score, they can also choose a player to
predict the man of the match.

## Features
### Functional Requirements
1. Countries – The system shall display a set of all countries that are taking part in
the tournament.
2. Upcoming Matches – The system shall display a list of upcoming matches. <br/>
3. Previous Matches - The system shall display a list of previous matches.<br/>
4. Current Match – The system shall display the current match taking place if a match is occurring at the time of view and will update the scores in real-time.<br/>
5. Standings Pools – The system shall display a list of the standings, such as pools (groups) and countries taking part in each pool.<br/>
6. Pools Countries – The system shall display the number of games, the number of wins and losses, the percentage of wins, and the points per country within a specific pool.<br/>
7. Standings Stages – The system shall display a visual representation of the stages of the tournament following the beginning of the tournament, including stages such as quarter-finals, semi-finals, and the final match, all being updated as the tournament progresses.<br/>
8. Predication Game – The system shall allow users and viewers to compete in a prediction game that will accept predicted winning teams and predicted scores by those teams within matches. Points will be awarded on how accurate these predictions are.<br/>
9. Prediction Game Leaderboard – The system shall display a leaderboard with the top 10 users for the tournament based on their total prediction score.
10. Prediction Game User Progress – The system shall display the user's current progress with their points based on their participation and predictions within the tournament.<br/>

### Azure API General Maintenance
The General Maintenance logic is designed to periodically update and maintain data
related to rugby teams, matches, pools, and standings. The process begins with the
initialization of connection strings, where the application retrieves sensitive
information like database credentials and API keys from secure storage, such as
user secrets or environment variables. Subsequently, the Data Collection function is
executed on a scheduled basis, triggered by a timer that runs every hour. Within this
function, the application fetches the latest information from an external API regarding
rugby teams and matches for a specified season. The obtained data is then
processed, and relevant details are inserted or updated in the Azure SQL database.
This includes actions such as inserting new teams, updating match results, and
managing pool and standings data. Additionally, the system sends information about
finished matches to a prediction queue, enhancing functionality beyond data storage.
Overall, the General Maintenance process ensures that the application's database
reflects the most current and accurate information available, supporting real-time
insights and predictions for rugby events.

### Azure Prediction System
The Prediction System is designed to handle the asynchronous processing of
prediction updates based on finished rugby matches. Triggered by messages in the
Azure Storage Queue, the function extracts relevant information from the message,
including the match ID, winning team, winning team's score, and losing team's score.
It then connects to the Azure SQL Database using the provided connection string
and retrieves predictions from the "Predictions" table that match the specified
criteria. <br/><br/>
For each valid prediction, the function calculates the user's points using a custom
point calculation logic, considering factors such as the predicted and actual winning
teams and their respective scores. The calculated points are then used to update the
user's score in the "Users" table and mark the corresponding prediction as checked
in the "Predictions" table, preventing redundant processing.<br/><br/>
The function demonstrates robust database interaction, utilizing asynchronous
commands to update user scores and prediction statuses. Additionally, it
incorporates a well-defined point calculation method to fairly assess user predictions,
providing an accurate reflection of their forecasting accuracy. This cohesive and
efficient process ensures the seamless functioning of the Prediction System,
contributing to a comprehensive and responsive website.

### Frontend and Deployment
The website was created with ReactNative using static pages. The website was deployed with Vercel and at the time ran perfectly, but to this day, for college cost purposes, the Azure function apps and data storage have been deactivated and deleted thus the website no longer works as intended. However, all the code still exists within each folder named with the corresponding function.<br/>
All information can be found here, [documentation](https://github.com/JoshuaAshley/ScrumPoint/blob/main/DOCUMENTATION/Documentation.pdf).

## Developer/s

### Joshua Ashley
| [Email](mailto:st10060590@vcconnect.edu.za)        |[LinkedIn](https://www.linkedin.com/in/joshua-ashley-857001227/)         |
| ---------------------------------------------------|------------------------------------------------------------------------:|

### Dane Govender
| [Email](mailto:st10176744@vcconnect.edu.za)        |[LinkedIn](https://www.linkedin.com/in/govenderdane/)                  |
| ---------------------------------------------------|------------------------------------------------------------------------:|

### Brandon Calitz
| [Email](mailto:st10039352@vcconnect.edu.za)        |[LinkedIn]()         |
| ---------------------------------------------------|------------------------------------------------------------------------:|

### Keagan Thorp
| [Email](mailto:st10038569@vcconnect.edu.za)        |[LinkedIn]()         |
| ---------------------------------------------------|------------------------------------------------------------------------:|

### Toby Dwyer
| [Email](mailto:st10019602@vcconnect.edu.za)        |[LinkedIn]()         |
| ---------------------------------------------------|------------------------------------------------------------------------:|
