using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class Timer : MonoBehaviour
{
    #region Variables
    
        #region Singleton

            public static Timer instance = null;

        #endregion
        
        #region Inspector
        
            public TMPro.TMP_Text time;
            public float transitionDuration;
            public Color activeColor;
            public Color inactiveColor;
            public TMPro.TMP_Text endScreenTime;
        
        #endregion
    
        #region DEBUG
        
            private AnimationCurve transitionCurve;

            [HideInInspector] public float timeSpent = 0;
            [HideInInspector] public float finalTimeSpent = 0;
            [HideInInspector] public int minutes;
            [HideInInspector] public int seconds;
            [HideInInspector] public int milliseconds;

            private bool stopTimer = false;
            [HideInInspector] public bool pauseTimer = false;

            private bool updateTimer = false;
        
        #endregion
    
    #endregion
    
    #region Built-in Methods
    
        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            transitionCurve = LevelManager.instance.transitionCurve;

            minutes = LevelManager.instance.minutes;
            seconds = LevelManager.instance.seconds;
            milliseconds = LevelManager.instance.milliseconds;
        }

        private void Update()
        {
            if(updateTimer)
            {
                if(!stopTimer)
                {
                    timeSpent += Time.deltaTime;

                    minutes = Mathf.FloorToInt(timeSpent / 60);
                    seconds = Mathf.FloorToInt(timeSpent % 60);
                    milliseconds = Mathf.FloorToInt((timeSpent * 100) % 60);

                    time.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                }
            }
        }
    
    #endregion
    
    #region Custom Methods
    
        public void Activate()
        {
            StartCoroutine(ActivateTimer());
            updateTimer = true;
        }

        public void Deactivate()
        {
            StartCoroutine(DeactivateTimer());
        }

        public void StopTimer()
        {
            stopTimer = true;
        }

        public void GetTime()
        {
            if(LevelManager.instance.currentLevel == 1 && timeSpent < GameManager.instance.level1BestTime)
            {
                endScreenTime.text = "NEW BEST TIME: " + string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
            }
            else if(LevelManager.instance.currentLevel == 2 && timeSpent < GameManager.instance.level2BestTime)
            {
                endScreenTime.text = "NEW BEST TIME: " + string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
            }
            else if(LevelManager.instance.currentLevel == 3 && timeSpent < GameManager.instance.level3BestTime)
            {
                endScreenTime.text = "NEW BEST TIME: " + string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
            }
            else
            {
                endScreenTime.text = "TIME: " + string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
            }

            finalTimeSpent = timeSpent;
        }
    
    #endregion
    
    #region Coroutines
    
        private IEnumerator ActivateTimer()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);
                
                time.color = Color.Lerp(inactiveColor, activeColor, smoothTransitionPercentage);

                yield return null;
            }
        }

        private IEnumerator DeactivateTimer()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);
                
                time.color = Color.Lerp(activeColor, inactiveColor, smoothTransitionPercentage);

                yield return null;
            }
        }

        private IEnumerator UpdateTimer()
        {
            while(!stopTimer)
            {
                yield return new WaitForSeconds(1f);

                if(!stopTimer)
                {
                    timeSpent++;

                    minutes = Mathf.FloorToInt(timeSpent / 60);
                    seconds = Mathf.FloorToInt(timeSpent % 60);
                    milliseconds = Mathf.FloorToInt(timeSpent % 60);

                    time.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                }
            }
        }
    
    #endregion
}