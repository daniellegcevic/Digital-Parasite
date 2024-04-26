using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class ReticleController : MonoBehaviour
{
    #region Variables

        #region Singleton

            public static ReticleController instance = null;

        #endregion

        #region Settings

            public Image reticle;
            public float transitionDuration;
            public Color activeReticle;
            public Color inactiveReticle;

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
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            transitionCurve = LevelManager.instance.transitionCurve;
        }

        private void Update()
        {
            reticle.transform.position = Input.mousePosition;
        }

    #endregion

    #region Custom Methods

        public void Activate()
        {
            Cursor.lockState = CursorLockMode.None;
            StartCoroutine(ActivateReticle());
        }

        public void Deactivate()
        {
            StartCoroutine(DeactivateReticle());
        }

    #endregion

    #region Coroutines

        private IEnumerator ActivateReticle()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);
                
                reticle.color = Color.Lerp(inactiveReticle, activeReticle, smoothTransitionPercentage);

                yield return null;
            }
        }

        private IEnumerator DeactivateReticle()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);
                
                reticle.color = Color.Lerp(activeReticle, inactiveReticle, smoothTransitionPercentage);

                yield return null;
            }
        }

    #endregion
}