using UnityEngine;

public class Source : MonoBehaviour
{
    #region Variables
        
        #region Inspector
        
            public Connection[] connections;
            public Node[] allNodes;
            public Connection[] allConnections;
        
        #endregion

        #region DEBUG
        
            public Node node;
        
        #endregion
    
    #endregion
    
    #region Built-in Methods
        
        private void Start()
        {
            node = GetComponent<Node>();
            
            foreach(Connection connection in connections)
            {
                if(!connection.active)
                {
                    connection.ChangeState(null, false);
                }

                connection.connectedNodes.Add(node);
                connection.ConnectSource(null);
            }
        }
    
    #endregion

    #region Custom Methods

        public void RecalculatePath(Node rotatingNode)
        {
            ResetAllNodes();

            foreach(Connection connection in connections)
            {
                connection.connectedNodes.Add(node);
                connection.ConnectSource(rotatingNode);
            }
        }

        private void ResetAllNodes()
        {
            foreach(Node node in allNodes)
            {
                node.connectedNodes.Clear();
                node.activeConnections.Clear();
            }

            foreach(Connection connection in allConnections)
            {
                connection.connectedNodes.Clear();
                connection.connected = false;
                connection.recentlyActivated = false;
                connection.recentlyDeactivated = false;
                connection.alternatePath = false;
            }
        }

        public void ResetConnections()
        {
            foreach(Connection connection in allConnections)
            {
                connection.connected = false;
                connection.recentlyActivated = false;
                connection.recentlyDeactivated = false;
                connection.alternatePath = false;
            }
        }

        public void SetupAllConnections()
        {
            foreach(Connection connection in allConnections)
            {
                connection.ForcedAwake();
            }
        }

    #endregion
}