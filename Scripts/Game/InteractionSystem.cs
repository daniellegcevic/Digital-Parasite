using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    #region Variables
    
        #region Singleton

            public static InteractionSystem instance = null;

        #endregion
        
        #region Inspector
        
            public float raycastLength = 10f;
            public Camera mainCamera;
            public bool mainMenu;
        
        #endregion
    
        #region DEBUG

            [HideInInspector] public bool enableInteraction = false;

            private Vector2 cursorPosition;
            private Vector3 screenPosition;
            private Vector3 worldPosition;

            private GameObject followPosition;
            private float followSpeed;

        #endregion
    
        #region Components
        
            private Node node;
            private Blocker blocker;
            private Gateway gateway;
            
            private RaycastHit hit;

            private SelectionController selectionController;
        
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
            if(!mainMenu)
            {
                followPosition = CameraController.instance.followPosition;
            }

            selectionController = SelectionController.instance;
        }

        private void Update()
        {
            if(!mainMenu)
            {
                if(LevelManager.instance.startLevel)
                {
                    if(GameManager.instance.inputMode == 1)
                    {
                        followSpeed = CameraController.instance.mouseFollowSpeed;
                        
                        screenPosition = Input.mousePosition;
                        screenPosition.z = mainCamera.nearClipPlane + followSpeed;

                        worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
                        followPosition.transform.position = worldPosition;        

                        if(enableInteraction)
                        {
                            Physics.Raycast(mainCamera.ScreenPointToRay(screenPosition), out hit, raycastLength);

                            if(hit.transform)
                            {
                                node = hit.transform.GetComponent<Node>();
                                gateway = hit.transform.GetComponent<Gateway>();
                                blocker = hit.transform.GetComponent<Blocker>();

                                if(node)
                                {
                                    if(!selectionController.highlightActive)
                                    {
                                        selectionController.MoveHighlightTo(node.gameObject, false, false);
                                    }
                                }
                                else if(gateway)
                                {
                                    if(!selectionController.highlightActive)
                                    {
                                        selectionController.MoveHighlightTo(gateway.gameObject, true, false);
                                    }
                                }
                                else if(blocker)
                                {
                                    if(!selectionController.highlightActive)
                                    {
                                        selectionController.MoveHighlightTo(blocker.node.gameObject, false, false);
                                    }
                                }
                                else if(selectionController.highlightActive)
                                {
                                    selectionController.RemoveHighlight();
                                }

                                if(InputHandler.instance.leftMouseClicked)
                                {
                                    if(node && !node.rotating && !node.locked)
                                    {
                                        node.RotateNode();
                                        selectionController.ScaleSelection();
                                    }
                                    else if(gateway && !gateway.blinking)
                                    {
                                        if(!gateway.locked)
                                        {
                                            gateway.UnlockGateway();
                                        }
                                        else
                                        {
                                            gateway.BlinkGateway();
                                        }
                                        
                                        selectionController.ScaleSelection();
                                    }
                                    else if(blocker)
                                    {
                                        if(!blocker.locked)
                                        {
                                            blocker.UnlockBlocker();
                                        }
                                        else if(!blocker.node.rotating)
                                        {
                                            blocker.LockedAnimation();
                                        }
                                        
                                        selectionController.ScaleSelection();
                                    }
                                }
                            }
                            else if(selectionController.highlightActive)
                            {
                                selectionController.RemoveHighlight();
                            }
                        }
                    }
                    else if(GameManager.instance.inputMode == 2)
                    {
                        if(!PauseMenu.instance.gameIsPaused && !mainMenu && !PauseMenu.instance.disableEscape)
                        {
                            if(selectionController.currentSelection)
                            {
                                followSpeed = CameraController.instance.keyboardFollowSpeed;

                                screenPosition = mainCamera.WorldToScreenPoint(selectionController.currentSelection.transform.position);
                                screenPosition.z = mainCamera.nearClipPlane + followSpeed;

                                worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
                                followPosition.transform.position = worldPosition;  
                            }

                            if(enableInteraction)
                            {
                                if(selectionController.currentSelection)
                                {
                                    node = selectionController.currentSelection.GetComponent<Node>();
                                    gateway = selectionController.currentSelection.GetComponent<Gateway>();
                                    blocker = selectionController.currentSelection.GetComponent<Blocker>();

                                    if(node)
                                    {
                                        if(!selectionController.highlightActive)
                                        {
                                            selectionController.MoveHighlightTo(node.gameObject, false, false);
                                        }
                                    }
                                    else if(gateway)
                                    {
                                        if(!selectionController.highlightActive)
                                        {
                                            selectionController.MoveHighlightTo(gateway.gameObject, true, false);
                                        }
                                    }
                                    else if(blocker)
                                    {
                                        if(!selectionController.highlightActive)
                                        {
                                            selectionController.MoveHighlightTo(blocker.node.gameObject, false, false);
                                        }
                                    }
                                    else if(selectionController.highlightActive)
                                    {
                                        selectionController.RemoveHighlight();
                                    }

                                    if(InputHandler.instance.interactClicked || InputHandler.instance.controllerInteractClicked)
                                    {
                                        if(node && !node.rotating && !node.locked)
                                        {
                                            node.RotateNode();
                                            selectionController.ScaleSelection();
                                        }
                                        else if(gateway)
                                        {
                                            if(!gateway.blinking)
                                            {
                                                if(!gateway.locked)
                                                {
                                                    gateway.UnlockGateway();
                                                }
                                                else
                                                {
                                                    gateway.BlinkGateway();
                                                }
                                                
                                                selectionController.ScaleSelection();
                                            }
                                        }
                                        else if(node.blocker && !node.rotating)
                                        {
                                            if(!node.blocker.locked)
                                            {
                                                node.blocker.UnlockBlocker();
                                            }
                                            else
                                            {
                                                node.blocker.LockedAnimation();
                                            }
                                            
                                            selectionController.ScaleSelection();
                                        }
                                    }
                                }
                                else if(selectionController.highlightActive)
                                {
                                    selectionController.RemoveHighlight();
                                }
                            }
                        }
                        else
                        {
                            if(selectionController.currentMenuSelection && enableInteraction)
                            {
                                if(InputHandler.instance.interactClicked || InputHandler.instance.controllerInteractClicked)
                                {
                                    if(PauseMenu.instance.gameIsPaused)
                                    {
                                        if(selectionController.currentMenuSelection == selectionController.selectableObjects[0, 2])
                                        {
                                            PauseMenu.instance.ResumeGame();
                                            SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                                        }
                                        else if(selectionController.currentMenuSelection == selectionController.selectableObjects[0, 1])
                                        {
                                            PauseMenu.instance.RestartLevel();
                                            SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                                        }
                                        else if(selectionController.currentMenuSelection == selectionController.selectableObjects[0, 0])
                                        {
                                            PauseMenu.instance.ExitToMainMenu();
                                            SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                                        }
                                    }
                                    else
                                    {
                                        if(selectionController.currentMenuSelection == selectionController.selectableObjects[0, 0])
                                        {
                                            PauseMenu.instance.RestartLevel();
                                            SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                                        }
                                        else if(selectionController.currentMenuSelection == selectionController.selectableObjects[1, 0])
                                        {
                                            LevelManager.instance.NextLevel();
                                            SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                                        }
                                        else if(selectionController.currentMenuSelection == selectionController.selectableObjects[2, 0])
                                        {
                                            PauseMenu.instance.ExitToMainMenu();
                                            SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if(selectionController.currentMenuSelection && enableInteraction)
                {
                    if(InputHandler.instance.interactClicked || InputHandler.instance.controllerInteractClicked)
                    {
                        if(MainMenu.instance.currentMenu == 1)
                        {
                            if(selectionController.currentMenuSelection == selectionController.selectableObjects[0, 3])
                            {
                                MainMenu.instance.NewGame();
                                SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                            }
                            else if(selectionController.currentMenuSelection == selectionController.selectableObjects[0, 2])
                            {
                                MainMenu.instance.LevelSelect();
                                SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                            }
                            else if(selectionController.currentMenuSelection == selectionController.selectableObjects[0, 1])
                            {
                                MainMenu.instance.Credits();
                                SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                            }
                            else if(selectionController.currentMenuSelection == selectionController.selectableObjects[0, 0])
                            {
                                MainMenu.instance.Quit();
                                SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                            }
                        }
                        else if(MainMenu.instance.currentMenu == 2)
                        {
                            if(selectionController.currentMenuSelection == selectionController.selectableObjects[0, 0])
                            {
                                MainMenu.instance.Back();
                                SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                            }
                            else if(selectionController.currentMenuSelection == selectionController.selectableObjects[0, 1])
                            {
                                MainMenu.instance.Level1();
                                SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                            }
                            else if(selectionController.currentMenuSelection == selectionController.selectableObjects[1, 1])
                            {
                                MainMenu.instance.Level2();
                                SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                            }
                            else if(selectionController.currentMenuSelection == selectionController.selectableObjects[2, 1])
                            {
                                MainMenu.instance.Level3();
                                SoundEffectManager.instance.PlaySoundEffect("Button Click", gameObject);
                            }
                        }
                    }
                }
            }
        }
    
    #endregion
}