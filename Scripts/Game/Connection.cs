using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection : MonoBehaviour
{
    #region Variables
    
        #region Inspector
        
            public bool active = false;

            public Node firstNode;
            public Node secondNode;
            public Node transmitter;

            [Header("DEBUG")]
            public bool connected = false;
            public bool looped = false;
            public List<Node> connectedNodes = new List<Node>();
            public bool recentlyActivated = false;
            public bool recentlyDeactivated = false;
            public bool semiActive = false;
            public bool lastConnection = false;
            public bool alternatePath = false;
        
        #endregion
    
        #region DEBUG

            private float activeTransparency;
            private float inactiveTransparency;

            private float transitionDuration;
            private AnimationCurve transitionCurve;

            private bool skipNode = false;
            private bool isFullyConnected = false;
        
        #endregion
    
        #region Components
        
            private Material activeMaterial;
            private Material inactiveMaterial;
            private Material semiActiveMaterial;

            private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        
        #endregion
    
    #endregion
    
    #region Built-in Methods
    
        public void ForcedAwake()
        {
            activeMaterial = LevelManager.instance.activeConnection;
            inactiveMaterial = LevelManager.instance.inactiveConnection;
            semiActiveMaterial = LevelManager.instance.semiActiveConnection;

            if(GetComponent<MeshRenderer>())
            {
                meshRenderers.Add(GetComponent<MeshRenderer>());
            }

            for(int i = 0; i < transform.childCount; i++)
            {
                meshRenderers.Add(transform.GetChild(i).GetComponent<MeshRenderer>());
            }

            transitionDuration = LevelManager.instance.transitionDuration;
            transitionCurve = LevelManager.instance.transitionCurve;
        }
    
    #endregion
    
    #region Custom Methods

        public void ChangeState(Node currentNode, bool pathCheckComplete)
        {
            Debug.Log("Connection :: " + name + " :: ChangeState() :: currentNode = " + currentNode);

            if(!transmitter || currentNode == transmitter)
            {
                transmitter = currentNode;
                connectedNodes = new List<Node>(currentNode.connectedNodes);

                if(!active)
                {
                    active = true;
                    recentlyActivated = true;
                    currentNode.activeConnections.Add(this);
                }
                else
                {
                    active = false;
                    recentlyDeactivated = true;
                    currentNode.activeConnections.Remove(this);
                    connectedNodes.Clear();
                }

                if(!lastConnection)
                {
                    if((transmitter == firstNode) && secondNode)
                    {
                        skipNode = (looped && secondNode.looped) ? true : false;

                        if(active)
                        {
                            LoopCheck(secondNode);

                            connectedNodes.Add(secondNode);
                            connected = true;
                            secondNode.activeConnections.Add(this);
                        }
                        else
                        {
                            Debug.Log("Connection :: " + name + " :: looped = false :: 1");

                            looped = false;
                            secondNode.looped = false;
                            connected = false;
                            secondNode.activeConnections.Remove(this);
                        }

                        if(!skipNode && LevelManager.instance.rotatingNode != secondNode)
                        {
                            secondNode.ForcedUpdate(this, pathCheckComplete);
                        }

                        isFullyConnected = SemiActiveConnectionCheck(secondNode) ? true : false;

                        Debug.Log("Connection :: " + name + " :: ChangeMaterial() 1 :: isFullyConnected = " + isFullyConnected + " :: active = " + active);
                        StartCoroutine(ChangeMaterial(active, isFullyConnected));
                    }
                    else if((transmitter == secondNode) && firstNode)
                    {
                        skipNode = (looped && firstNode.looped) ? true : false;

                        if(active)
                        {
                            LoopCheck(firstNode);

                            connectedNodes.Add(firstNode);
                            connected = true;
                            firstNode.activeConnections.Add(this);
                        }
                        else
                        {
                            Debug.Log("Connection :: " + name + " :: looped = false :: 2");

                            looped = false;
                            firstNode.looped = false;
                            connected = false;
                            firstNode.activeConnections.Remove(this);
                        }

                        if(!skipNode && LevelManager.instance.rotatingNode != firstNode)
                        {
                            firstNode.ForcedUpdate(this, pathCheckComplete);
                        }

                        isFullyConnected = SemiActiveConnectionCheck(firstNode) ? true : false;

                        Debug.Log("Connection :: " + name + " :: ChangeMaterial() 2 :: isFullyConnected = " + isFullyConnected + " :: active = " + active);
                        StartCoroutine(ChangeMaterial(active, isFullyConnected));
                    }

                    if(!active)
                    {
                        transmitter = null;
                    }
                }
                else
                {
                    if(active)
                    {
                        connectedNodes.Add(secondNode);
                        connected = true;
                    }
                    else
                    {
                        connected = false;
                    }

                    Debug.Log("Connection :: " + name + " :: ChangeMaterial() FINAL :: isFullyConnected = TRUE");
                    StartCoroutine(ChangeMaterial(active, true));
                }
            }
        }

        private void LoopCheck(Node nodeToCheck)
        {
            if(nodeToCheck.connectedNodes.Count != 0)
            {
                Debug.Log("Connection :: " + name + " :: LoopCheck() :: " + nodeToCheck + ".connectedNodes.Count != 0");

                foreach(Node node in connectedNodes)
                {
                    if(node == nodeToCheck)
                    {
                        looped = true;
                        nodeToCheck.looped = true;
                    }
                }

                if(SemiActiveConnectionCheck(nodeToCheck))
                {
                    Debug.Log("Node :: T50 :: LevelManager.instance.FEEDBACKLOOP = true");
                    LevelManager.instance.feedbackLoop = true;
                }
                else
                {
                    Debug.Log("Node :: F50 :: LevelManager.instance.FEEDBACKLOOP = false");
                    LevelManager.instance.feedbackLoop = false;
                }
            }
            else
            {
                looped = false;
                nodeToCheck.looped = false;

                Debug.Log("Node :: F60 :: LevelManager.instance.FEEDBACKLOOP = false");
                LevelManager.instance.feedbackLoop = false;
            }
        }

        public void ConnectSource(Node rotatingNode)
        {
            connectedNodes.Add(firstNode);
            connected = true;

            firstNode.activeConnections.Add(this);

            firstNode.UpdateConnections(connectedNodes, rotatingNode, this);
        }

        public void CheckPath(List<Node> previousPath, List<Node> currentPath, Node rotatingNode, Connection connection)
        {
            if(connection == null)
            {
                connection = this;
            }

            Debug.Log("Connection :: " + name + " :: CheckPath()");
            alternatePath = true;

            if((transmitter == firstNode) && secondNode)
            {
                secondNode.CheckPath(previousPath, currentPath, rotatingNode, this, connection);
            }
            else if((transmitter == secondNode) && firstNode)
            {
                firstNode.CheckPath(previousPath, currentPath, rotatingNode, this, connection);
            }
        }
        
        public void ConnectNodes(Node currentNode, Node rotatingNode)
        {
            Debug.Log("Connection :: " + name + " :: ConnectNodes() :: currentNode.connectedNodes.Count = " + currentNode.connectedNodes.Count);
            connectedNodes = new List<Node>(currentNode.connectedNodes);

            if((transmitter == firstNode) && secondNode)
            {
                if(secondNode.connectedNodes.Count != 0)
                {
                    looped = true;
                    secondNode.looped = true;
                }
                else
                {
                    looped = false;
                    secondNode.looped = false;
                }
                
                connectedNodes.Add(secondNode);
                connected = true;

                currentNode.activeConnections.Add(this);
                secondNode.activeConnections.Add(this);

                if(!(secondNode.looped && looped))
                {
                    secondNode.UpdateConnections(connectedNodes, rotatingNode, this);
                }
                else
                {
                    Debug.Log("Connection :: " + name + " :: CheckPath() :: END PATH");
                }
                
                if(active && !semiActive && !SemiActiveConnectionCheck(secondNode))
                {
                    Debug.Log("Connection :: " + name + " :: ChangeMaterial() 3 :: (true, false)");
                    StartCoroutine(ChangeMaterial(true, false));
                }
            }
            else if((transmitter == secondNode) && firstNode)
            {
                if(firstNode.connectedNodes.Count != 0)
                {
                    looped = true;
                    firstNode.looped = true;
                }
                else
                {
                    looped = false;
                    firstNode.looped = false;
                }
                
                connectedNodes.Add(firstNode);
                connected = true;

                currentNode.activeConnections.Add(this);
                firstNode.activeConnections.Add(this);

                if(!(firstNode.looped && looped))
                {
                    firstNode.UpdateConnections(connectedNodes, rotatingNode, this);
                }
                else
                {
                    Debug.Log("Connection :: " + name + " :: CheckPath() :: END PATH");
                }

                if(active && !semiActive && !SemiActiveConnectionCheck(firstNode))
                {
                    Debug.Log("Connection :: " + name + " :: ChangeMaterial() 4 :: (true, false)");
                    StartCoroutine(ChangeMaterial(true, false));
                }
            }
        }

        private bool SemiActiveConnectionCheck(Node node)
        {
            if(node.frontConnection == this)
            {
                if(node.nodeType == Node.NodeType.I) 
                {
                    if(node.currentOrientation == Node.Orientation.Forward || node.currentOrientation == Node.Orientation.Backward)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.L)
                {
                    if(node.currentOrientation == Node.Orientation.Forward || node.currentOrientation == Node.Orientation.Left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.T)
                {
                    if(node.currentOrientation != Node.Orientation.Forward)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.X)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(node.rightConnection == this)
            {
                if(node.nodeType == Node.NodeType.I) 
                {
                    if(node.currentOrientation == Node.Orientation.Right || node.currentOrientation == Node.Orientation.Left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.L)
                {
                    if(node.currentOrientation == Node.Orientation.Forward || node.currentOrientation == Node.Orientation.Right)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.T)
                {
                    if(node.currentOrientation != Node.Orientation.Right)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.X)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(node.backConnection == this)
            {
                if(node.nodeType == Node.NodeType.I) 
                {
                    if(node.currentOrientation == Node.Orientation.Forward || node.currentOrientation == Node.Orientation.Backward)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.L)
                {
                    if(node.currentOrientation == Node.Orientation.Right || node.currentOrientation == Node.Orientation.Backward)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.T)
                {
                    if(node.currentOrientation != Node.Orientation.Backward)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.X)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(node.leftConnection == this)
            {
                if(node.nodeType == Node.NodeType.I) 
                {
                    if(node.currentOrientation == Node.Orientation.Right || node.currentOrientation == Node.Orientation.Left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.L)
                {
                    if(node.currentOrientation == Node.Orientation.Backward || node.currentOrientation == Node.Orientation.Left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.T)
                {
                    if(node.currentOrientation != Node.Orientation.Left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(node.nodeType == Node.NodeType.X)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void ActivateConnection()
        {
            StartCoroutine(ChangeMaterial(true, true));
        }

        public void SemiActivateConnection()
        {
            StartCoroutine(ChangeMaterial(true, false));
        }
    
    #endregion

    #region Coroutines

        private IEnumerator ChangeMaterial(bool activate, bool fullyConnected)
        {
            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / transitionDuration;

            Material currentMaterial = meshRenderers[0].material;

            if(activate && !fullyConnected)
            {
                semiActive = true;
            }
            else
            {
                semiActive = false;
            }

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = transitionCurve.Evaluate(transitionPercentage);

                if(activate)
                {
                    if(fullyConnected)
                    {
                        foreach(MeshRenderer meshRenderer in meshRenderers)
                        {
                            meshRenderer.material.color = Color.Lerp(currentMaterial.color, activeMaterial.color, smoothTransitionPercentage);
                        }
                    }
                    else
                    {
                        foreach(MeshRenderer meshRenderer in meshRenderers)
                        {
                            meshRenderer.material.color = Color.Lerp(currentMaterial.color, semiActiveMaterial.color, smoothTransitionPercentage);
                        }
                    }
                }
                else
                {
                    foreach(MeshRenderer meshRenderer in meshRenderers)
                    {
                        meshRenderer.material.color = Color.Lerp(currentMaterial.color, inactiveMaterial.color, smoothTransitionPercentage);
                    }
                }

                yield return null;
            }
        }

    #endregion
}