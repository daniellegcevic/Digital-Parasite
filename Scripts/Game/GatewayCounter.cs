using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GatewayCounter : MonoBehaviour
{
    #region Variables
    
        #region Singleton

            public static GatewayCounter instance = null;

        #endregion
        
        #region Inspector
        
            public Image[] icons;
            public Image[] outlines;
            public float transitionDuration;

        #endregion

        #region DEBUG

            private int gatewaysAccessed = 0;
            private AnimationCurve transitionCurve;
            private Image counter;

            private Color activeColor;
            private Color inactiveColor;

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
            counter = GetComponent<Image>();

            activeColor = LevelManager.instance.activeColor;
            inactiveColor = LevelManager.instance.inactiveColor;
        }
    
    #endregion
    
    #region Custom Methods
    
        public void Activate()
        {
            StartCoroutine(ActivateCounter());
        }

        public void Deactivate()
        {
            StartCoroutine(DeactivateCounter());
        }
        
        public void AddGateway()
        {
            gatewaysAccessed++;

            StartCoroutine(ActivateIcon());
        }
    
    #endregion
    
    #region Coroutines
    
        private IEnumerator ActivateIcon()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);
                
                icons[gatewaysAccessed - 1].color = Color.Lerp(inactiveColor, activeColor, smoothTransitionPercentage);

                yield return null;
            }

            InteractionSystem.instance.enableInteraction = false;
            PauseMenu.instance.disableEscape = true;
            PauseMenu.instance.transition = true;

            if(gatewaysAccessed == icons.Length)
            {
                PauseMenu.instance.transition = false;

                Timer.instance.StopTimer();
                Timer.instance.GetTime();
                
                yield return new WaitForSeconds(LevelManager.instance.endingDelay);

                LevelManager.instance.EndLevel(true);
            }
            else
            {
                yield return new WaitForSeconds(LevelManager.instance.puzzleDelay);

                FadeController.instance.FadeOut();
            }
        }

        private IEnumerator ActivateCounter()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);
                
                counter.color = Color.Lerp(inactiveColor, activeColor, smoothTransitionPercentage);

                foreach(Image outline in outlines)
                {
                    outline.color = Color.Lerp(inactiveColor, activeColor, smoothTransitionPercentage);
                }

                yield return null;
            }
        }

        private IEnumerator DeactivateCounter()
        {
            foreach(Image outline in outlines)
            {
                outline.gameObject.SetActive(false);
            }

            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);
                
                counter.color = Color.Lerp(activeColor, inactiveColor, smoothTransitionPercentage);

                foreach(Image icon in icons)
                {
                    icon.color = Color.Lerp(activeColor, inactiveColor, smoothTransitionPercentage);
                }

                yield return null;
            }
        }
    
    #endregion
}