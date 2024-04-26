using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    #region Variables

        #region Singleton

            public static MainMenu instance = null;

        #endregion

        #region Settings

            public int currentMenu = 1;
            public float menuTransitionDuration = 1f;
            public float buttonFadeDuration = 0.5f;
            public AnimationCurve buttonFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            public Color activeButtonColor;
            public Color inactiveButtonColor;
            public Color activeSelectedButtonColor;
            public Color inactiveSelectedButtonColor;
            public Color activeTextColor;
            public Color inactiveTextColor;
            public Color activeImageColor;
            public Color inactiveImageColor;
            public Color activeBestTimeColor;
            public Color inactiveBestTimeColor;
            public bool fadeText = false;

        #endregion

        #region References

            [Header("Main Menu")]
            public GameObject continueButton;
            public GameObject newGameButton;
            public GameObject levelSelectButton;
            public GameObject creditsButton;
            public GameObject exitButton;
            public GameObject logo;

            [Header("Level Select")]
            public GameObject level1Button;
            public GameObject level2Button;
            public GameObject level3Button;
            public GameObject backButton;

        #endregion

        #region DEBUG

            private Image[] mainMenuButtons;
            private Image[] levelSelectButtons;

            private TMP_Text[] mainMenuText;
            private TMP_Text[] levelSelectText;

            private Image[] levelSelectImages;
            private TMP_Text[] levelSelectBestTimeText;

            private Image logoImage;

            private bool levelSelectButtonActive = false;
            private bool creditsButtonActive = false;
            private bool backButtonActive = false;

            private bool level1BestTimeLoaded = false;
            private bool level2BestTimeLoaded = false;
            private bool level3BestTimeLoaded = false;

            [Header("DEBUG")]
            public bool disableEscape = true;
            public bool levelSelectActive = false;
            public bool levelSelectBackActive = false;
            public bool creditsActive = false;
            public bool creditsBackActive = false;
            public bool mainMenuActive = false;
            public bool backToMainMenu = false;

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

            mainMenuButtons = new Image[5];
            mainMenuButtons[0] = continueButton.GetComponent<Image>();
            mainMenuButtons[1] = newGameButton.GetComponent<Image>();
            mainMenuButtons[2] = levelSelectButton.GetComponent<Image>();
            mainMenuButtons[3] = creditsButton.GetComponent<Image>();
            mainMenuButtons[4] = exitButton.GetComponent<Image>();

            levelSelectButtons = new Image[4];
            levelSelectButtons[0] = level1Button.GetComponent<Image>();
            levelSelectButtons[1] = level2Button.GetComponent<Image>();
            levelSelectButtons[2] = level3Button.GetComponent<Image>();
            levelSelectButtons[3] = backButton.GetComponent<Image>();

            mainMenuText = new TMP_Text[5];
            mainMenuText[0] = continueButton.transform.GetChild(2).GetComponent<TMP_Text>();
            mainMenuText[1] = newGameButton.transform.GetChild(2).GetComponent<TMP_Text>();
            mainMenuText[2] = levelSelectButton.transform.GetChild(2).GetComponent<TMP_Text>();
            mainMenuText[3] = creditsButton.transform.GetChild(2).GetComponent<TMP_Text>();
            mainMenuText[4] = exitButton.transform.GetChild(2).GetComponent<TMP_Text>();

            levelSelectText = new TMP_Text[4];
            levelSelectText[0] = level1Button.transform.GetChild(2).GetComponent<TMP_Text>();
            levelSelectText[1] = level2Button.transform.GetChild(2).GetComponent<TMP_Text>();
            levelSelectText[2] = level3Button.transform.GetChild(2).GetComponent<TMP_Text>();
            levelSelectText[3] = backButton.transform.GetChild(2).GetComponent<TMP_Text>();

            levelSelectImages = new Image[3];
            levelSelectImages[0] = level1Button.transform.GetChild(3).GetComponent<Image>();
            levelSelectImages[1] = level2Button.transform.GetChild(3).GetComponent<Image>();
            levelSelectImages[2] = level3Button.transform.GetChild(3).GetComponent<Image>();

            levelSelectBestTimeText = new TMP_Text[3];
            levelSelectBestTimeText[0] = level1Button.transform.GetChild(4).GetComponent<TMP_Text>();
            levelSelectBestTimeText[1] = level2Button.transform.GetChild(4).GetComponent<TMP_Text>();
            levelSelectBestTimeText[2] = level3Button.transform.GetChild(4).GetComponent<TMP_Text>();

            logoImage = logo.GetComponent<Image>();

            GetBestTimes();
        }

        private void Update()
        {
            if(!disableEscape)
            {
                if(InputHandler.instance.escapeClicked)
                {
                    Quit();
                }
            }
            else if(levelSelectBackActive)
            {
                if(InputHandler.instance.escapeClicked)
                {
                    levelSelectBackActive = false;
                    Back();
                    SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                }
            }
            else if(creditsBackActive && !TextManager.instance.exitCredits)
            {
                if(InputHandler.instance.escapeClicked)
                {
                    creditsBackActive = false;
                    TextManager.instance.rollCredits = false;
                    TextManager.instance.FadeOutCredits();
                    SoundEffectManager.instance.PlaySoundEffect("Exit Credits", gameObject);
                }
            }
        }

    #endregion

    #region Custom Methods

        public void GetBestTimes()
        {
            if(GameManager.instance.level1BestTime > 0)
            {
                levelSelectBestTimeText[0].text = "BEST TIME" + "\n" + string.Format("{0:00}:{1:00}.{2:00}", Mathf.FloorToInt(GameManager.instance.level1BestTime / 60), Mathf.FloorToInt(GameManager.instance.level1BestTime % 60), Mathf.FloorToInt((GameManager.instance.level1BestTime * 100) % 60));
                level1BestTimeLoaded = true;
            }

            if(GameManager.instance.level2BestTime > 0)
            {
                levelSelectBestTimeText[1].text = "BEST TIME" + "\n" + string.Format("{0:00}:{1:00}.{2:00}", Mathf.FloorToInt(GameManager.instance.level2BestTime / 60), Mathf.FloorToInt(GameManager.instance.level2BestTime % 60), Mathf.FloorToInt((GameManager.instance.level2BestTime * 100) % 60));
                level2BestTimeLoaded = true;
            }

            if(GameManager.instance.level3BestTime > 0)
            {
                levelSelectBestTimeText[2].text = "BEST TIME" + "\n" + string.Format("{0:00}:{1:00}.{2:00}", Mathf.FloorToInt(GameManager.instance.level3BestTime / 60), Mathf.FloorToInt(GameManager.instance.level3BestTime % 60), Mathf.FloorToInt((GameManager.instance.level3BestTime * 100) % 60));
                level3BestTimeLoaded = true;
            }
        }
        
        public void Continue()
        {
            // EnableButtons(false);
            // GameManager.instance.LoadGameWorld(false, GameManager.instance.savedScene);
        }

        public void NewGame()
        {
            disableEscape = true;
            // GameManager.instance.savedScene = SceneLoader.Scene.Level1;
            EnableButtons(false);
            GameManager.instance.LoadGameWorld(true, SceneLoader.Scene.Level1);
        }

        public void LevelSelect()
        {
            mainMenuActive = false;
            levelSelectActive = true;
            disableEscape = true;
            EnableButtons(false);
            FadeButtons(true);
            SelectionController.instance.FadeOutMenuSelection();
        }

        public void Credits()
        {
            mainMenuActive = false;
            creditsActive = true;
            disableEscape = true;
            EnableButtons(false);
            FadeButtons(true);
            SelectionController.instance.FadeOutMenuSelection();
        }

        public void ExitCredits()
        {
            creditsActive = false;
            creditsBackActive = false;
            StartCoroutine(ExitCreditsCoroutine());
        }

        public void Quit()
        {
            EnableButtons(false);
            Application.Quit();
        }

        public void Back()
        {
            levelSelectActive = false;
            backToMainMenu = true;
            EnableButtons(false);
            FadeButtons(true);
            SelectionController.instance.FadeOutMenuSelection();
        }

        public void Level1()
        {
            NewGame();
        }

        public void Level2()
        {
            // GameManager.instance.savedScene = SceneLoader.Scene.Level2;
            EnableButtons(false);
            GameManager.instance.LoadGameWorld(true, SceneLoader.Scene.Level2);
        }

        public void Level3()
        {
            // GameManager.instance.savedScene = SceneLoader.Scene.Level3;
            EnableButtons(false);
            GameManager.instance.LoadGameWorld(true, SceneLoader.Scene.Level3);
        }

        public void EnableButtons(bool enableButtons)
        {
            if(enableButtons)
            {
                InteractionSystem.instance.enableInteraction = true;
            }
            else
            {
                InteractionSystem.instance.enableInteraction = false;
            }
        }

        public void FadeButtons(bool fadeOut)
        {
            StartCoroutine(FadeButtonsCoroutine(fadeOut));
        }

    #endregion

    #region Coroutines

        public IEnumerator ExitCreditsCoroutine()
        {
            CameraController.instance.ExitCredits();

            yield return new WaitForSeconds(menuTransitionDuration);

            SelectionController.instance.SetupMainMenuSelection();
            backButtonActive = true;
            FadeButtons(false);
            SelectionController.instance.FadeInMenuSelection();
        }
        
        public IEnumerator FadeButtonsCoroutine(bool fadeOut)
        {
            if(SelectionController.instance.currentMenuSelection == mainMenuButtons[2].gameObject)
            {
                levelSelectButtonActive = true;
            }
            else if(SelectionController.instance.currentMenuSelection == mainMenuButtons[3].gameObject)
            {
                creditsButtonActive = true;
            }
            else if(SelectionController.instance.currentMenuSelection == levelSelectButtons[3].gameObject)
            {
                backButtonActive = true;
            }

            if(!fadeOut && !fadeText)
            {
                if(currentMenu == 1)
                {
                    mainMenuText[0].color = activeTextColor;
                    mainMenuText[1].color = activeTextColor;
                    mainMenuText[2].color = activeTextColor;
                    mainMenuText[3].color = activeTextColor;
                    mainMenuText[4].color = activeTextColor;
                }
                else if(currentMenu == 2)
                {
                    levelSelectText[0].color = activeTextColor;
                    levelSelectText[1].color = activeTextColor;
                    levelSelectText[2].color = activeTextColor;
                    levelSelectText[3].color = activeTextColor;
                }
            }

            float buttonFadePercentage = 0f;
            float smoothButtonFadePercentage = 0f;
            float buttonFadeSpeed = 1f / buttonFadeDuration;

            while(buttonFadePercentage < 1f)
            {
                buttonFadePercentage += Time.deltaTime * buttonFadeSpeed;
                smoothButtonFadePercentage = buttonFadeCurve.Evaluate(buttonFadePercentage);
                
                if(currentMenu == 1)
                {
                    if(fadeOut)
                    {
                        logoImage.color = Color.Lerp(activeImageColor, inactiveImageColor, smoothButtonFadePercentage);

                        mainMenuButtons[0].color = Color.Lerp(activeButtonColor, inactiveButtonColor, smoothButtonFadePercentage);
                        mainMenuButtons[1].color = Color.Lerp(activeButtonColor, inactiveButtonColor, smoothButtonFadePercentage);
                        
                        if(levelSelectButtonActive)
                        {
                            mainMenuButtons[2].color = Color.Lerp(activeSelectedButtonColor, inactiveSelectedButtonColor, smoothButtonFadePercentage);
                        }
                        else
                        {
                            mainMenuButtons[2].color = Color.Lerp(activeButtonColor, inactiveButtonColor, smoothButtonFadePercentage);
                        }

                        if(creditsButtonActive)
                        {
                            mainMenuButtons[3].color = Color.Lerp(activeSelectedButtonColor, inactiveSelectedButtonColor, smoothButtonFadePercentage);
                        }
                        else
                        {
                            mainMenuButtons[3].color = Color.Lerp(activeButtonColor, inactiveButtonColor, smoothButtonFadePercentage);
                        }
                        
                        mainMenuButtons[4].color = Color.Lerp(activeButtonColor, inactiveButtonColor, smoothButtonFadePercentage);

                        if(fadeText)
                        {
                            mainMenuText[0].color = Color.Lerp(activeTextColor, inactiveTextColor, smoothButtonFadePercentage);
                            mainMenuText[1].color = Color.Lerp(activeTextColor, inactiveTextColor, smoothButtonFadePercentage);
                            mainMenuText[2].color = Color.Lerp(activeTextColor, inactiveTextColor, smoothButtonFadePercentage);
                            mainMenuText[3].color = Color.Lerp(activeTextColor, inactiveTextColor, smoothButtonFadePercentage);
                            mainMenuText[4].color = Color.Lerp(activeTextColor, inactiveTextColor, smoothButtonFadePercentage);
                        }
                    }
                    else
                    {
                        logoImage.color = Color.Lerp(inactiveImageColor, activeImageColor, smoothButtonFadePercentage);

                        mainMenuButtons[0].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);
                        
                        if(backButtonActive)
                        {
                            mainMenuButtons[1].color = Color.Lerp(inactiveSelectedButtonColor, activeSelectedButtonColor, smoothButtonFadePercentage);
                        }
                        else
                        {
                            mainMenuButtons[1].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);
                        }

                        mainMenuButtons[2].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);
                        mainMenuButtons[3].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);
                        mainMenuButtons[4].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);

                        if(fadeText)
                        {
                            mainMenuText[0].color = Color.Lerp(inactiveTextColor, activeTextColor, smoothButtonFadePercentage);
                            mainMenuText[1].color = Color.Lerp(inactiveTextColor, activeTextColor, smoothButtonFadePercentage);
                            mainMenuText[2].color = Color.Lerp(inactiveTextColor, activeTextColor, smoothButtonFadePercentage);
                            mainMenuText[3].color = Color.Lerp(inactiveTextColor, activeTextColor, smoothButtonFadePercentage);
                            mainMenuText[4].color = Color.Lerp(inactiveTextColor, activeTextColor, smoothButtonFadePercentage);
                        }
                    }
                }
                else if(currentMenu == 2)
                {
                    if(fadeOut)
                    {
                        levelSelectButtons[0].color = Color.Lerp(activeButtonColor, inactiveButtonColor, smoothButtonFadePercentage);
                        levelSelectButtons[1].color = Color.Lerp(activeButtonColor, inactiveButtonColor, smoothButtonFadePercentage);
                        levelSelectButtons[2].color = Color.Lerp(activeButtonColor, inactiveButtonColor, smoothButtonFadePercentage);

                        levelSelectImages[0].color = Color.Lerp(activeImageColor, inactiveImageColor, smoothButtonFadePercentage);
                        levelSelectImages[1].color = Color.Lerp(activeImageColor, inactiveImageColor, smoothButtonFadePercentage);
                        levelSelectImages[2].color = Color.Lerp(activeImageColor, inactiveImageColor, smoothButtonFadePercentage);

                        if(level1BestTimeLoaded)
                        {
                            levelSelectBestTimeText[0].color = Color.Lerp(activeBestTimeColor, inactiveBestTimeColor, smoothButtonFadePercentage);
                        }
                        
                        if(level2BestTimeLoaded)
                        {
                            levelSelectBestTimeText[1].color = Color.Lerp(activeBestTimeColor, inactiveBestTimeColor, smoothButtonFadePercentage);
                        }
                        
                        if(level3BestTimeLoaded)
                        {
                            levelSelectBestTimeText[2].color = Color.Lerp(activeBestTimeColor, inactiveBestTimeColor, smoothButtonFadePercentage);
                        }
                        
                        if(backButtonActive)
                        {
                            levelSelectButtons[3].color = Color.Lerp(activeSelectedButtonColor, inactiveSelectedButtonColor, smoothButtonFadePercentage);
                        }
                        else
                        {
                            levelSelectButtons[3].color = Color.Lerp(activeButtonColor, inactiveButtonColor, smoothButtonFadePercentage);
                        }

                        if(fadeText)
                        {
                            levelSelectText[0].color = Color.Lerp(activeTextColor, inactiveTextColor, smoothButtonFadePercentage);
                            levelSelectText[1].color = Color.Lerp(activeTextColor, inactiveTextColor, smoothButtonFadePercentage);
                            levelSelectText[2].color = Color.Lerp(activeTextColor, inactiveTextColor, smoothButtonFadePercentage);
                            levelSelectText[3].color = Color.Lerp(activeTextColor, inactiveTextColor, smoothButtonFadePercentage);
                        }
                    }
                    else
                    {
                        if(levelSelectButtonActive)
                        {
                            levelSelectButtons[0].color = Color.Lerp(inactiveSelectedButtonColor, activeSelectedButtonColor, smoothButtonFadePercentage);
                        }
                        else
                        {
                            levelSelectButtons[0].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);
                        }
                        
                        levelSelectButtons[1].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);
                        levelSelectButtons[2].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);
                        levelSelectButtons[3].color = Color.Lerp(inactiveButtonColor, activeButtonColor, smoothButtonFadePercentage);

                        levelSelectImages[0].color = Color.Lerp(inactiveImageColor, activeImageColor, smoothButtonFadePercentage);
                        levelSelectImages[1].color = Color.Lerp(inactiveImageColor, activeImageColor, smoothButtonFadePercentage);
                        levelSelectImages[2].color = Color.Lerp(inactiveImageColor, activeImageColor, smoothButtonFadePercentage);

                        if(level1BestTimeLoaded)
                        {
                            levelSelectBestTimeText[0].color = Color.Lerp(inactiveBestTimeColor, activeBestTimeColor, smoothButtonFadePercentage);
                        }

                        if(level2BestTimeLoaded)
                        {
                            levelSelectBestTimeText[1].color = Color.Lerp(inactiveBestTimeColor, activeBestTimeColor, smoothButtonFadePercentage);
                        }

                        if(level3BestTimeLoaded)
                        {
                            levelSelectBestTimeText[2].color = Color.Lerp(inactiveBestTimeColor, activeBestTimeColor, smoothButtonFadePercentage);
                        }

                        if(fadeText)
                        {
                            levelSelectText[0].color = Color.Lerp(inactiveTextColor, activeTextColor, smoothButtonFadePercentage);
                            levelSelectText[1].color = Color.Lerp(inactiveTextColor, activeTextColor, smoothButtonFadePercentage);
                            levelSelectText[2].color = Color.Lerp(inactiveTextColor, activeTextColor, smoothButtonFadePercentage);
                            levelSelectText[3].color = Color.Lerp(inactiveTextColor, activeTextColor, smoothButtonFadePercentage);
                        }
                    }
                }

                yield return null;
            }

            if(!creditsButtonActive)
            {
                if(fadeOut)
                {
                    if(currentMenu == 1)
                    {
                        if(!fadeText)
                        {
                            mainMenuText[0].color = inactiveTextColor;
                            mainMenuText[1].color = inactiveTextColor;
                            mainMenuText[2].color = inactiveTextColor;
                            mainMenuText[3].color = inactiveTextColor;
                            mainMenuText[4].color = inactiveTextColor;
                        }

                        currentMenu = 2;
                        SelectionController.instance.SetupLevelSelectSelection();
                        CameraController.instance.MoveToLevelSelect();
                    }
                    else if(currentMenu == 2)
                    {
                        if(!fadeText)
                        {
                            levelSelectText[0].color = inactiveTextColor;
                            levelSelectText[1].color = inactiveTextColor;
                            levelSelectText[2].color = inactiveTextColor;
                            levelSelectText[3].color = inactiveTextColor;
                        }

                        currentMenu = 1;
                        SelectionController.instance.SetupMainMenuSelection();
                        CameraController.instance.ExitLevelSelect();
                    }

                    yield return new WaitForSeconds(menuTransitionDuration);

                    FadeButtons(false);
                    SelectionController.instance.FadeInMenuSelection();
                }
                else
                {
                    if(currentMenu == 1)
                    {
                        backButtonActive = false;
                    }
                    else if(currentMenu == 2)
                    {
                        levelSelectButtonActive = false;
                    }

                    EnableButtons(true);
                }
            }
            else
            {
                if(!fadeText)
                {
                    mainMenuText[0].color = inactiveTextColor;
                    mainMenuText[1].color = inactiveTextColor;
                    mainMenuText[2].color = inactiveTextColor;
                    mainMenuText[3].color = inactiveTextColor;
                    mainMenuText[4].color = inactiveTextColor;
                }

                CameraController.instance.MoveToCredits();

                yield return new WaitForSeconds(menuTransitionDuration);

                TextManager.instance.RollCredits();
                creditsButtonActive = false;
            }
        }

    #endregion
}