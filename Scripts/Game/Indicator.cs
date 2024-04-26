using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    #region Variables
    
        #region Inspector
        
            public bool active = false;
            public Connection connection;
        
        #endregion
    
        #region DEBUG
        
            private float transitionDuration;
            private AnimationCurve transitionCurve;
        
        #endregion
    
        #region Components
        
            private bool connectionStatus;
            
            private Material activeMaterial;
            private Material inactiveMaterial;

            private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
            private MeshRenderer meshRenderer;
        
        #endregion
    
    #endregion
    
    #region Built-in Methods
    
        private void Start()
        {
            activeMaterial = LevelManager.instance.activeIndicator;
            inactiveMaterial = LevelManager.instance.inactiveIndicator;

            meshRenderer = GetComponent<MeshRenderer>();

            if(meshRenderer)
            {
                meshRenderers.Add(meshRenderer);
            }
            
            for(int i = 0; i < transform.childCount; i++)
            {
                meshRenderers.Add(transform.GetChild(i).GetComponent<MeshRenderer>());
            }

            transitionDuration = LevelManager.instance.transitionDuration;
            transitionCurve = LevelManager.instance.transitionCurve;
        }

        private void Update()
        {
            if(connection && connectionStatus != connection.active)
            {
                connectionStatus = !connectionStatus;
                ChangeState();
            }
        }
    
    #endregion
    
    #region Custom Methods
    
        public void ChangeState()
        {
            if(!active)
            {
                active = true;
                StartCoroutine(ChangeMaterial(true));
            }
            else
            {
                active = false;
                StartCoroutine(ChangeMaterial(false));
            }
        }
    
    #endregion
    
    #region Coroutines
        
        private IEnumerator ChangeMaterial(bool activate)
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);

                if(activate)
                {
                    foreach(MeshRenderer meshRenderer in meshRenderers)
                    {
                        meshRenderer.material.color = Color.Lerp(inactiveMaterial.color, activeMaterial.color, smoothTransitionPercentage);
                    }
                }
                else
                {
                    foreach(MeshRenderer meshRenderer in meshRenderers)
                    {
                        meshRenderer.material.color = Color.Lerp(activeMaterial.color, inactiveMaterial.color, smoothTransitionPercentage);
                    }
                }

                yield return null;
            }
        }
    
    #endregion
}