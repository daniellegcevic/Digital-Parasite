using System.Collections.Generic;

[System.Serializable]

public class GameData
{
    #region Variables

        #region Data

            public float level1BestTime; // LIST OF DATA TO SAVE
            public float level2BestTime;
            public float level3BestTime;

        #endregion

    #endregion

    #region Constructor

        public GameData(bool serializationFinished)
        {
            if(serializationFinished && Timer.instance)
            {
                if(LevelManager.instance.currentLevel == 1)
                {
                    if(GameManager.instance.level1BestTime > Timer.instance.finalTimeSpent || GameManager.instance.level1BestTime == 0)
                    {
                        level1BestTime = Timer.instance.finalTimeSpent;
                        GameManager.instance.level1BestTime = Timer.instance.finalTimeSpent;
                    }
                    else
                    {
                        level1BestTime = GameManager.instance.level1BestTime;
                    }

                    level2BestTime = GameManager.instance.level2BestTime;
                    level3BestTime = GameManager.instance.level3BestTime;
                }
                else if(LevelManager.instance.currentLevel == 2)
                {
                    if(GameManager.instance.level2BestTime > Timer.instance.finalTimeSpent || GameManager.instance.level2BestTime == 0)
                    {
                        level2BestTime = Timer.instance.finalTimeSpent;
                        GameManager.instance.level2BestTime = Timer.instance.finalTimeSpent;
                    }
                    else
                    {
                        level2BestTime = GameManager.instance.level2BestTime;
                    }

                    level1BestTime = GameManager.instance.level1BestTime;
                    level3BestTime = GameManager.instance.level3BestTime;
                }
                else if(LevelManager.instance.currentLevel == 3)
                {
                    if(GameManager.instance.level3BestTime > Timer.instance.finalTimeSpent || GameManager.instance.level3BestTime == 0)
                    {
                        level3BestTime = Timer.instance.finalTimeSpent;
                        GameManager.instance.level3BestTime = Timer.instance.finalTimeSpent;
                    }
                    else
                    {
                        level3BestTime = GameManager.instance.level3BestTime;
                    }

                    level1BestTime = GameManager.instance.level1BestTime;
                    level2BestTime = GameManager.instance.level2BestTime;
                }
            }
        }

    #endregion
}