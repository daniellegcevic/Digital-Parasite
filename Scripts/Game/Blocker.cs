using System.Collections;
using UnityEngine;

public class Blocker : MonoBehaviour
{
    #region Variables
    
        #region Inspector
        
            public bool locked = true;

            public Connection[] connections;
            public Node node;

            [Header("Locked Animation")]
            public float animationDuration;
            public AnimationCurve animationCurve;
        
        #endregion
    
        #region DEBUG
        
            private bool[] connectionStatus;
            private int activeConnections = 0;
        
        #endregion
    
    #endregion
    
    #region Built-in Methods
    
        private void Start()
        {
            node.LockNode();
            connectionStatus = new bool[connections.Length];
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
                    }
                    else
                    {
                        activeConnections--;
                    }

                    LockCheck();

                    connectionStatus[x] = !connectionStatus[x];
                }

                x++;
            }
        }
    
    #endregion
    
    #region Custom Methods
        
        private void LockCheck()
        {
            if(activeConnections == connections.Length)
            {
                locked = false;
            }
            else
            {
                locked = true;
            }
        }

        public void UnlockBlocker()
        {
            node.UnlockNode();
            Destroy(gameObject);
        }

        public void LockedAnimation()
        {
            node.LockedAnimation(animationDuration, animationCurve);
        }
    
    #endregion
}