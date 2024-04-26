using Cinemachine;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    #region Variables
    
        #region Singleton

            public static LevelManager instance = null;

        #endregion
        
        #region Inspector
        
            [Header("Settings")]
            public LevelType levelType;
            public int currentLevel = 1;
            public int currentPuzzle = 1;

            public enum LevelType
            {
                Standard,
                Timed
            }

            public SceneLoader.Scene nextScene;
            public float endingDelay;
            public float endScreenDelay;

            [Header("Standard Level")]
            public float puzzleDelay;
            public GameObject[] puzzles;
            public GameObject[] spawnPoints;

            [Header("Timed Level")]
            public int minutes;
            public int seconds;
            public int milliseconds;

            [Header("Adaptive Input Modes")]
            public CinemachineVirtualCamera cameraB;
            public float mouseDeadZoneWidth;
            public float mouseDeadZoneHeight;
            public float mouseScreenY;
            public float keyboardDeadZoneWidth;
            public float keyboardDeadZoneHeight;
            public float keyboardScreenY;
            
            [Header("Nodes")]
            public float rotationDuration = 1f;
            public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            public Material unlockedNode;
            public Material lockedNode;

            [Header("Connections")]
            public Material activeConnection;
            public Material semiActiveConnection;
            public Material inactiveConnection;
            public float transitionDuration;
            public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            [Header("Sources")]
            public Source[] allSources;

            [Header("Indicators")]
            public Material activeIndicator;
            public Material inactiveIndicator;
            public Material noConnection;

            [Header("UI")]
            public Color activeColor;
            public Color inactiveColor;
        
        #endregion

        #region DEBUG

            [Header("DEBUG")]
            public bool startLevel = false;
            public bool feedbackLoop = false;
            public bool feedbackLoopWrongPath = false;
            public Node rotatingNode;
            public Source[] sources;

            private CinemachineFramingTransposer transposer;

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

            foreach(Source source in sources)
            {
                source.SetupAllConnections();
            }
        }

        public void Start()
        {
            InputHandler.instance.enableInput = false;
            transposer = cameraB.GetCinemachineComponent<CinemachineFramingTransposer>();
            SetInputMode(GameManager.instance.inputMode);
        }
    
    #endregion

    #region Custom Methods

        public void StartLevel()
        {
            startLevel = true;
            
            Debug.Log("GameManager.instance.inputMode = " + GameManager.instance.inputMode);

            if(GameManager.instance.inputMode == 1)
            {
                ReticleController.instance.Activate();
                ArrowController.instance.Activate();
            }
            else if(GameManager.instance.inputMode == 2)
            {
                SelectionController.instance.SetupSelectionData(true);
            }

            if(levelType == LevelType.Standard)
            {
                GatewayCounter.instance.Activate();
                Timer.instance.Activate();
            }
            else
            {
                Timer.instance.Activate();
            }
            
            InputHandler.instance.enableInput = true;
            InteractionSystem.instance.enableInteraction = true;
        }

        public void EndLevel(bool complete)
        {
            if(GameManager.instance.inputMode == 1)
            {
                ReticleController.instance.Deactivate();
                ArrowController.instance.Deactivate();
            }
            else if(GameManager.instance.inputMode == 2)
            {
                //
            }

            if(levelType == LevelType.Standard)
            {
                GatewayCounter.instance.Deactivate();
                Timer.instance.Deactivate();
            }
            else
            {
                Timer.instance.Deactivate();
            }

            if(complete)
            {
                CameraController.instance.EndLevel();

                StartCoroutine(ActivateEndScreen());
            }
        }

        public void NextLevel()
        {
            if(nextScene == SceneLoader.Scene.MainMenu)
            {
                GameManager.instance.LoadMainMenu();
            }
            else
            {
                GameManager.instance.levelTransition = true;
                GameManager.instance.LoadGameWorld(false, nextScene);
            }
        }

        public void NextPuzzle()
        {
            if(puzzles[currentPuzzle - 1])
            {
                puzzles[currentPuzzle - 1].SetActive(false);
                puzzles[currentPuzzle].SetActive(true);
                puzzles[currentPuzzle].transform.position = spawnPoints[currentPuzzle - 1].transform.position;

                currentPuzzle++;

                SelectionController.instance.SetupSelectionData(true);
                FadeController.instance.FadeIn();
            }
        }

        public void SetInputMode(int inputMode)
        {
            if(inputMode == 1)
            {
                transposer.m_DeadZoneWidth = mouseDeadZoneWidth;
                transposer.m_DeadZoneHeight = mouseDeadZoneHeight;
                transposer.m_ScreenY = mouseScreenY;
            }
            else if(inputMode == 2)
            {
                transposer.m_DeadZoneWidth = keyboardDeadZoneWidth;
                transposer.m_DeadZoneHeight = keyboardDeadZoneHeight;
                transposer.m_ScreenY = keyboardScreenY;
            }
        }

    #endregion

    #region Coroutines

        private IEnumerator ActivateEndScreen()
        {
            yield return new WaitForSeconds(endScreenDelay);

            GameManager.instance.SaveGame();

            PauseMenu.instance.FadeEndScreenButtons();
            SelectionController.instance.SetupEndScreenSelection();
            SelectionController.instance.FadeInMenuSelection();
        }

    #endregion
}