using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ArrowController : MonoBehaviour
{
#region Variables
    
        #region Singleton

            public static ArrowController instance = null;

        #endregion
        
        #region Inspector
        
            public Image[] arrows;
            public float transitionDuration;
            public Color activeColor;
            public Color inactiveColor;

            [Header("Positions")]
            public GameObject topStartPosition;
            public GameObject topEndPosition;
            public GameObject rightStartPosition;
            public GameObject rightEndPosition;
            public GameObject bottomStartPosition;
            public GameObject bottomEndPosition;
            public GameObject leftStartPosition;
            public GameObject leftEndPosition;

        #endregion

        #region DEBUG

            private AnimationCurve transitionCurve;

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
        }
    
    #endregion
    
    #region Custom Methods
    
        public void Activate()
        {
            StartCoroutine(ActivateArrows());
        }

        public void Deactivate()
        {
            StartCoroutine(DeactivateArrows());
        }
    
    #endregion
    
    #region Coroutines

        private IEnumerator ActivateArrows()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);

                arrows[0].transform.position = Vector2.Lerp(topStartPosition.transform.position, topEndPosition.transform.position, smoothTransitionPercentage);
                arrows[1].transform.position = Vector2.Lerp(rightStartPosition.transform.position, rightEndPosition.transform.position, smoothTransitionPercentage);
                arrows[2].transform.position = Vector2.Lerp(bottomStartPosition.transform.position, bottomEndPosition.transform.position, smoothTransitionPercentage);
                arrows[3].transform.position = Vector2.Lerp(leftStartPosition.transform.position, leftEndPosition.transform.position, smoothTransitionPercentage);

                foreach(Image arrow in arrows)
                {
                    arrow.color = Color.Lerp(inactiveColor, activeColor, smoothTransitionPercentage);
                }

                yield return null;
            }
        }

        private IEnumerator DeactivateArrows()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);
                
                foreach(Image arrow in arrows)
                {
                    arrow.color = Color.Lerp(activeColor, inactiveColor, smoothTransitionPercentage);
                }

                yield return null;
            }
        }
    
    #endregion
}