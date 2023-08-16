# Hell Server
 A plugin for <a href=https://github.com/Corecii>Corecii's</a> <a href=https://github.com/Corecii/Distance-Server>Distance-Server</a> tool.
 
 This simulates the Hell Server experience in auto server form! HELL SERVER is a server where any Sprint level on the workshop can be randomly selected play. The server will not skip to the next level unless someone is able to beat the level or 24 hours pass. Once everyone is done playing the level the server will choose the next level.

# Setup
If you're already familiar with the setup of the Distance-Server then that's great! If you don't then <a href=https://github.com/Corecii/Distance-Server>you really should go here before you do anything with this plugin.</a> 

Download the latest zip file in the releases page. <br />
The `HellServer.dll` should be moved into the `Plugins` folder within your `DistanceServer` directory. <br />
Add the `HellServer.dll` directory to the `Server.json` located in the `config` folder. <br />
The `0WorkshopSearch.dll` should be moved in the same place, replace the one that is there with this one. <br />
`BasicAutoServer.json` should be moved into the `config` folder within your `DistanceServer` directory. Replace the json that is in there already if needed. You will need to edit this file to your own preferences, but what is already written is preferred. There should not be anything in the `Workshop` section of the json. The HellServer plugin will find the levels on its own. If you don't understand what you're doing, see Corecii's <a href=https://github.com/Corecii/Distance-Server/blob/master/PLUGINS.md>PLUGINS.md</a> for configuration explanations.

In your `Server.json` in the `config` folder, you should remove the `VoteCommands.dll` directory.<br />
Why? <br />
Hell Server does not have voting! That ruins the spirit of Hell Server!<br />

That's it! Your Hell Server should be all set.

# Known Issues

