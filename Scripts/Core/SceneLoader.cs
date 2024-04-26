using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    #region Variables

        #region Singleton

            public static SceneLoader instance = null;

        #endregion
    
        #region Inspector

            [Tooltip("The amount of seconds it takes to fade in and out of scenes")]
            public float fadeLength = 1f;
            [Tooltip("The amount of seconds between each fade out and fade in")]
            public float transitionLength = 1f;
            [Tooltip("The minimum amount of seconds held on each loading screen")]
            public float minimumLoadingLength = 1f;

        #endregion
    
        #region DEBUG
        
            public enum Scene
            {
                MainMenu,
                Loading,
                Level1,
                Level2,
                Level3
            }

            [HideInInspector] public bool crossfadeComplete = false;
            [HideInInspector] public bool beginLoaderCallback = false;
        
        #endregion
    
        #region Components
        
            private Action onLoaderCallback;
            private AsyncOperation loadingAsyncOperation;
            private AsyncOperation unloadingAsyncOperation;
            private AsyncOperation setupAsyncOperation;
        
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
    
    #endregion
    
    #region Custom Methods
    
        public void Load(Scene scene)
        {
            onLoaderCallback = () =>
            {
                LoadScene(scene);
            };

            LoadScene(Scene.Loading);
        }

        public void LoaderCallback()
        {
            if(onLoaderCallback != null)
            {
                onLoaderCallback();
                onLoaderCallback = null;
            }
        }

        public void LoadScene(Scene scene)
        {
            StartCoroutine(LoadSceneAsync(scene));
        }

        public void UnloadScene(Scene scene)
        {
            StartCoroutine(UnloadSceneAsync(scene));
        }

        public void GameSetup()
        {
            StartCoroutine(LoadSetup());
        }

    #endregion
    
    #region Coroutines
    
        private IEnumerator LoadSceneAsync(Scene scene)
        {
            if(scene == Scene.Loading) // FADE OUT OF PREVIOUS SCENE
            {
                CrossfadeController.instance.FadeOut(fadeLength);

                while(!crossfadeComplete)
                {
                    yield return null;
                }

                crossfadeComplete = false;

                yield return new WaitForSeconds(transitionLength);
            }
            else
            {
                yield return new WaitForSeconds(minimumLoadingLength);
            }

            loadingAsyncOperation = SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Additive); // LOAD NEXT SCENE

            while(!loadingAsyncOperation.isDone)
            {
                yield return null;
            }

            if(scene == Scene.Loading) // FADE INTO LOADING SCENE
            {
                CrossfadeController.instance.FadeIn(fadeLength);

                while(!crossfadeComplete)
                {
                    yield return null;
                }

                crossfadeComplete = false;
            }
            else  // FADE OUT OF LOADING SCENE
            {
                if(scene == Scene.Level1 || scene == Scene.Level2 || scene == Scene.Level3 || scene == Scene.Level4) // LOAD GAME DATA
                {
                    GameManager.instance.LoadGame();

                    while(!GameManager.instance.dataLoaded)
                    {
                        yield return null;
                    }

                    GameManager.instance.dataLoaded = false;
                    GameManager.instance.levelTransition = false;
                }
                else if(scene == Scene.MainMenu) // LOAD MAIN MENU DATA AGAIN
                {
                    GameManager.instance.EnableContinue();
                    GameManager.instance.levelTransition = false;
                }

                GameManager.instance.currentScene = scene;

                CrossfadeController.instance.FadeOut(fadeLength);

                while(!crossfadeComplete)
                {
                    yield return null;
                }

                crossfadeComplete = false;

                yield return new WaitForSeconds(transitionLength);
            }

            if(scene == Scene.Loading)
            {
                beginLoaderCallback = true;
            }
            else
            {
                UnloadScene(Scene.Loading);
            }
        }

        private IEnumerator UnloadSceneAsync(Scene scene)
        {
            if(scene == Scene.Level1 || scene == Scene.Level2 || scene == Scene.Level3 || scene == Scene.Level4)
            {
                //GameManager.instance.SaveGame(); // SAVE GAME DATA

                // while(!GameManager.instance.dataSaved)
                // {
                //     yield return null;
                // }

                // GameManager.instance.dataSaved = false; 

                // GameManager.instance.SaveMainMenuData(); // SAVE MAIN MENU DATA

                // while(!GameManager.instance.dataSaved)
                // {
                //     yield return null;
                // }

                // GameManager.instance.dataSaved = false;
            }

            unloadingAsyncOperation = SceneManager.UnloadSceneAsync(scene.ToString()); // UNLOAD SCENE

            while(!unloadingAsyncOperation.isDone)
            {
                yield return null;
            }

            if(scene != Scene.Loading)
            {
                LoaderCallback();
            }
            else
            {
                CrossfadeController.instance.FadeIn(fadeLength);

                while(!crossfadeComplete)
                {
                    yield return null;
                }

                crossfadeComplete = false;

                if(GameManager.instance.currentScene == Scene.MainMenu)
                {
                    MainMenu.instance.EnableButtons(true); // ENABLE MAIN MENU
                    SelectionController.instance.SetupMainMenuSelection();
                }
                else if(GameManager.instance.currentScene == Scene.Level1 || GameManager.instance.currentScene == Scene.Level2 || GameManager.instance.currentScene == Scene.Level3 || GameManager.instance.currentScene == Scene.Level4)
                {
                    // START GAME
                }
            }
        }

        private IEnumerator LoadSetup()
        {
            setupAsyncOperation = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive); // LOAD MAIN MENU

            while(!setupAsyncOperation.isDone)
            {
                yield return null;
            }

            GameManager.instance.LoadGameData(); // LOAD MAIN MENU DATA

            while(!GameManager.instance.dataLoaded)
            {
                yield return null;
            }

            GameManager.instance.dataLoaded = false;

            CrossfadeController.instance.FadeIn(fadeLength); // FADE INTO MAIN MENU

            while(!crossfadeComplete)
            {
                yield return null;
            }

            crossfadeComplete = false;

            MainMenu.instance.EnableButtons(true); // ENABLE MAIN MENU
            SelectionController.instance.SetupMainMenuSelection();
        }
    
    #endregion
}