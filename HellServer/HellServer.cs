extern alias Distance;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using WorkshopSearch;

namespace HellServer
{
    public class HellServer : DistanceServerPlugin
    {
        public override string DisplayName => "Hell Server";
        public override string Author => "Tribow; Discord: Tribow";
        public override int Priority => -6;
        public override SemanticVersion ServerVersion => new SemanticVersion("0.4.0");

        private List<string> blacklistedLevels = new List<string>()
        {
            "462920909", //Underground (BCPowerhouse)
            "608581734", //Fun With Saws
            "698910049", //Storm (NEBULA)
            "757229210", //Duust
            "739443870", //Monolith pure
            "839128031", //Framerate Crusher (Trackmogrify)
            "854888666", //Ride to the Accelodrome
            "1118651291", //Waluigi Dystopia (WIP)
            "1242493298", //Ignition [Acceleracers Role-Play Map
            "1794707950", //Broken Symmetry But You Have A Minute
            "2064264537", //Terminus 2 Pure
            "2495752655", //Zipzop 2.1
            "2412825433", //always strive to be better
            "1829045376", //[ff0000]skip[-]
            "2266021888", //Cheaters Chamber
            "851254413", //Side Winder
            "585882210" //001
        };
        private int levelsCompleted = 0;
        private bool playerFinished = false;
        private bool updatingPlaylist = false; //bool not actually used but that's okay
        private bool diversionlmao = true;

        public override void Start()
        {
            Log.Info("Welcome to [ H E L L ]");

            DistanceServerMainStarter.Instance.StartCoroutine(FindWorkshopLevels());

            //Things to do once a level is initiated.
            Server.OnLevelStartInitiatedEvent.Connect(() =>
            {
                Random rnd = new Random();
                if (rnd.Next(0, 8) < 1)
                {
                    Server.SayChat(DistanceChat.Server("HellServer:serverVersion", "Server Version: v1.2.1"));
                }

                playerFinished = false;
                
                if(levelsCompleted >= 300)
                {
                    DistanceServerMainStarter.Instance.StartCoroutine(UpdatePlaylist());
                    Server.SayChat(DistanceChat.Server("HellServer:serverUpdate", "[FF0000]HELL[-] is under maintenance. Don't worry you can keep playing."));
                    levelsCompleted = 0;
                }
            });

            //Level Start Event
            Server.OnLevelStartedEvent.Connect(() =>
            {
                //Checking if the current level is an impossible one.
                foreach(string levelid in blacklistedLevels)
                {
                    if(levelid == Server.CurrentLevel.WorkshopFileId)
                    {
                        Server.SayChat(DistanceChat.Server("HellServer:blacklistedmessage", "[FF4C00]This level is impossible! Due to this[-] [FF0000]HELL[-] [FF4C00]will continue once all players have given up.[-]"));
                        playerFinished = true;
                    }
                }
            });

            //Side wheelie easter egg
            DistanceServerMain.GetEvent<Events.Instanced.TrickComplete>().Connect(trickData =>
            {
                if (trickData.sideWheelieMeters_ > 20)
                {
                    Random rnd = new Random();
                    if (rnd.Next(0, 11) < 1)
                    {
                        Server.SayChat(DistanceChat.Server("Glicko2Rankings:sidewheelie", $"SIIICK {trickData.sideWheelieMeters_} METER SIDE WHEELIE"));
                    }
                }
            });

            //Commands
            Server.OnChatMessageEvent.Connect((chatMessage) =>
            {
                //The "/explain" command exists to explain hell server lmao
                if (Regex.Match(chatMessage.Message, @"(?<=^\[[0-9A-F]{6}\].+\[FFFFFF\]: /explain).*$").Success)
                {
                    Server.SayChat(DistanceChat.Server("HellServer:explaintime", "HELL SERVER is a server where any Sprint level on the workshop can be randomly selected play. The server will not load the next level unless someone is able to beat the level or 24 hours pass. Once everyone is done playing the level the server will choose the next level."));
                }

                //YOU CANT SKIP HERE AAHAHAHAHHA
                if (Regex.Match(chatMessage.Message, @"(?<=^\[[0-9A-F]{6}\].+\[FFFFFF\]: /skip).*$").Success)
                {
                    Server.SayChat(DistanceChat.Server("HellServer:skiplmao", "THERE ARE NO SKIPS IN [FF0000]HELL[-]."));
                }

                //Okay I intend for this to be a fail safe of sorts but I don't want it to be abuseable. 
                if (Regex.Match(chatMessage.Message, @"(?<=^\[[0-9A-F]{6}\].+\[FFFFFF\]: /impossible).*$").Success)
                {
                    Server.SayChat(DistanceChat.Server("HellServer:impossiblelevel", "Tribow write something useful here lmao."));
                }
            });

            DistanceServerMain.GetEvent<Events.Instanced.Finished>().Connect((instance, data) =>
            {
                CheckIfHellCanLoadNextLevel();
            });

            Server.OnPlayerDisconnectedEvent.Connect((handler) =>
            {
                CheckIfHellCanLoadNextLevel();
            });
        }

        /// <summary>
        /// Finds ALL workshop levels to put into the playlist for the server. Logs what it finds as well.
        /// Adds what it finds into a shuffled list for BasicAutoServer to use.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator FindWorkshopLevels()
        {
            updatingPlaylist = true;
            DistanceSearchRetriever retriever = SetReceiver(WorkshopSearchParameters.GameFiles("233610"), false);

            if (retriever == null)
            {
                Log.Error("No workshop levels defined.");
            }
            else
            {
                Log.Debug("Scanning Workshop...");
                retriever.StartCoroutine();
                yield return retriever.TaskCoroutine;
                if (retriever.HasError)
                {
                    Log.Error($"Error retrieving levels: {retriever.Error}");
                }

                List<DistanceLevel> results = retriever.Results.ConvertAll(result => result.DistanceLevelResult);

                if (results.Count == 0)
                {
                    Log.Error("Workshop search returned nothing");
                }
                else
                {
                    //AutoServer playlist gets updated with the first batch here
                    BasicAutoServer.BasicAutoServer AutoServer = DistanceServerMain.Instance.GetPlugin<BasicAutoServer.BasicAutoServer>();
                    AutoServer.Playlist.Clear();
                    AutoServer.Playlist.AddRange(results);
                    int looped = 0;

                    //Loop searching until the entire workshop gets grabbed
                    while (results.Count != 0)
                    {
                        looped++;
                        Log.Debug($"Scanning Workshop[{looped}]...");
                        retriever = SetReceiver(WorkshopSearchParameters.GameFiles("233610", "", WorkshopSearchParameters.SortType.Default, WorkshopSearchParameters.FilterType.Default, -1, new string[] { "Sprint" }, looped + 1, 30), true);
                        yield return retriever.TaskCoroutine;
                        if (retriever.HasError)
                        {
                            Log.Error($"Error retrieving levels: {retriever.Error}");
                            break;
                        }

                        results = retriever.Results.ConvertAll(result => result.DistanceLevelResult);

                        if (results.Count == 0)
                        {
                            Log.Debug("No more levels to grab");
                            break;
                        }
                        AutoServer.Playlist.AddRange(results);
                    }

                    //NOW shuffle the playlist
                    AutoServer.Playlist.Shuffle();

                    LogLevels(AutoServer.Playlist, 0);

                    //The first level that gets chosen is always diversion so this will skip diversion I think maybe
                    if (diversionlmao)
                    {
                        diversionlmao = false;
                        AutoServer.NextLevel();

                    }
                }
            }
            updatingPlaylist = false;
            yield break;
        }

        /// <summary>
        /// Updates the current playlist of levels with any NEW level that exists on the workshop. Once new levels are added, it reshuffles.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator UpdatePlaylist()
        {
            updatingPlaylist = true;
            DistanceSearchRetriever retriever = SetReceiver(WorkshopSearchParameters.GameFiles("233610", "", WorkshopSearchParameters.SortType.Recent, WorkshopSearchParameters.FilterType.Default, -1, null, 1, 30), false);
            

            if (retriever == null)
            {
                Log.Error("No workshop levels defined.");
            }
            else
            {
                Log.Debug("Scanning Workshop For New Levels...");
                retriever.StartCoroutine();
                yield return retriever.TaskCoroutine;
                if (retriever.HasError)
                {
                    Log.Error($"Error retrieving levels: {retriever.Error}");
                }

                List<DistanceLevel> results = retriever.Results.ConvertAll(result => result.DistanceLevelResult);

                if (results.Count == 0)
                {
                    Log.Error("Workshop search returned nothing....WELP NOTHIN NEW!");
                    updatingPlaylist = false;
                    yield break;
                }
                else
                {
                    BasicAutoServer.BasicAutoServer AutoServer = DistanceServerMain.Instance.GetPlugin<BasicAutoServer.BasicAutoServer>();

                    int removeFrom = RemoveLevelsStartingFromHere(results, AutoServer.Playlist);
                    int previousLevelAmountInPlaylist = AutoServer.Playlist.Count - 1;
                    int looped = 0;

                    //Start going for more levels if there is nothing to remove! Keep looping until there's nothing new to grab.
                    while (removeFrom == results.Count)
                    {
                        //Add the current new levels into the playlist
                        AutoServer.Playlist.AddRange(results);

                        looped++;
                        Log.Debug("Scanning for more New Workshop Levels...");
                        retriever = SetReceiver(WorkshopSearchParameters.GameFiles("233610", "", WorkshopSearchParameters.SortType.Recent, WorkshopSearchParameters.FilterType.Default, -1, null, looped + 1, 30), true);
                        yield return retriever.TaskCoroutine;
                        if (retriever.HasError)
                        {
                            Log.Error($"Error retrieving levels: {retriever.Error}");
                            break;
                        }

                        results = retriever.Results.ConvertAll(result => result.DistanceLevelResult);

                        if (results.Count == 0)
                        {
                            Log.Error("Workshop search returned nothing...THAT'S WEIRD! This message can only appear if every single level on the workshop is somehow new! Something must be wrong...");
                            break;
                        }

                        removeFrom = RemoveLevelsStartingFromHere(results, AutoServer.Playlist);

                    }

                    try
                    {
                        results.RemoveRange(removeFrom, results.Count - removeFrom);
                        AutoServer.Playlist.AddRange(results);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Results list count: {results.Count}");
                        Log.Error("If you see this message, Tribow's math sucks! Go tell him his math sucks. Ping him multiple times. Bully that lad.");
                    }

                    //This will log the new levels added into the playlist
                    Log.Info("NEW LEVELS!");
                    LogLevels(AutoServer.Playlist, previousLevelAmountInPlaylist);

                    //Shuffle now that new levels got added and logged
                    AutoServer.Playlist.Shuffle();
                }
            }
            Server.SayChat(DistanceChat.Server("HellServer:maintenancedone", "Maintanence complete! [FF0000]HELL[-] grows deeper..."));
            updatingPlaylist = false;
            yield break;
        }

        /// <summary>
        /// Sets the DistanceSearchRetriever to whatever you need.
        /// </summary>
        /// <param name="searchParameters">The specific WorkshopSearchParameters you want.</param>
        /// <param name="startCoroutine">Whether or not you want to immediately start the search coroutine.</param>
        /// <returns></returns>
        private DistanceSearchRetriever SetReceiver(WorkshopSearchParameters searchParameters, bool startCoroutine)
        {
            DistanceSearchRetriever retriever = null;

            try
            {
                retriever = new DistanceSearchRetriever(new DistanceSearchParameters()
                {
                    GameMode = "Sprint",
                    Search = searchParameters,
                }, startCoroutine);
            }
            catch (Exception e)
            {
                Log.Error($"Error retrieving workshop level settings:\n{e}");
            }

            return retriever;
        }

        /// <summary>
        /// Checks if any level in the list of new levels are levels that already exist in the current playlist.
        /// </summary>
        /// <param name="results">The results list</param>
        /// <param name="playlist">The current AutoServer playlist</param>
        /// <returns></returns>
        private int RemoveLevelsStartingFromHere(List<DistanceLevel> results, List<DistanceLevel> playlist)
        {
            int removeFrom = results.Count;

            for (int i = 0; i < results.Count; i++)
            {
                foreach (DistanceLevel playlistLevel in playlist)
                {
                    if (results[i] == playlistLevel)
                    {
                        removeFrom = i;
                        return removeFrom;
                    }
                }
            }

            return removeFrom;
        }

        /// <summary>
        /// Logs each level in a given playlist to display that the Server is working as intended.
        /// </summary>
        /// <param name="playlist">The current AutoServer playlist</param>
        /// <param name="startfrom">Where you want the start logging in the playlist</param>
        private void LogLevels(List<DistanceLevel> playlist, int startfrom)
        {
            string listString = $"Levels ({playlist.Count - startfrom}):";
            for(int i = startfrom; i < playlist.Count; i++)
            {
                listString += $"\n{playlist[i].Name}";
            }
            Log.Info(listString);
        }

        /// <summary>
        /// Hell will load the next level as long as a player has beaten it, otherwise it will continue to wait.
        /// </summary>
        private void CheckIfHellCanLoadNextLevel()
        {
            int finishCount = 0;

            //Check if any player that finished actually beat the level. Count how many players are "finished" as well.
            foreach (DistancePlayer player in Server.ValidPlayers)
            {
                if (player.Car != null)
                {
                    if (player.Car.FinishType == Distance::FinishType.Normal)
                    {
                        playerFinished = true;
                    }

                    if (player.Car.Finished)
                    {
                        finishCount++;
                    }
                }
                else
                    finishCount++;
            }

            //If any player beat the level and the amount of players that finished are equal to valid players it can move on.
            if (playerFinished && finishCount >= Server.ValidPlayers.Count)
            {
                Server.SayChat(DistanceChat.Server("HellServer:finished", "All players finished. Advancing to the next layer of [FF0000]HELL[-] in 10 seconds."));
                DistanceServerMain.Instance.GetPlugin<BasicAutoServer.BasicAutoServer>().AdvanceLevel();
                levelsCompleted++;
                playerFinished = false;
            }
        }
    }

    /// <summary>
    /// Literally just exists to shuffle lists better
    /// </summary>
    static class ListShuffler
    {
        /// <summary>
        /// Creates a new System.Random that will truly be a lot more random than just calling it normally
        /// </summary>
        /// <returns></returns>
        public static Random ThreadSafeRandom()
        {
            Random Local = new Random();
            return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
        }

        /// <summary>
        /// Shuffles the order of a list
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <param name="list">the list to be shuffled</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            Random rnd = ThreadSafeRandom();
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}