using UnityEngine;

public class StartLevel : MonoBehaviour
{   
    #region Custom Methods
    
        private void StartGame()
        {
            CameraController.instance.StartLevel();
        }
    
    #endregion
}