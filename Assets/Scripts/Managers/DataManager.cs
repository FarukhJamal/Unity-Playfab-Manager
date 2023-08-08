using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Models;
using Newtonsoft.Json;

namespace Managers
{
    public static class DataManager
    {
        public static GameData GameData;
        public static MetaData MetaData;

        public static void BindEvent()
        {
            
        }

        public static List<PlayerData> GetAllPlayers()
        {
            return GameData.AllPlayers;
        }

        public static MetaData GetMetaData()
        {
            return MetaData;
        }

        public static void SetGameData(string data)
        {
            // change it according to your need of the game data
            if(string.IsNullOrEmpty(data))
                return;
            GameData = JsonConvert.DeserializeObject<GameData>(data);
        }

        public static void PopulateMetaData(string metadata)
        {
            // fill it according to your need of metadata
            if(string.IsNullOrEmpty(metadata))
                return;
            MetaData = JsonConvert.DeserializeObject<MetaData>(metadata);
        }
    }
}