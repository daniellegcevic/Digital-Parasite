using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelectionController : MonoBehaviour
{
    #region Variables

        #region Singleton

            public static SelectionController instance = null;

        #endregion

        #region Inspector

            [Header("References")]
            public GameObject highlight;
            public GameObject selection;
            public GameObject menuSelectionRight;
            public GameObject menuSelectionLeft;

            [Header("Highlight Settings")]
            public float highlightYOffset;
            public float transitionDuration;
            public Color activeHighlight;
            public Color inactiveHighlight;
            public float gatewayScaleMultiplier;

            [Header("Selection Settings")]
            public float selectionYOffset;
            public Color activeSelection;
            public Color inactiveSelection;
            public Vector3 defaultScale;
            public Vector3 interactionScale;
            public float scaleDuration;
            public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            [Header("Menu Selection Settings")]
            public Color activeButtonSelection;
            public Color inactiveButtonSelection;
            public Color activeMenuSelection;
            public Color inactiveMenuSelection;
            public bool mainMenu;

            [Header("Controller Settings")]
            public float joystickThreshold;

            [Header("Selection Data")]
            public SelectableData[] selectableData;

        #endregion

        #region DEBUG

            [HideInInspector] public bool highlightActive = false;
            [HideInInspector] public bool selectionActive = false;
            [HideInInspector] public bool menuSelectionActive = false;
            private AnimationCurve transitionCurve;
            private Vector3 startScale;

            private int currentColumn = 0;
            private int currentRow = 0;

            private int currentMenuColumn = 0;
            private int currentMenuRow = 0;

            private bool allowHorizontalMovement = false;
            private bool allowVerticalMovement = false;

            [HideInInspector] public GameObject currentSelection;
            [HideInInspector] public GameObject currentMenuSelection;
            [HideInInspector] public GameObject[,] selectableObjects;

            private bool invalidSelection = false;
            private bool horizontalMovement = false;
            private bool verticalMovement = false;

        #endregion

        #region Components

            private SpriteRenderer highlightSpriteRenderer;
            private SpriteRenderer selectionSpriteRenderer;

            private Image menuSelectionImage1;
            private Image menuSelectionImage2;
            private Image menuSelectionImage3;
            private Image menuSelectionImage4;

            private GameObject currentArrows;

            private RectTransform rectTransform;

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
                transitionCurve = LevelManager.instance.transitionCurve;
                highlightSpriteRenderer = highlight.GetComponent<SpriteRenderer>();
                selectionSpriteRenderer = selection.GetComponent<SpriteRenderer>();
                startScale = highlight.transform.localScale;
            }
            else
            {
                transitionCurve = MainMenu.instance.buttonFadeCurve;
            }

            menuSelectionImage1 = menuSelectionRight.transform.GetChild(0).GetComponent<Image>();
            menuSelectionImage2 = menuSelectionRight.transform.GetChild(1).GetComponent<Image>();
            menuSelectionImage3 = menuSelectionLeft.transform.GetChild(0).GetComponent<Image>();
            menuSelectionImage4 = menuSelectionLeft.transform.GetChild(1).GetComponent<Image>();
        }

        private void Update()
        {
            if(GameManager.instance.inputMode == 2 && InteractionSystem.instance.enableInteraction)
            {
                if(InputHandler.instance.movementInputVector.x == 0 && InputHandler.instance.movementInputVector.y == 0)
                {
                    allowHorizontalMovement = true;
                    allowVerticalMovement = true;
                }

                if(allowHorizontalMovement)
                {
                    if(InputHandler.instance.movementInputVector.x > joystickThreshold)
                    {
                        if(!mainMenu && !PauseMenu.instance.gameIsPaused && !PauseMenu.instance.disableEscape)
                        {
                            currentColumn++;
                            allowHorizontalMovement = false;
                            allowVerticalMovement = false;
                            horizontalMovement = true;

                            invalidSelection = !IsColumnValid(currentColumn);

                            if((!invalidSelection && selectableObjects[currentColumn, currentRow]) || InvalidSelectionCheck())
                            {
                                if(!invalidSelection)
                                {
                                    SelectionCheck(selectableObjects[currentColumn, currentRow]);
                                }

                                invalidSelection = false;
                                horizontalMovement = false;

                                MoveSelectionTo(selectableObjects[currentColumn, currentRow], false);
                            }
                            else
                            {
                                currentColumn--;
                            }
                        }
                        else
                        {
                            currentMenuColumn++;
                            allowHorizontalMovement = false;
                            allowVerticalMovement = false;

                            if(IsColumnValid(currentMenuColumn) && selectableObjects[currentMenuColumn, currentMenuRow])
                            {
                                if(mainMenu)
                                {
                                    MenuSelectionCheck(selectableObjects[currentMenuColumn, currentMenuRow]);
                                }

                                MoveSelectionTo(selectableObjects[currentMenuColumn, currentMenuRow], false);
                            }
                            else
                            {
                                currentMenuColumn--;
                            }
                        }
                    }
                    else if(InputHandler.instance.movementInputVector.x < -joystickThreshold)
                    {
                        if(!mainMenu && !PauseMenu.instance.gameIsPaused && !PauseMenu.instance.disableEscape)
                        {
                            currentColumn--;
                            allowHorizontalMovement = false;
                            allowVerticalMovement = false;
                            horizontalMovement = true;

                            invalidSelection = !IsColumnValid(currentColumn);

                            if((!invalidSelection && selectableObjects[currentColumn, currentRow]) || InvalidSelectionCheck())
                            {
                                if(!invalidSelection)
                                {
                                    SelectionCheck(selectableObjects[currentColumn, currentRow]);
                                }

                                invalidSelection = false;
                                horizontalMovement = false;

                                MoveSelectionTo(selectableObjects[currentColumn, currentRow], false);
                            }
                            else
                            {
                                currentColumn++;
                            }
                        }
                        else
                        {
                            currentMenuColumn--;
                            allowHorizontalMovement = false;
                            allowVerticalMovement = false;

                            if(IsColumnValid(currentMenuColumn) && selectableObjects[currentMenuColumn, currentMenuRow])
                            {
                                if(mainMenu)
                                {
                                    MenuSelectionCheck(selectableObjects[currentMenuColumn, currentMenuRow]);
                                }

                                MoveSelectionTo(selectableObjects[currentMenuColumn, currentMenuRow], false);
                            }
                            else
                            {
                                currentMenuColumn++;
                            }
                        }
                    }
                }

                if(allowVerticalMovement)
                {
                    if(InputHandler.instance.movementInputVector.y > joystickThreshold)
                    {
                        if(!mainMenu && !PauseMenu.instance.gameIsPaused && !PauseMenu.instance.disableEscape)
                        {
                            currentRow++;
                            allowVerticalMovement = false;
                            allowHorizontalMovement = false;
                            verticalMovement = true;

                            invalidSelection = !IsRowValid(currentRow);

                            if((!invalidSelection && selectableObjects[currentColumn, currentRow]) || InvalidSelectionCheck())
                            {
                                if(!invalidSelection)
                                {
                                    SelectionCheck(selectableObjects[currentColumn, currentRow]);
                                }

                                invalidSelection = false;
                                verticalMovement = false;

                                MoveSelectionTo(selectableObjects[currentColumn, currentRow], false);
                            }
                            else
                            {
                                currentRow--;
                            }
                        }
                        else
                        {
                            currentMenuRow++;
                            allowVerticalMovement = false;
                            allowHorizontalMovement = false;

                            if(IsRowValid(currentMenuRow) && selectableObjects[currentMenuColumn, currentMenuRow])
                            {
                                if(mainMenu)
                                {
                                    MenuSelectionCheck(selectableObjects[currentMenuColumn, currentMenuRow]);
                                }

                                MoveSelectionTo(selectableObjects[currentMenuColumn, currentMenuRow], false);
                            }
                            else
                            {
                                currentMenuRow--;
                            }
                        }
                    }
                    else if(InputHandler.instance.movementInputVector.y < -joystickThreshold)
                    {
                        if(!mainMenu && !PauseMenu.instance.gameIsPaused && !PauseMenu.instance.disableEscape)
                        {
                            currentRow--;
                            allowVerticalMovement = false;
                            allowHorizontalMovement = false;
                            verticalMovement = true;

                            invalidSelection = !IsRowValid(currentRow);

                            if((!invalidSelection && selectableObjects[currentColumn, currentRow]) || InvalidSelectionCheck())
                            {
                                if(!invalidSelection)
                                {
                                    SelectionCheck(selectableObjects[currentColumn, currentRow]);
                                }

                                invalidSelection = false;
                                verticalMovement = false;

                                MoveSelectionTo(selectableObjects[currentColumn, currentRow], false);
                            }
                            else
                            {
                                currentRow++;
                            }
                        }
                        else
                        {
                            currentMenuRow--;
                            allowVerticalMovement = false;
                            allowHorizontalMovement = false;

                            if(IsRowValid(currentMenuRow) && selectableObjects[currentMenuColumn, currentMenuRow])
                            {
                                if(mainMenu)
                                {
                                    MenuSelectionCheck(selectableObjects[currentMenuColumn, currentMenuRow]);
                                }

                                MoveSelectionTo(selectableObjects[currentMenuColumn, currentMenuRow], false);
                            }
                            else
                            {
                                currentMenuRow++;
                            }
                        }
                    }
                }
            }
        }

    #endregion

    #region Custom Methods

        private void MenuSelectionCheck(GameObject selectableObject)
        {
            if(MainMenu.instance.currentMenu == 2)
            {
                if(selectableObject == selectableObjects[1, 0])
                {
                    if(currentMenuSelection == selectableObjects[1, 1])
                    {
                        currentMenuColumn--;
                    }
                    else if(currentMenuSelection == selectableObjects[0, 0])
                    {
                        currentMenuColumn--;
                    }
                }
                else if(selectableObject == selectableObjects[2, 0])
                {
                    if(currentMenuSelection == selectableObjects[2, 1])
                    {
                        currentMenuColumn -= 2;
                    }
                }
            }
        }

        private void SelectionCheck(GameObject selectableObject)
        {
            if(LevelManager.instance.currentLevel == 1)
            {
                if(LevelManager.instance.currentPuzzle == 2)
                {
                    if(selectableObject == selectableObjects[4, 0]) // Lower Right Corner
                    {
                        if(currentSelection == selectableObjects[3, 0])
                        {
                            currentRow++;
                        }
                        else if(currentSelection == selectableObjects[4, 1])
                        {
                            currentColumn--;
                        }
                    }
                }
                else if(LevelManager.instance.currentPuzzle == 3)
                {
                    if(selectableObject == selectableObjects[0, 3]) // Upper Left Corner
                    {
                        if(currentSelection == selectableObjects[0, 2])
                        {
                            currentColumn++;
                        }
                        else if(currentSelection == selectableObjects[1, 3])
                        {
                            currentRow--;
                        }
                    }
                    else if(selectableObject == selectableObjects[2, 3]) // Upper Right Corner
                    {
                        if(currentSelection == selectableObjects[2, 2])
                        {
                            currentColumn--;
                        }
                        else if(currentSelection == selectableObjects[1, 3])
                        {
                            currentRow--;
                        }
                    }
                    else if(selectableObject == selectableObjects[2, 1]) // Lower Right Corner Upward
                    {
                        if(currentSelection == selectableObjects[2, 0])
                        {
                            currentRow++;
                        }
                    }
                    else if(selectableObject == selectableObjects[0, 1]) // Lower Left Corner Upward
                    {
                        if(currentSelection == selectableObjects[0, 0])
                        {
                            currentRow++;
                        }
                    }
                    else if(selectableObject == selectableObjects[1, 2]) // Upper Corners Inward
                    {
                        if(currentSelection == selectableObjects[2, 2])
                        {
                            currentRow++;
                        }
                        if(currentSelection == selectableObjects[0, 2])
                        {
                            currentRow++;
                        }
                    }
                    else if(selectableObject == selectableObjects[0, 2]) // Middle
                    {
                        if(currentSelection == selectableObjects[1, 2])
                        {
                            currentRow--;
                        }
                    }
                    else if(selectableObject == selectableObjects[2, 2]) // Middle
                    {
                        if(currentSelection == selectableObjects[1, 2])
                        {
                            currentRow--;
                        }
                    }
                }
            }
            else if(LevelManager.instance.currentLevel == 2)
            {
                if(LevelManager.instance.currentPuzzle == 1)
                {
                    if(selectableObject == selectableObjects[1, 0]) // Lower Right
                    {
                        if(currentSelection == selectableObjects[0, 0])
                        {
                            currentRow += 2;
                        }
                    }
                    else if(selectableObject == selectableObjects[1, 1]) // Lower Middle Right
                    {
                        if(currentSelection == selectableObjects[0, 1])
                        {
                            currentRow++;
                        }
                        else if(currentSelection == selectableObjects[1, 2])
                        {
                            currentColumn--;
                        }
                    }
                    else if(selectableObject == selectableObjects[1, 3]) // Upper Right
                    {
                        if(currentSelection == selectableObjects[0, 3])
                        {
                            currentRow--;
                        }
                        else if(currentSelection == selectableObjects[1, 2])
                        {
                            currentColumn--;
                        }
                    }
                }
                else if(LevelManager.instance.currentPuzzle == 2)
                {
                    if(selectableObject == selectableObjects[3, 0])
                    {
                        if(currentSelection == selectableObjects[4, 0])
                        {
                            currentColumn--;
                        }
                        else if(currentSelection == selectableObjects[2, 0])
                        {
                            currentColumn++;
                        }
                        else if(currentSelection == selectableObjects[3, 1])
                        {
                            currentColumn++;
                            currentRow++;
                        }
                    }
                    else if(selectableObject == selectableObjects[2, 0])
                    {
                        if(currentSelection == selectableObjects[2, 1])
                        {
                            currentRow++;
                            currentColumn--;
                        }
                    }
                    else if(selectableObject == selectableObjects[2, 1])
                    {
                        if(currentSelection == selectableObjects[2, 0])
                        {
                            currentRow--;
                            currentColumn--;
                        }
                    }
                }
            }
            else if(LevelManager.instance.currentLevel == 3)
            {
                if(LevelManager.instance.currentPuzzle == 1)
                {
                    if(selectableObject == selectableObjects[0, 3]) // Right Top Row
                    {
                        if(currentSelection == selectableObjects[0, 2])
                        {
                            currentRow--;
                        }
                    }
                    else if(selectableObject == selectableObjects[1, 3])
                    {
                        if(currentSelection == selectableObjects[1, 2])
                        {
                            currentRow--;
                            currentColumn--;
                        }
                    }
                    else if(selectableObject == selectableObjects[2, 3])
                    {
                        if(currentSelection == selectableObjects[2, 2])
                        {
                            currentRow--;
                            currentColumn--;
                        }
                    }
                    else if(selectableObject == selectableObjects[3, 3]) // Middle Top Row
                    {
                        if(currentSelection == selectableObjects[3, 2])
                        {
                            //
                        }
                    }
                    else if(selectableObject == selectableObjects[4, 3]) // Left Top Row
                    {
                        if(currentSelection == selectableObjects[4, 2])
                        {
                            currentRow--;
                            currentColumn++;
                        }
                    }
                    else if(selectableObject == selectableObjects[5, 3])
                    {
                        if(currentSelection == selectableObjects[5, 2])
                        {
                            currentRow--;
                            currentColumn++;
                        }
                    }
                    else if(selectableObject == selectableObjects[6, 3])
                    {
                        if(currentSelection == selectableObjects[6, 2])
                        {
                            currentRow--;
                        }
                    }
                    else if(selectableObject == selectableObjects[3, 0]) // Under Source
                    {
                        if(currentSelection == selectableObjects[2, 0])
                        {
                            currentRow++;
                        }
                        else if(currentSelection == selectableObjects[4, 0])
                        {
                            currentRow++;
                        }
                        else if(currentSelection == selectableObjects[3, 1])
                        {
                            currentRow++;
                        }
                    }
                    else if(selectableObject == selectableObjects[3, 2]) // Above Source
                    {
                        if(currentSelection == selectableObjects[3, 1])
                        {
                            currentRow--;
                            currentColumn++;
                        }
                        else if(currentSelection == selectableObjects[4, 2])
                        {
                            currentColumn--;
                        }
                        else if(currentSelection == selectableObjects[2, 2])
                        {
                            currentColumn++;
                        }
                    }
                    else if(selectableObject == selectableObjects[5, 1]) // Lower Left
                    {
                        if(currentSelection == selectableObjects[5, 0])
                        {
                            currentRow--;
                            currentColumn++;
                        }
                        else if(currentSelection == selectableObjects[5, 2]) //// Left Top Reverse
                        {
                            currentRow++;
                            currentColumn--;
                        }
                    }
                    else if(selectableObject == selectableObjects[0, 1]) // Right Top Reverse
                    {
                        if(currentSelection == selectableObjects[0, 2])
                        {
                            currentRow++;
                            currentColumn++;
                        }
                    }
                    else if(selectableObject == selectableObjects[1, 1])
                    {
                        if(currentSelection == selectableObjects[1, 2])
                        {
                            currentRow++;
                            currentColumn++;
                        }
                    }
                    else if(selectableObject == selectableObjects[6, 1]) // Left Top Reverse
                    {
                        if(currentSelection == selectableObjects[6, 2])
                        {
                            currentRow++;
                            currentColumn--;
                        }
                    }
                    else if(selectableObject == selectableObjects[6, 2])
                    {
                        if(currentSelection == selectableObjects[6, 1])
                        {
                            currentRow--;
                            currentColumn--;
                        }
                    }
                    else if(selectableObject == selectableObjects[4, 1]) // Source
                    {
                        if(currentSelection == selectableObjects[3, 1])
                        {
                            currentRow--;
                        }
                    }
                    else if(selectableObject == selectableObjects[2, 1]) // Source
                    {
                        if(currentSelection == selectableObjects[3, 1])
                        {
                            currentRow--;
                        }
                    }
                }
            }
        }

        private bool InvalidSelectionCheck()
        {
            bool outOfBounds = false;

            if(LevelManager.instance.currentLevel == 1)
            {
                if(LevelManager.instance.currentPuzzle == 3)
                {
                    if(invalidSelection)
                    {
                        if(currentSelection == selectableObjects[2, 2]) // Middle Right
                        {
                            currentColumn--;
                            currentRow -= 2;
                            outOfBounds = true;
                        }
                        else if(currentSelection == selectableObjects[0, 2]) // Middle Left
                        {
                            currentColumn++;
                            currentRow -= 2;
                            outOfBounds = true;
                        }
                        else if(currentSelection == selectableObjects[2, 1]) // Middle Right
                        {
                            currentColumn--;
                            currentRow++;
                            outOfBounds = true;
                        }
                        else if(currentSelection == selectableObjects[0, 1]) // Middle Left
                        {
                            currentColumn++;
                            currentRow++;
                            outOfBounds = true;
                        }
                    }
                }
            }
            else if(LevelManager.instance.currentLevel == 2)
            {
                if(LevelManager.instance.currentPuzzle == 1)
                {
                    if(invalidSelection)
                    {
                        if(currentSelection == selectableObjects[0, 0]) // Lower Left
                        {
                            if(horizontalMovement)
                            {
                                currentColumn++;
                                currentRow += 2;
                                outOfBounds = true;
                            }
                        }
                        else if(currentSelection == selectableObjects[0, 1]) // Middle Left
                        {
                            currentColumn++;
                            currentRow++;
                            outOfBounds = true;
                        }
                        else if(currentSelection == selectableObjects[0, 3]) // Upper Left
                        {
                            if(horizontalMovement)
                            {
                                currentColumn++;
                                currentRow--;
                                outOfBounds = true;
                            }
                        }
                    }
                }
                else if(LevelManager.instance.currentPuzzle == 2)
                {
                    if(invalidSelection)
                    {
                        if(currentSelection == selectableObjects[4, 1]) // Upper Right
                        {
                            if(verticalMovement)
                            {
                                currentColumn--;
                                currentRow--;
                                outOfBounds = true;
                            }
                        }
                        else if(currentSelection == selectableObjects[5, 1])
                        {
                            if(verticalMovement)
                            {
                                currentColumn -= 2;
                                currentRow--;
                                outOfBounds = true;
                            }
                        }
                        else if(currentSelection == selectableObjects[1, 1]) // Upper Left
                        {
                            if(verticalMovement)
                            {
                                currentColumn++;
                                currentRow--;
                                outOfBounds = true;
                            }
                        }
                        else if(currentSelection == selectableObjects[0, 1])
                        {
                            if(verticalMovement)
                            {
                                currentColumn += 2;
                                currentRow--;
                                outOfBounds = true;
                            }
                        }
                        else if(currentSelection == selectableObjects[4, 0]) // Lower Right
                        {
                            if(verticalMovement)
                            {
                                currentColumn -= 2;
                                currentRow++;
                                outOfBounds = true;
                            }
                        }
                        else if(currentSelection == selectableObjects[5, 0])
                        {
                            if(verticalMovement)
                            {
                                currentColumn -= 3;
                                currentRow++;
                                outOfBounds = true;
                            }
                        }
                        else if(currentSelection == selectableObjects[1, 0]) // Lower Left
                        {
                            if(verticalMovement)
                            {
                                currentColumn++;
                                currentRow++;
                                outOfBounds = true;
                            }
                        }
                        else if(currentSelection == selectableObjects[0, 0])
                        {
                            if(verticalMovement)
                            {
                                currentColumn += 2;
                                currentRow++;
                                outOfBounds = true;
                            }
                        }
                    }
                }
            }
            else if(LevelManager.instance.currentLevel == 3)
            {
                if(LevelManager.instance.currentPuzzle == 1)
                {
                    if(invalidSelection)
                    {
                        if(currentSelection == selectableObjects[6, 0]) // Lower Left
                        {
                            if(verticalMovement)
                            {
                                currentColumn--;
                                currentRow++;
                                outOfBounds = true;
                            }
                        }
                        else if(currentSelection == selectableObjects[0, 0]) // Lower Right
                        {
                            if(verticalMovement)
                            {
                                currentColumn++;
                                currentRow++;
                                outOfBounds = true;
                            }
                        }
                    }
                }
            }

            return outOfBounds;
        }

        private bool IsRowValid(int row)
        {
            return row >= 0 && row < selectableObjects.GetLength(1);
        }

        private bool IsColumnValid(int column)
        {
            return column >= 0 && column < selectableObjects.GetLength(0);
        }

        public void SetupSelectionData(bool moveToSelection)
        {
            if(LevelManager.instance.currentLevel == 1)
            {
                if(LevelManager.instance.currentPuzzle == 1)
                {
                    selectableObjects = new GameObject[3, 2]
                    {
                        {selectableData[2].column1[0], selectableData[2].column1[1]},
                        {selectableData[2].column2[0], selectableData[2].column2[1]},
                        {selectableData[2].column3[0], selectableData[2].column3[1]}
                    };

                    if(moveToSelection)
                    {
                        currentColumn = 1;
                        currentRow = 0;

                        MoveSelectionTo(selectableObjects[currentColumn, currentRow], true);
                    }
                }
                else if(LevelManager.instance.currentPuzzle == 2)
                {
                    selectableObjects = new GameObject[5, 2]
                    {
                        {selectableData[3].column1[0], selectableData[3].column1[1]},
                        {selectableData[3].column2[0], selectableData[3].column2[1]},
                        {selectableData[3].column3[0], selectableData[3].column3[1]},
                        {selectableData[3].column4[0], selectableData[3].column4[1]},
                        {selectableData[3].column5[0], selectableData[3].column5[1]}
                    };

                    if(moveToSelection)
                    {
                        currentColumn = 1;
                        currentRow = 0;

                        MoveSelectionTo(selectableObjects[currentColumn, currentRow], true);
                    }
                }
                else if(LevelManager.instance.currentPuzzle == 3)
                {
                    selectableObjects = new GameObject[3, 4]
                    {
                        {selectableData[4].column1[0], selectableData[4].column1[1], selectableData[4].column1[2], selectableData[4].column1[3]},
                        {selectableData[4].column2[0], selectableData[4].column2[1], selectableData[4].column2[2], selectableData[4].column2[3]},
                        {selectableData[4].column3[0], selectableData[4].column3[1], selectableData[4].column3[2], selectableData[4].column3[3]}
                    };

                    if(moveToSelection)
                    {
                        currentColumn = 1;
                        currentRow = 1;

                        MoveSelectionTo(selectableObjects[currentColumn, currentRow], true);
                    }
                }
            }
            else if(LevelManager.instance.currentLevel == 2)
            {
                if(LevelManager.instance.currentPuzzle == 1)
                {
                    selectableObjects = new GameObject[2, 4]
                    {
                        {selectableData[5].column1[0], selectableData[5].column1[1], selectableData[5].column1[2], selectableData[5].column1[3]},
                        {selectableData[5].column2[0], selectableData[5].column2[1], selectableData[5].column2[2], selectableData[5].column2[3]}
                    };

                    if(moveToSelection)
                    {
                        currentColumn = 0;
                        currentRow = 0;

                        MoveSelectionTo(selectableObjects[currentColumn, currentRow], true);
                    }
                }
                else if(LevelManager.instance.currentPuzzle == 2)
                {
                    selectableObjects = new GameObject[6, 2]
                    {
                        {selectableData[6].column1[0], selectableData[6].column1[1]},
                        {selectableData[6].column2[0], selectableData[6].column2[1]},
                        {selectableData[6].column3[0], selectableData[6].column3[1]},
                        {selectableData[6].column4[0], selectableData[6].column4[1]},
                        {selectableData[6].column5[0], selectableData[6].column5[1]},
                        {selectableData[6].column6[0], selectableData[6].column6[1]}
                    };

                    if(moveToSelection)
                    {
                        currentColumn = 4;
                        currentRow = 0;

                        MoveSelectionTo(selectableObjects[currentColumn, currentRow], true);
                    }
                }
            }
            else if(LevelManager.instance.currentLevel == 3)
            {
                if(LevelManager.instance.currentPuzzle == 1)
                {
                    selectableObjects = new GameObject[7, 4]
                    {
                        {selectableData[7].column1[0], selectableData[7].column1[1], selectableData[7].column1[2], selectableData[7].column1[3]},
                        {selectableData[7].column2[0], selectableData[7].column2[1], selectableData[7].column2[2], selectableData[7].column2[3]},
                        {selectableData[7].column3[0], selectableData[7].column3[1], selectableData[7].column3[2], selectableData[7].column3[3]},
                        {selectableData[7].column4[0], selectableData[7].column4[1], selectableData[7].column4[2], selectableData[7].column4[3]},
                        {selectableData[7].column5[0], selectableData[7].column5[1], selectableData[7].column5[2], selectableData[7].column5[3]},
                        {selectableData[7].column6[0], selectableData[7].column6[1], selectableData[7].column6[2], selectableData[7].column6[3]},
                        {selectableData[7].column7[0], selectableData[7].column7[1], selectableData[7].column7[2], selectableData[7].column7[3]}
                    };

                    if(moveToSelection)
                    {
                        currentColumn = 4;
                        currentRow = 2;

                        MoveSelectionTo(selectableObjects[currentColumn, currentRow], true);
                    }
                }
            }
        }

        public void SetupPauseMenuSelection()
        {
            selectableObjects = new GameObject[1, 3]
            {
                {selectableData[0].column1[0], selectableData[0].column1[1], selectableData[0].column1[2]}
            };

            currentMenuColumn = 0;
            currentMenuRow = 2;

            MoveSelectionTo(selectableObjects[currentMenuColumn, currentMenuRow], true);
        }

        public void SetupMainMenuSelection()
        {
            selectableObjects = new GameObject[1, 4]
            {
                {selectableData[0].column1[0], selectableData[0].column1[1], selectableData[0].column1[2], selectableData[0].column1[3]}
            };

            currentMenuColumn = 0;
            currentMenuRow = 3;

            MoveSelectionTo(selectableObjects[currentMenuColumn, currentMenuRow], true);
        }

        public void SetupLevelSelectSelection()
        {
            selectableObjects = new GameObject[3, 2]
            {
                {selectableData[1].column1[0], selectableData[1].column1[1]},
                {selectableData[1].column2[0], selectableData[1].column2[1]},
                {selectableData[1].column3[0], selectableData[1].column3[1]}
            };

            currentMenuColumn = 0;
            currentMenuRow = 1;

            MoveSelectionTo(selectableObjects[currentMenuColumn, currentMenuRow], true);
        }

        public void SetupEndScreenSelection()
        {
            if(LevelManager.instance.currentLevel == 3)
            {
                selectableObjects = new GameObject[2, 1]
                {
                    {selectableData[1].column1[0]},
                    {selectableData[1].column2[0]}
                };

                currentMenuColumn = 1;
                currentMenuRow = 0;

                MoveSelectionTo(selectableObjects[currentMenuColumn, currentMenuRow], true);
            }
            else
            {
                selectableObjects = new GameObject[3, 1]
                {
                    {selectableData[1].column1[0]},
                    {selectableData[1].column2[0]},
                    {selectableData[1].column3[0]}
                };

                currentMenuColumn = 1;
                currentMenuRow = 0;

                MoveSelectionTo(selectableObjects[currentMenuColumn, currentMenuRow], true);
            }
        }

        private void MoveSelectionTo(GameObject selectableObject, bool setup)
        {
            if(!mainMenu && !PauseMenu.instance.gameIsPaused && (!PauseMenu.instance.disableEscape || PauseMenu.instance.transition))
            {
                MoveHighlightTo(selectableObject, false, setup);

                currentSelection = selectableObject;

                selectionActive = true;
                selection.transform.position = new Vector3(selectableObject.transform.position.x, selectableObject.transform.position.y + selectionYOffset, selectableObject.transform.position.z);

                if(setup)
                {
                    selectionSpriteRenderer.color = inactiveSelection;
                    selection.SetActive(true);
                    StartCoroutine(FadeInSelectionCoroutine());
                }
                else
                {
                    SoundEffectManager.instance.PlaySoundEffect("Node Select", gameObject);
                    selection.SetActive(true);
                }
            }
            else
            {
                if(currentMenuSelection)
                {
                    currentMenuSelection.GetComponent<Image>().color = new Color(inactiveButtonSelection.r, inactiveButtonSelection.g, inactiveButtonSelection.b, currentMenuSelection.GetComponent<Image>().color.a);
                }

                currentMenuSelection = selectableObject;

                rectTransform = selectableObject.GetComponent<RectTransform>();

                Vector3 previousPosition = menuSelectionRight.transform.position;

                menuSelectionActive = true;
                menuSelectionRight.transform.position = new Vector3(selectableObject.transform.GetChild(0).position.x, selectableObject.transform.position.y, selectableObject.transform.position.z);
                menuSelectionLeft.transform.position = new Vector3(selectableObject.transform.GetChild(1).position.x, selectableObject.transform.position.y, selectableObject.transform.position.z);

                currentMenuSelection.GetComponent<Image>().color = new Color(activeButtonSelection.r, activeButtonSelection.g, activeButtonSelection.b, currentMenuSelection.GetComponent<Image>().color.a);

                if(setup)
                {
                    menuSelectionImage1.color = activeMenuSelection;
                    menuSelectionImage2.color = activeMenuSelection;
                    menuSelectionImage3.color = activeMenuSelection;
                    menuSelectionImage4.color = activeMenuSelection;

                    menuSelectionRight.SetActive(true);
                    menuSelectionLeft.SetActive(true);
                }
                else
                {
                    if(previousPosition != menuSelectionRight.transform.position)
                    {
                        SoundEffectManager.instance.PlaySoundEffect("Button Select", gameObject);
                    }

                    menuSelectionRight.SetActive(true);
                    menuSelectionLeft.SetActive(true);
                }
            }
        }

        public void MoveHighlightTo(GameObject selectableObject, bool gateway, bool setup)
        {
            highlightActive = true;
            highlight.transform.position = new Vector3(selectableObject.transform.position.x, selectableObject.transform.position.y + highlightYOffset, selectableObject.transform.position.z);

            if(gateway)
            {
                highlight.transform.localScale *= gatewayScaleMultiplier;
            }

            if(setup)
            {
                highlightSpriteRenderer.color = inactiveHighlight;
                highlight.SetActive(true);
                StartCoroutine(FadeInHighlightCoroutine());
            }
            else
            {
                highlight.SetActive(true);
            }
        }

        public void RemoveHighlight()
        {
            highlightActive = false;
            highlight.SetActive(false);
        }

        public void RemoveSelection()
        {
            selectionActive = false;
            selection.SetActive(false);
            selection.transform.localScale = defaultScale;
        }

        public void RemoveMenuSelection()
        {
            menuSelectionActive = false;
            menuSelectionRight.SetActive(false);
            menuSelectionLeft.SetActive(false);
        }

        public void FadeOutHighlight()
        {
            if(highlightActive)
            {
                StartCoroutine(FadeOutHighlightCoroutine());
            }
        }

        public void FadeOutSelection()
        {
            if(selectionActive)
            {
                StartCoroutine(FadeOutSelectionCoroutine());
            }
        }

        public void FadeOutMenuSelection()
        {
            if(menuSelectionActive || (MainMenu.instance && MainMenu.instance.backToMainMenu))
            {
                if(MainMenu.instance)
                {
                    MainMenu.instance.backToMainMenu = false;
                }

                StartCoroutine(FadeOutMenuSelectionCoroutine());
            }
        }

        public void FadeInMenuSelection()
        {
            menuSelectionRight.SetActive(true);
            menuSelectionLeft.SetActive(true);

            StartCoroutine(FadeInMenuSelectionCoroutine());
        }

        public void ScaleSelection()
        {
            StartCoroutine(Scale());
        }

    #endregion

    #region Coroutines

        private IEnumerator FadeOutHighlightCoroutine()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);

                highlightSpriteRenderer.color = Color.Lerp(activeHighlight, inactiveHighlight, smoothTransitionPercentage);

                yield return null;
            }

            RemoveHighlight();

            highlightSpriteRenderer.color = activeHighlight;
        }

        private IEnumerator FadeInHighlightCoroutine()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);

                highlightSpriteRenderer.color = Color.Lerp(inactiveHighlight, activeHighlight, smoothTransitionPercentage);

                yield return null;
            }
        }

        private IEnumerator FadeOutSelectionCoroutine()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);

                selectionSpriteRenderer.color = Color.Lerp(activeSelection, inactiveSelection, smoothTransitionPercentage);

                yield return null;
            }

            RemoveSelection();

            selectionSpriteRenderer.color = activeHighlight;
        }

        private IEnumerator FadeOutMenuSelectionCoroutine()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);

                menuSelectionImage1.color = Color.Lerp(activeMenuSelection, inactiveMenuSelection, smoothTransitionPercentage);
                menuSelectionImage2.color = Color.Lerp(activeMenuSelection, inactiveMenuSelection, smoothTransitionPercentage);
                menuSelectionImage3.color = Color.Lerp(activeMenuSelection, inactiveMenuSelection, smoothTransitionPercentage);
                menuSelectionImage4.color = Color.Lerp(activeMenuSelection, inactiveMenuSelection, smoothTransitionPercentage);

                yield return null;
            }

            RemoveMenuSelection();
        }

        private IEnumerator FadeInSelectionCoroutine()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);

                selectionSpriteRenderer.color = Color.Lerp(inactiveSelection, activeSelection, smoothTransitionPercentage);

                yield return null;
            }
        }

        private IEnumerator FadeInMenuSelectionCoroutine()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);

                menuSelectionImage1.color = Color.Lerp(inactiveMenuSelection, activeMenuSelection, smoothTransitionPercentage);
                menuSelectionImage2.color = Color.Lerp(inactiveMenuSelection, activeMenuSelection, smoothTransitionPercentage);
                menuSelectionImage3.color = Color.Lerp(inactiveMenuSelection, activeMenuSelection, smoothTransitionPercentage);
                menuSelectionImage4.color = Color.Lerp(inactiveMenuSelection, activeMenuSelection, smoothTransitionPercentage);

                yield return null;
            }

            if(MainMenu.instance.levelSelectActive)
            {
                MainMenu.instance.levelSelectBackActive = true;
            }
            else if(!MainMenu.instance.mainMenuActive)
            {
                MainMenu.instance.mainMenuActive = true;
                MainMenu.instance.disableEscape = false;
            }
        }

        private IEnumerator Scale()
        {
            float scalePercentage = 0f;
            float smoothScalePercentage = 0f;
            float scaleSpeed = 1f / scaleDuration;

            while(scalePercentage < 1f)
            {
                scalePercentage += Time.deltaTime * scaleSpeed;
                smoothScalePercentage = scaleCurve.Evaluate(scalePercentage);

                selection.transform.localScale = Vector3.Lerp(defaultScale, interactionScale, smoothScalePercentage);

                yield return null;
            }
        }

    #endregion
}