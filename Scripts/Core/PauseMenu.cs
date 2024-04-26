using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PauseMenu : MonoBehaviour
{
    #region Variables

        #region Singleton

            public static PauseMenu instance = null;

        #endregion

        #region References

            [Header("Settings")]
            public float buttonFadeDuration = 0.5f;
            public AnimationCurve buttonFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            public Color activeButtonColor;
            public Color inactiveButtonColor;
            public Color activeSelectedButtonColor;
            public Color inactiveSelectedButtonColor;
            public Color activeTextColor;
            public Color inactiveTextColor;
            public Color activeEndingTextColor;
            public Color inactiveEndingTextColor;
            public bool fadeText = false;
            public Volume postProcessingVolume;
            
            [Header("Pause Menu")]
            [SerializeField] private GameObject resumeButton;
            [SerializeField] private GameObject resetButton;
            [SerializeField] private GameObject quitButton;

            [Header("End Screen")]
            [SerializeField] private GameObject replayButton;
            [SerializeField] private GameObject continueButton;
            [SerializeField] private GameObject exitButton;

        #endregion

        #region DEBUG

            [HideInInspector] public bool gameIsPaused = false;
            [HideInInspector] public bool disableEscape = false;
            [HideInInspector] public bool transition = false;

            private Image[] endScreenButtons;
            private TMP_Text[] endScreenText;

            private DepthOfField depthOfFieldEffect;

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
            endScreenButtons = new Image[3];
            endScreenButtons[0] = replayButton.GetComponent<Image>();
            endScreenButtons[1] = continueButton.GetComponent<Image>();
            endScreenButtons[2] = exitButton.GetComponent<Image>();

            endScreenText = new TMP_Text[3];
            endScreenText[0] = replayButton.transform.GetChild(2).GetComponent<TMP_Text>();
            endScreenText[1] = continueButton.transform.GetChild(2).GetComponent<TMP_Text>();
            endScreenText[2] = exitButton.transform.GetChild(2).GetComponent<TMP_Text>();

            postProcessingVolume.profile.TryGet(out depthOfFieldEffect);
        }

        private void Update()
        {
            if(!disableEscape)
            {
                if(InputHandler.instance.escapeClicked)
                {
                    SoundEffectManager.instance.PlaySoundEffect("Pause Game", gameObject);

                    if(gameIsPaused)
                    {
                        ResumeGame();
                    }
                    else
                    {
                        PauseGame();
                    }
                }
            }
        }

    #endregion

    #region Custom Methods

        private void PauseGame()
        {
            depthOfFieldEffect.active = true;

            resumeButton.SetActive(true);
            resetButton.SetActive(true);
            quitButton.SetActive(true);

            gameIsPaused = true;
            SelectionController.instance.SetupPauseMenuSelection();
            Timer.instance.pauseTimer = true;
        }

        public void ResumeGame()
        {
            depthOfFieldEffect.active = false;

            resumeButton.SetActive(false);
            resetButton.SetActive(false);
            quitButton.SetActive(false);
            SelectionController.instance.RemoveMenuSelection();

            SelectionController.instance.SetupSelectionData(false);
            Timer.instance.pauseTimer = false;
            gameIsPaused = false;
        }

        public void RestartLevel()
        {
            DisableButtons();

            if(LevelManager.instance.currentLevel == 1)
            {
                GameManager.instance.LoadGameWorld(false, SceneLoader.Scene.Level1);
            }
            else if(LevelManager.instance.currentLevel == 2)
            {
                GameManager.instance.LoadGameWorld(false, SceneLoader.Scene.Level2);
            }
            else if(LevelManager.instance.currentLevel == 3)
            {
                GameManager.instance.LoadGameWorld(false, SceneLoader.Scene.Level3);
            }
            else if(LevelManager.instance.currentLevel == 4)
            {
                GameManager.instance.LoadGameWorld(false, SceneLoader.Scene.Level4);
            }
        }

        public void ExitToMainMenu()
        {
            InputHandler.instance.enableInput = false;
            DisableButtons();
            // GameManager.instance.enableContinue = true;
            GameManager.instance.LoadMainMenu();
        }

        private void DisableButtons()
        {
            InteractionSystem.instance.enableInteraction = false;
        }

        public void FadeEndScreenButtons()
        {
            StartCoroutine(FadeEndScreenButtonsCoroutine());
        }

    #endregion

    #region Coroutines

        public IEnumerator FadeEndScreenButtonsCoroutine()
        {
            if(!fadeText)
            {
                endScreenText[0].color = activeEndingTextColor;
                endScreenText[1].color = activeEndingTextColor;
                endScreenText[2].color = activeEndingTextColor;
            }

            float buttonFadePercentage = 0f;
            float smoothButtonFadePercentage = 0f;
            float buttonFadeSpeed = 1f / buttonFadeDuration;

            while(buttonFadePercentage < 1f)
            {
                buttonFadePercentage += Time.deltaTime * buttonFadeSpeed;
                smoothButtonFadePercentage = buttonFadeCurve.Evaluate(buttonFadePercentage);

                endScreenButtons[0].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);
                endScreenButtons[1].color = Color.Lerp(inactiveSelectedButtonColor, activeSelectedButtonColor, smoothButtonFadePercentage);
                endScreenButtons[2].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);

                if(fadeText)
                {
                    endScreenText[0].color = Color.Lerp(inactiveEndingTextColor, activeEndingTextColor, smoothButtonFadePercentage);
                    endScreenText[1].color = Color.Lerp(inactiveEndingTextColor, activeEndingTextColor, smoothButtonFadePercentage);
                    endScreenText[2].color = Color.Lerp(inactiveEndingTextColor, activeEndingTextColor, smoothButtonFadePercentage);
                }

                yield return null;
            }

            InteractionSystem.instance.enableInteraction = true;
        }

    #endregion
}