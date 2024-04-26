using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class Gateway : MonoBehaviour
{
    #region Variables
    
        #region Inspector

            public Connection[] connections;

            public MeshRenderer circle;
            public Color activeSelection;
            public Color inactiveSelection;
            public float movementDuration;
            public AnimationCurve movementCurve;

            public GameObject[] arrows;
            
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
     
            private bool[] connectionStatus;
            private int activeConnections = 0;

            [HideInInspector] public bool locked = true;

            private float transitionDuration;
            private AnimationCurve transitionCurve;

            private bool stopArrows;

            [HideInInspector] public bool blinking = false;
        
        #endregion

        #region Components
            
            private Material activeMaterial;
            private Material inactiveMaterial;
            private Material noConnection;
            private SpriteRenderer[] spriteRenderers = new SpriteRenderer[4];
        
        #endregion
    
    #endregion
    
    #region Built-in Methods
    
        private void Start()
        {
            activeMaterial = LevelManager.instance.activeIndicator;
            inactiveMaterial = LevelManager.instance.inactiveIndicator;
            noConnection = LevelManager.instance.noConnection;

            connectionStatus = new bool[connections.Length];

            transitionDuration = LevelManager.instance.transitionDuration;
            transitionCurve = LevelManager.instance.transitionCurve;
            
            spriteRenderers[0] = arrows[0].GetComponent<SpriteRenderer>();
            spriteRenderers[1] = arrows[1].GetComponent<SpriteRenderer>();
            spriteRenderers[2] = arrows[2].GetComponent<SpriteRenderer>();
            spriteRenderers[3] = arrows[3].GetComponent<SpriteRenderer>();
        }
    
        private void Update()
        {
            int x = 0;

            foreach(Connection connection in connections)
            {
                if(connectionStatus[x] != connection.active)
                {
                    if(connection.active)
                    {
                        activeConnections++;
                        Debug.Log("Connection Active");
                    }
                    else
                    {
                        activeConnections--;
                        Debug.Log("Connection Inactive");
                    }

                    GatewayCheck();

                    connectionStatus[x] = !connectionStatus[x];
                }

                x++;
            }
        }
    
    #endregion
    
    #region Custom Methods
    
        private void GatewayCheck()
        {
            Debug.Log("activeConnections: " + activeConnections);
            Debug.Log("connections.Length: " + connections.Length);
            if(activeConnections == connections.Length)
            {
                locked = false;

                stopArrows = false;

                foreach(GameObject arrow in arrows)
                {
                    arrow.gameObject.SetActive(true);
                }

                StartCoroutine(MoveArrows());
            }
            else
            {
                locked = true;

                stopArrows = true;

                foreach(GameObject arrow in arrows)
                {
                    arrow.gameObject.SetActive(false);
                }

                arrows[0].transform.position = topStartPosition.transform.position;
                arrows[1].transform.position = rightStartPosition.transform.position;
                arrows[2].transform.position = bottomStartPosition.transform.position;
                arrows[3].transform.position = leftStartPosition.transform.position;
            }
        }

        public void UnlockGateway()
        {
            SoundEffectManager.instance.PlaySoundEffect("Access Gateway", gameObject);

            PuzzleComplete();

            circle.material.color = activeMaterial.color;
        }

        public void BlinkGateway()
        {
            SoundEffectManager.instance.PlaySoundEffect("Locked Gateway", gameObject);
            
            blinking = true;

            StartCoroutine(Blink());
        }

        private void PuzzleComplete()
        {
            InteractionSystem.instance.enableInteraction = false;
            SelectionController.instance.FadeOutHighlight();
            SelectionController.instance.FadeOutSelection();
            StartCoroutine(FadeArrows());

            if(LevelManager.instance.levelType == LevelManager.LevelType.Standard)
            {
                GatewayCounter.instance.AddGateway();
            }
            else
            {
                Timer.instance.StopTimer();
            }
        }
    
    #endregion

    #region Coroutines

        private IEnumerator Blink()
        {
            circle.material.color = noConnection.color;

            yield return new WaitForSeconds(transitionDuration);

            circle.material.color = inactiveMaterial.color;

            yield return new WaitForSeconds(0.25f - transitionDuration);

            blinking = false;
        }

        private IEnumerator MoveArrows()
        {
            float movementPercentage = 0f;
            float smoothMovementPercentage = 0f;
            float movementSpeed = 1f / movementDuration;

            while(!stopArrows)
            {
                while(movementPercentage < 1f)
                {
                    movementPercentage += Time.deltaTime * movementSpeed;
                    smoothMovementPercentage = movementCurve.Evaluate(movementPercentage);

                    arrows[0].transform.position = Vector3.Lerp(topStartPosition.transform.position, topEndPosition.transform.position, smoothMovementPercentage);
                    arrows[1].transform.position = Vector3.Lerp(rightStartPosition.transform.position, rightEndPosition.transform.position, smoothMovementPercentage);
                    arrows[2].transform.position = Vector3.Lerp(bottomStartPosition.transform.position, bottomEndPosition.transform.position, smoothMovementPercentage);
                    arrows[3].transform.position = Vector3.Lerp(leftStartPosition.transform.position, leftEndPosition.transform.position, smoothMovementPercentage);

                    yield return null;
                }

                movementPercentage = 0f;
                smoothMovementPercentage = 0f;
            }
        }

        private IEnumerator FadeArrows()
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);
                
                spriteRenderers[0].color = Color.Lerp(activeSelection, inactiveSelection, smoothTransitionPercentage);
                spriteRenderers[1].color = Color.Lerp(activeSelection, inactiveSelection, smoothTransitionPercentage);
                spriteRenderers[2].color = Color.Lerp(activeSelection, inactiveSelection, smoothTransitionPercentage);
                spriteRenderers[3].color = Color.Lerp(activeSelection, inactiveSelection, smoothTransitionPercentage);

                yield return null;
            }

            foreach(GameObject arrow in arrows)
            {
                arrow.gameObject.SetActive(false);
            }
        }

    #endregion
}