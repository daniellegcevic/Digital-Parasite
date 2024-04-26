using System.Collections;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    #region Variables
    
        #region Singleton

            public static FadeController instance = null;

        #endregion
        
        #region Inspector
        
            public Material activeFade;
            public Material inactiveFade;

            public float fadeDuration;
        
        #endregion
    
        #region DEBUG
        
            private AnimationCurve fadeCurve;
        
        #endregion
    
        #region Components
        
            private MeshRenderer meshRenderer;
        
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
            fadeCurve = LevelManager.instance.transitionCurve;

            meshRenderer = GetComponent<MeshRenderer>();
        }
    
    #endregion
    
    #region Custom Methods
    
        public void FadeOut()
        {
            StartCoroutine(Fade(true));
        }

        public void FadeIn()
        {
            StartCoroutine(Fade(false));
        }
    
    #endregion
    
    #region Coroutines

        private IEnumerator Fade(bool fadeOut)
        {
            float fadePercentage = 0f;
            float smoothFadePercentage = 0f;
            float fadeSpeed = 1f / fadeDuration;

            while(fadePercentage < 1f)
            {
                fadePercentage += Time.deltaTime * fadeSpeed;
                smoothFadePercentage = fadeCurve.Evaluate(fadePercentage);

                if(fadeOut)
                {
                    meshRenderer.material.color = Color.Lerp(inactiveFade.color, activeFade.color, smoothFadePercentage);
                }
                else
                {
                    meshRenderer.material.color = Color.Lerp(activeFade.color, inactiveFade.color, smoothFadePercentage);
                }

                yield return null;
            }

            if(fadeOut)
            {
                LevelManager.instance.NextPuzzle();
            }
            else
            {
                InteractionSystem.instance.enableInteraction = true;
                PauseMenu.instance.disableEscape = false;
                PauseMenu.instance.transition = false;
            }
        }
    
    #endregion
}