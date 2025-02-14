using UnityEngine;

namespace Data
{
    public class GameSettingEx
    {
        #region

        public static GameSettingEx Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameSettingEx();
                }

                return instance;
            }
        }
        private static GameSettingEx instance;

        #endregion

        public static int ServerIndex
        {
            get { return PlayerPrefs.GetInt("ServerIndex", 1); }
            set
            {
                PlayerPrefs.SetInt("ServerIndex", value);
                PlayerPrefs.Save();
            }
        }
    }
}