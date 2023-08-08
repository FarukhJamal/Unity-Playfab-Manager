using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class PlayerData
    {
        public int playfabID;
        public int playerLevel;
        public string displayName;
        public List<Items> playerItems;

        public PlayerData()
        {
            playfabID = -1;
            playerLevel = -1;
            displayName = string.Empty;
            playerItems = new List<Items>();
        }
        public PlayerData(int playerID, int playerLevel, string displayName)
        {
            this.playfabID = playerID;
            this.playerLevel = playerLevel;
            this.displayName = displayName;
        }

        #region Getter-Setter
        
        public int GetPlayerID()
        {
            return playfabID;
        }

        public int GetPlayerLevel()
        {
            return playerLevel;
        }

        public string GetPlayerDisplayName()
        {
            return displayName;
        }

        public List<Items> GetPlayerItems()
        {
            return playerItems;
        }

        public void SetPlayerID(int id)
        {
            this.playfabID = id;
        }

        public void SetPlayerLevel(int level)
        {
            this.playerLevel = level;
        }

        public void SetPlayerName(string name)
        {
            this.displayName = name;
        }

        public void SetPlayerItems(List<Items> itemsList)
        {
            this.playerItems = itemsList;
        }
        #endregion
    }

    [Serializable] 
    public class Items
    {
        public int ID;
        public string name;
    }
}