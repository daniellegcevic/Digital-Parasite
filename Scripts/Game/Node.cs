using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class Node : MonoBehaviour
{
    #region Variables
    
        #region Inspector
        
            public Orientation currentOrientation;

            public enum Orientation
            {
                Forward,
                Right,
                Backward,
                Left
            }

            public NodeType nodeType;

            public Source source;

            public enum NodeType
            {
                I,
                L,
                T,
                X
            }

            [Header("Connections")]
            public Connection frontConnection;
            public Connection rightConnection;
            public Connection backConnection;
            public Connection leftConnection;

            [Header("Lock")]
            public Blocker blocker;

            [Header("DEBUG")]
            public bool rotating = false;
            public bool locked = false;
            public bool looped = false;
            public List<Connection> activeConnections = new List<Connection>();
            public List<Node> connectedNodes = new List<Node>();
            public bool currentPathLooped = false;
            public bool currentPathConnected = false;
            public bool pathComplete = false;

        #endregion
    
        #region DEBUG
        
            private bool frontConnectionStatus;
            private bool rightConnectionStatus;
            private bool backConnectionStatus;
            private bool leftConnectionStatus;

            private Transmitter firstTransmitter;
            private Transmitter secondTransmitter;

            private enum Transmitter
            {
                Front,
                Right,
                Back,
                Left,
                None
            }

            private float rotationDuration;
            private AnimationCurve animationCurve;

            private Material unlockedMaterial;
            private Material lockedMaterial;
        
        #endregion

        #region Components

            private MeshRenderer meshRenderer;

        #endregion
    
    #endregion
    
    #region Built-in Methods
        
        private void Start()
        {
            rotationDuration = LevelManager.instance.rotationDuration;
            animationCurve = LevelManager.instance.animationCurve;
            meshRenderer = GetComponent<MeshRenderer>();

            unlockedMaterial = LevelManager.instance.unlockedNode;
            lockedMaterial = LevelManager.instance.lockedNode;

            StatusCheck();
            
            LocateTransmitters();
        }
    
    #endregion
    
    #region Custom Methods
        
        public void CheckPath(List<Node> previousPath, List<Node> currentPath, Node rotatingNode, Connection previousConnection, Connection connection)
        {
            Transmitter previousTransmitter = Transmitter.None;

            if(previousConnection)
            {
                if(previousConnection == frontConnection)
                {
                    previousTransmitter = Transmitter.Front;
                }
                else if(previousConnection == rightConnection)
                {
                    previousTransmitter = Transmitter.Right;
                }
                else if(previousConnection == backConnection)
                {
                    previousTransmitter = Transmitter.Back;
                }
                else if(previousConnection == leftConnection)
                {
                    previousTransmitter = Transmitter.Left;
                }
            }

            currentPathLooped = false;
            currentPathConnected = false;
            LevelManager.instance.feedbackLoopWrongPath = false;

            if(currentPath == null)
            {
                currentPath = new List<Node>();
            }

            foreach(Node node in currentPath)
            {
                Debug.Log("Node: " + node.name);
                if(node == this)
                {
                    Debug.Log("Node :: " + name + " :: currentPathLooped = True :: END PATH CHECK");
                    currentPathLooped = true;

                    if(SemiActiveConnectionCheck(GetTransimtter(previousConnection)))
                    {
                        currentPathConnected = true;
                    }

                    previousConnection.looped = true;

                    if(SemiActiveConnectionCheck(previousTransmitter))
                    {
                        Debug.Log("Node :: T10 :: LevelManager.instance.FEEDBACKLOOP = true");
                        LevelManager.instance.feedbackLoop = true;
                    }
                    else
                    {
                        Debug.Log("Node :: F10 :: LevelManager.instance.FEEDBACKLOOP = false");
                        LevelManager.instance.feedbackLoop = false;
                    }

                    rotatingNode.UpdateConnections(previousPath, rotatingNode, null);
                }
            }

            currentPath.Add(this);

            if(!currentPathLooped)
            {
                if(rotatingNode != this)
                {
                    foreach(Node node in previousPath)
                    {
                        if(node == this)
                        {
                            Debug.Log("Node :: " + name + " :: feedbackLoopWrongPath = True :: END PATH CHECK");
                            LevelManager.instance.feedbackLoopWrongPath = true;
                            connection.looped = true;
                        }
                    }
                }
                
                if(!LevelManager.instance.feedbackLoopWrongPath)
                {
                    if(frontConnection && frontConnection.active && !frontConnection.connected && !frontConnection.alternatePath && SemiActiveConnectionCheck(Transmitter.Front))
                    {
                        frontConnection.transmitter = this;
                        frontConnection.CheckPath(previousPath, currentPath, rotatingNode, connection);
                    }

                    if(rightConnection && rightConnection.active && !rightConnection.connected && !rightConnection.alternatePath && SemiActiveConnectionCheck(Transmitter.Right))
                    {
                        rightConnection.transmitter = this;
                        rightConnection.CheckPath(previousPath, currentPath, rotatingNode, connection);
                    }

                    if(backConnection && backConnection.active && !backConnection.connected && !backConnection.alternatePath && SemiActiveConnectionCheck(Transmitter.Back))
                    {
                        backConnection.transmitter = this;
                        backConnection.CheckPath(previousPath, currentPath, rotatingNode, connection);
                    }
                    
                    if(leftConnection && leftConnection.active && !leftConnection.connected && !leftConnection.alternatePath && SemiActiveConnectionCheck(Transmitter.Left))
                    {
                        leftConnection.transmitter = this;
                        leftConnection.CheckPath(previousPath, currentPath, rotatingNode, connection);
                    }
                }
            }
        }

        private Transmitter GetTransimtter(Connection previousConnection)
        {
            Transmitter previousTransmitter = Transmitter.None;

            if(previousConnection)
            {
                if(previousConnection == frontConnection)
                {
                    previousTransmitter = Transmitter.Front;
                }
                else if(previousConnection == rightConnection)
                {
                    previousTransmitter = Transmitter.Right;
                }
                else if(previousConnection == backConnection)
                {
                    previousTransmitter = Transmitter.Back;
                }
                else if(previousConnection == leftConnection)
                {
                    previousTransmitter = Transmitter.Left;
                }
            }

            return previousTransmitter;
        }
        
        public void UpdateConnections(List<Node> nodes, Node rotatingNode, Connection previousConnection)
        {
            Transmitter previousTransmitter = Transmitter.None;

            previousTransmitter = GetTransimtter(previousConnection);

            if(SemiActiveConnectionCheck(previousTransmitter))
            {
                connectedNodes = new List<Node>(nodes);

                Debug.Log("Node :: " + name + " :: UpdateConnections() :: connectedNodes.Count = " + connectedNodes.Count);
            }

            if(rotatingNode == this && LevelManager.instance.feedbackLoop && SemiActiveConnectionCheck(previousTransmitter))
            {
                Debug.Log("Node :: " + name + " :: CheckPath() :: START PATH CHECK");
                CheckPath(nodes, null, rotatingNode, previousConnection, null);
            }
            else if(SemiActiveConnectionCheck(previousTransmitter))
            {
                if(frontConnection && frontConnection.active && !frontConnection.connected && (LevelManager.instance.feedbackLoopWrongPath == true || !(looped && frontConnection.looped)))
                {
                    if(SemiActiveConnectionCheck(Transmitter.Front))
                    {
                        frontConnection.transmitter = this;
                        frontConnection.ConnectNodes(this, rotatingNode);
                    }
                    else
                    {
                        Debug.Log("Node :: " + name + " :: CheckPath() :: START PATH CHECK :: Front Connection Check");
                        CheckPath(nodes, null, rotatingNode, previousConnection, frontConnection);
                    }
                }

                if(rightConnection && rightConnection.active && !rightConnection.connected && (LevelManager.instance.feedbackLoopWrongPath == true || !(looped && rightConnection.looped)))
                {
                    if(SemiActiveConnectionCheck(Transmitter.Right))
                    {
                        rightConnection.transmitter = this;
                        rightConnection.ConnectNodes(this, rotatingNode);
                    }
                    else
                    {
                        Debug.Log("Node :: " + name + " :: CheckPath() :: START PATH CHECK :: Right Connection Check");
                        CheckPath(nodes, null, rotatingNode, previousConnection, rightConnection);
                    }
                }

                if(backConnection && backConnection.active && !backConnection.connected && (LevelManager.instance.feedbackLoopWrongPath == true || !(looped && backConnection.looped)))
                {
                    if(SemiActiveConnectionCheck(Transmitter.Back))
                    {
                        backConnection.transmitter = this;
                        backConnection.ConnectNodes(this, rotatingNode);
                    }
                    else
                    {
                        Debug.Log("Node :: " + name + " :: CheckPath() :: START PATH CHECK :: Back Connection Check");
                        CheckPath(nodes, null, rotatingNode, previousConnection, backConnection);
                    }
                }
                
                if(leftConnection && leftConnection.active && !leftConnection.connected && (LevelManager.instance.feedbackLoopWrongPath == true || !(looped && leftConnection.looped)))
                {
                    if(SemiActiveConnectionCheck(Transmitter.Left))
                    {
                        leftConnection.transmitter = this;
                        leftConnection.ConnectNodes(this, rotatingNode);
                    }
                    else
                    {
                        Debug.Log("Node :: " + name + " :: CheckPath() :: START PATH CHECK :: Left Connection Check");
                        CheckPath(nodes, null, rotatingNode, previousConnection, leftConnection);
                    }
                } 
            }
            else
            {
                Debug.Log("Node :: " + name + " :: CheckPath() :: END PATH");
            }
        }

        public void ForcedUpdate(Connection connection, bool pathCheckComplete)
        {
            pathComplete = pathCheckComplete;

            if(!connection.looped)
            {
                connectedNodes = new List<Node>(connection.connectedNodes);
            }
            else
            {
                Debug.Log("LOOPED");
            }

            Debug.Log("Node :: " + name + " :: ForcedUpdate()");

            UpdateNode();
        }
        
        public void UpdateNode()
        {
            if(!locked)
            {
                if(frontConnection && frontConnectionStatus != frontConnection.active && !rotating) // FRONT CONNECTION CHECK
                {
                    if(!frontConnection.looped)
                    {
                        if(nodeType == NodeType.I) 
                        {
                            if(currentOrientation == Orientation.Forward || currentOrientation == Orientation.Backward)
                            {
                                BackConnectionChange(frontConnection);
                            }
                        }
                        else if(nodeType == NodeType.L)
                        {
                            if(currentOrientation == Orientation.Forward)
                            {
                                if(rightConnection)
                                {
                                    RightConnectionChange(frontConnection);
                                }
                            }
                            else if(currentOrientation == Orientation.Left)
                            {
                                LeftConnectionChange(frontConnection);
                            }
                        }
                        else if(nodeType == NodeType.T)
                        {
                            if(currentOrientation == Orientation.Right)
                            {
                                if(backConnection)
                                {
                                    BackConnectionChange(frontConnection);
                                }
                                
                                if(leftConnection)
                                {
                                    LeftConnectionChange(frontConnection);
                                }
                            }
                            else if(currentOrientation == Orientation.Backward)
                            {
                                RightConnectionChange(frontConnection);
                                LeftConnectionChange(frontConnection);
                            }
                            else if(currentOrientation == Orientation.Left)
                            {
                                RightConnectionChange(frontConnection);
                                BackConnectionChange(frontConnection);
                            }
                        }
                    }

                    frontConnectionStatus = !frontConnectionStatus;
                }
                else if(rightConnection && rightConnectionStatus != rightConnection.active && !rotating) // RIGHT CONNECTION CHECK
                {
                    if(!rightConnection.looped)
                    {
                        if(nodeType == NodeType.I)
                        {
                            if(currentOrientation == Orientation.Right || currentOrientation == Orientation.Left)
                            {
                                LeftConnectionChange(rightConnection);
                            }
                        }
                        else if(nodeType == NodeType.L)
                        {
                            if(currentOrientation == Orientation.Forward)
                            {
                                FrontConnectionChange(rightConnection);
                            }
                            else if(currentOrientation == Orientation.Right)
                            {
                                BackConnectionChange(rightConnection);
                            }
                        }
                        else if(nodeType == NodeType.T)
                        {
                            if(currentOrientation == Orientation.Forward)
                            {
                                BackConnectionChange(rightConnection);
                                LeftConnectionChange(rightConnection);
                            }
                            else if(currentOrientation == Orientation.Backward)
                            {
                                LeftConnectionChange(rightConnection);
                                FrontConnectionChange(rightConnection);
                            }
                            else if(currentOrientation == Orientation.Left)
                            {
                                FrontConnectionChange(rightConnection);
                                BackConnectionChange(rightConnection);
                            }
                        }
                    }

                    rightConnectionStatus = !rightConnectionStatus;
                }
                else if(backConnection && backConnectionStatus != backConnection.active && !rotating) // BACK CONNECTION CHECK
                {
                    if(!backConnection.looped)
                    {
                        if(nodeType == NodeType.I)
                        {
                            if(currentOrientation == Orientation.Forward || currentOrientation == Orientation.Backward)
                            {
                                FrontConnectionChange(backConnection);
                            }
                        }
                        else if(nodeType == NodeType.L)
                        {
                            if(currentOrientation == Orientation.Right)
                            {
                                RightConnectionChange(backConnection);
                            }
                            else if(currentOrientation == Orientation.Backward)
                            {
                                LeftConnectionChange(backConnection);
                            }
                        }
                        else if(nodeType == NodeType.T)
                        {
                            if(currentOrientation == Orientation.Forward)
                            {
                                RightConnectionChange(backConnection);
                                LeftConnectionChange(backConnection);
                            }
                            else if(currentOrientation == Orientation.Right)
                            {
                                LeftConnectionChange(backConnection);
                                FrontConnectionChange(backConnection);
                            }
                            else if(currentOrientation == Orientation.Left)
                            {
                                FrontConnectionChange(backConnection);
                                RightConnectionChange(backConnection);
                            }
                        }
                    }
                    
                    backConnectionStatus = !backConnectionStatus;
                }
                else if(leftConnection && leftConnectionStatus != leftConnection.active && !rotating) // LEFT CONNECTION CHECK
                {
                    if(!leftConnection.looped)
                    {
                        if(nodeType == NodeType.I)
                        {
                            if(currentOrientation == Orientation.Right || currentOrientation == Orientation.Left)
                            {
                                RightConnectionChange(leftConnection);
                            }
                        }
                        else if(nodeType == NodeType.L)
                        {
                            if(currentOrientation == Orientation.Backward)
                            {
                                BackConnectionChange(leftConnection);
                            }
                            else if(currentOrientation == Orientation.Left)
                            {
                                FrontConnectionChange(leftConnection);
                            }
                        }
                        else if(nodeType == NodeType.T)
                        {
                            if(currentOrientation == Orientation.Forward)
                            {
                                RightConnectionChange(leftConnection);
                                BackConnectionChange(leftConnection);
                            }
                            else if(currentOrientation == Orientation.Right)
                            {
                                FrontConnectionChange(leftConnection);
                                BackConnectionChange(leftConnection);
                            }
                            else if(currentOrientation == Orientation.Backward)
                            {
                                FrontConnectionChange(leftConnection);
                                RightConnectionChange(leftConnection);
                            }
                        }
                    }

                    leftConnectionStatus = !leftConnectionStatus;
                }
            }
        }

        public void SemiActivateConnections()
        {
            Debug.Log("Node :: " + name + " :: No Connection Change Detected");

            if(frontConnection && frontConnection.active && !frontConnection.recentlyActivated)
            {
                frontConnection.SemiActivateConnection();
            }
            else if(rightConnection && rightConnection.active && !rightConnection.recentlyActivated)
            {
                rightConnection.SemiActivateConnection();
            }
            else if(backConnection && backConnection.active && !backConnection.recentlyActivated)
            {
                backConnection.SemiActivateConnection();
            }
            else if(leftConnection && leftConnection.active && !leftConnection.recentlyActivated)
            {
                leftConnection.SemiActivateConnection();
            }
        }
        
        public void LockNode()
        {
            locked = true;
        }

        public void UnlockNode()
        {
            locked = false;
            UpdateNode();
            source.RecalculatePath(this);
            StartCoroutine(ColorChange());
        }
        
        private void FrontConnectionChange(Connection previousConnection)
        {
            if(frontConnection && frontConnection.active || previousConnection && previousConnection.active)
            {
                Debug.Log("Node :: " + name + " :: FrontConnectionChange() :: previousConnection = " + previousConnection);

                Transmitter previousTransmitter = Transmitter.None;

                previousTransmitter = GetTransimtter(previousConnection);

                bool currentConnectionIsSemiActive = false;

                bool pathCheckComplete = false;

                if(frontConnection && !frontConnection.recentlyDeactivated && !frontConnection.recentlyActivated)
                {
                    if(frontConnection.active && !pathComplete)
                    {
                        List<Node> currentPath = new List<Node>();
                        currentPath.Add(this);
                        frontConnection.CheckPath(connectedNodes, currentPath, this, frontConnection);
                        pathCheckComplete = true;
                    }

                    if(frontConnection.active && currentPathLooped && SemiActiveConnectionCheck(previousTransmitter) && currentPathConnected)
                    {
                        if(!frontConnection.semiActive)
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6X :: SemiActivateConnection() -> " + frontConnection);
                            frontConnection.SemiActivateConnection();
                        }
                        else
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6X :: ActivateConnection() -> " + frontConnection);
                            frontConnection.ActivateConnection();
                        }
                    }
                    else
                    {
                        currentConnectionIsSemiActive = frontConnection.semiActive;

                        if(!frontConnection.looped || frontConnection.transmitter == this)
                        {
                            frontConnectionStatus = !frontConnectionStatus;
                            frontConnection.ChangeState(this, pathCheckComplete);
                        }
                        else if(frontConnection.semiActive && frontConnection.transmitter != this)
                        {
                            Debug.Log("Node :: T1 :: LevelManager.instance.FEEDBACKLOOP = true");
                            LevelManager.instance.feedbackLoop = true;
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 5L :: ActivateConnection() -> " + frontConnection);
                            frontConnection.ActivateConnection();
                        }
                        else if(!SemiActiveConnectionCheck(Transmitter.Front))
                        {
                            Debug.Log("Node :: F1 :: LevelManager.instance.FEEDBACKLOOP = false");
                            LevelManager.instance.feedbackLoop = false;
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 5X :: SemiActivateConnection() -> " + frontConnection);
                            frontConnection.SemiActivateConnection();
                        }
                    }
                }

                if(!previousConnection.recentlyActivated)
                {
                    if(previousConnection.semiActive)
                    {
                        if(frontConnection && !currentConnectionIsSemiActive)
                        {
                            Debug.Log("Node :: T1b :: LevelManager.instance.FEEDBACKLOOP = true");
                            LevelManager.instance.feedbackLoop = true;
                        }

                        Debug.Log("Node :: " + name + " :: ChangeMaterial() 5A :: ActivateConnection() -> " + previousConnection);
                        previousConnection.ActivateConnection();
                    }
                    else
                    {
                        if(!SemiActiveConnectionCheck(firstTransmitter))
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 5B :: SemiActivateConnection() -> " + previousConnection);
                            previousConnection.SemiActivateConnection();
                        }

                        if(secondTransmitter != Transmitter.None && !SemiActiveConnectionCheck(secondTransmitter))
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 5C :: SemiActivateConnection() -> " + previousConnection);
                            previousConnection.SemiActivateConnection();
                        }
                    }

                    previousConnection.recentlyActivated = true;
                }
            }
        }

        private void RightConnectionChange(Connection previousConnection)
        {
            if(rightConnection && rightConnection.active || previousConnection && previousConnection.active)
            {
                Debug.Log("Node :: " + name + " :: RightConnectionChange() :: previousConnection = " + previousConnection);

                Transmitter previousTransmitter = Transmitter.None;

                previousTransmitter = GetTransimtter(previousConnection);

                bool currentConnectionIsSemiActive = false;

                bool pathCheckComplete = false;

                if(rightConnection && !rightConnection.recentlyDeactivated && !rightConnection.recentlyActivated)
                {
                    if(rightConnection.active && !pathComplete)
                    {
                        List<Node> currentPath = new List<Node>();
                        currentPath.Add(this);
                        rightConnection.CheckPath(connectedNodes, currentPath, this, rightConnection);
                        pathCheckComplete = true;
                    }

                    if(rightConnection.active && currentPathLooped && SemiActiveConnectionCheck(previousTransmitter) && currentPathConnected)
                    {
                        if(!rightConnection.semiActive)
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6X :: SemiActivateConnection() -> " + rightConnection);
                            rightConnection.SemiActivateConnection();
                        }
                        else
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6X :: ActivateConnection() -> " + rightConnection);
                            rightConnection.ActivateConnection();
                        }
                    }
                    else
                    {
                        currentConnectionIsSemiActive = rightConnection.semiActive;

                        if(!rightConnection.looped || rightConnection.transmitter == this)
                        {
                            rightConnectionStatus = !rightConnectionStatus;
                            rightConnection.ChangeState(this, pathCheckComplete);
                        }
                        else if(rightConnection.semiActive && rightConnection.transmitter != this)
                        {
                            Debug.Log("Node :: T2 :: LevelManager.instance.FEEDBACKLOOP = true");
                            LevelManager.instance.feedbackLoop = true;
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6L :: ActivateConnection() -> " + rightConnection);
                            rightConnection.ActivateConnection();
                        }
                        else if(!SemiActiveConnectionCheck(Transmitter.Right))
                        {
                            Debug.Log("Node :: F2 :: LevelManager.instance.FEEDBACKLOOP = true");
                            LevelManager.instance.feedbackLoop = false;
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6X :: SemiActivateConnection() -> " + rightConnection);
                            rightConnection.SemiActivateConnection();
                        }
                    }
                }

                if(!previousConnection.recentlyActivated)
                {
                    if(previousConnection.semiActive)
                    {
                        if(rightConnection && !currentConnectionIsSemiActive)
                        {
                            Debug.Log("Node :: T2b :: LevelManager.instance.FEEDBACKLOOP = true");
                            LevelManager.instance.feedbackLoop = true;
                        }

                        Debug.Log("Node :: " + name + " :: ChangeMaterial() 6A :: ActivateConnection() -> " + previousConnection);
                        previousConnection.ActivateConnection();
                    }
                    else
                    {
                        if(!SemiActiveConnectionCheck(firstTransmitter))
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6B :: SemiActivateConnection() -> " + previousConnection);
                            previousConnection.SemiActivateConnection();
                        }

                        if(secondTransmitter != Transmitter.None && !SemiActiveConnectionCheck(secondTransmitter))
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6C :: SemiActivateConnection() -> " + previousConnection);
                            previousConnection.SemiActivateConnection();
                        }
                    }

                    previousConnection.recentlyActivated = true;
                }
            }
        }

        private void BackConnectionChange(Connection previousConnection)
        {
            if(backConnection && backConnection.active || previousConnection && previousConnection.active)
            {
                Debug.Log("Node :: " + name + " :: BackConnectionChange() :: previousConnection = " + previousConnection);

                Transmitter previousTransmitter = Transmitter.None;

                previousTransmitter = GetTransimtter(previousConnection);

                bool currentConnectionIsSemiActive = false;

                bool pathCheckComplete = false;

                if(backConnection && !backConnection.recentlyDeactivated && !backConnection.recentlyActivated)
                {
                    if(backConnection.active && !pathComplete)
                    {
                        List<Node> currentPath = new List<Node>();
                        currentPath.Add(this);
                        backConnection.CheckPath(connectedNodes, currentPath, this, backConnection);
                        pathCheckComplete = true;
                    }

                    if(backConnection.active && currentPathLooped && SemiActiveConnectionCheck(previousTransmitter) && currentPathConnected)
                    {
                        if(!backConnection.semiActive)
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6X :: SemiActivateConnection() -> " + backConnection);
                            backConnection.SemiActivateConnection();
                        }
                        else
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6X :: ActivateConnection() -> " + backConnection);
                            backConnection.ActivateConnection();
                        }
                    }
                    else
                    {
                        currentConnectionIsSemiActive = backConnection.semiActive;

                        if(!backConnection.looped || backConnection.transmitter == this)
                        {
                            backConnectionStatus = !backConnectionStatus;
                            backConnection.ChangeState(this, pathCheckComplete);
                        }
                        else if(backConnection.semiActive && backConnection.transmitter != this)
                        {
                            Debug.Log("Node :: T3 :: LevelManager.instance.FEEDBACKLOOP = true");
                            LevelManager.instance.feedbackLoop = true;
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 7L :: ActivateConnection() -> " + backConnection);
                            backConnection.ActivateConnection();
                        }
                        else if(!SemiActiveConnectionCheck(Transmitter.Back))
                        {
                            Debug.Log("Node :: F3 :: LevelManager.instance.FEEDBACKLOOP = false");
                            LevelManager.instance.feedbackLoop = false;
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 7X :: SemiActivateConnection() -> " + backConnection);
                            backConnection.SemiActivateConnection();
                        }
                    }
                }

                if(!previousConnection.recentlyActivated)
                {
                    if(previousConnection.semiActive)
                    {
                        if(backConnection && !currentConnectionIsSemiActive)
                        {
                            Debug.Log("Node :: T3b :: LevelManager.instance.FEEDBACKLOOP = true");
                            LevelManager.instance.feedbackLoop = true;
                        }

                        Debug.Log("Node :: " + name + " :: ChangeMaterial() 7A :: ActivateConnection() -> " + previousConnection);
                        previousConnection.ActivateConnection();
                    }
                    else
                    {
                        if(!SemiActiveConnectionCheck(firstTransmitter))
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 7B :: SemiActivateConnection() -> " + previousConnection);
                            previousConnection.SemiActivateConnection();
                        }

                        if(secondTransmitter != Transmitter.None && !SemiActiveConnectionCheck(secondTransmitter))
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 7C :: SemiActivateConnection() -> " + previousConnection);
                            previousConnection.SemiActivateConnection();
                        }
                    }

                    previousConnection.recentlyActivated = true;
                }
            }
        }

        private void LeftConnectionChange(Connection previousConnection)
        {
            if(leftConnection && leftConnection.active || previousConnection && previousConnection.active)
            {
                Debug.Log("Node :: " + name + " :: LeftConnectionChange() :: previousConnection = " + previousConnection);

                Transmitter previousTransmitter = Transmitter.None;

                previousTransmitter = GetTransimtter(previousConnection);

                bool currentConnectionIsSemiActive = false;

                bool pathCheckComplete = false;

                if(leftConnection && !leftConnection.recentlyDeactivated && !leftConnection.recentlyActivated)
                {
                    if(leftConnection.active && !pathComplete)
                    {
                        List<Node> currentPath = new List<Node>();
                        currentPath.Add(this);
                        leftConnection.CheckPath(connectedNodes, currentPath, this, leftConnection);
                        pathCheckComplete = true;
                    }

                    if(leftConnection.active && currentPathLooped && SemiActiveConnectionCheck(previousTransmitter) && currentPathConnected)
                    {
                        if(!leftConnection.semiActive)
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6X :: SemiActivateConnection() -> " + leftConnection);
                            leftConnection.SemiActivateConnection();
                        }
                        else
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 6X :: ActivateConnection() -> " + leftConnection);
                            leftConnection.ActivateConnection();
                        }
                    }
                    else
                    {
                        currentConnectionIsSemiActive = leftConnection.semiActive;

                        if(!leftConnection.looped || leftConnection.transmitter == this)
                        {
                            leftConnectionStatus = !leftConnectionStatus;
                            leftConnection.ChangeState(this, pathCheckComplete);
                        }
                        else if(leftConnection.semiActive && leftConnection.transmitter != this)
                        {
                            Debug.Log("Node :: T4 :: LevelManager.instance.FEEDBACKLOOP = true");
                            LevelManager.instance.feedbackLoop = true;
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 8L :: ActivateConnection() -> " + leftConnection);
                            leftConnection.ActivateConnection();
                        }
                        else if(!SemiActiveConnectionCheck(Transmitter.Left))
                        {
                            Debug.Log("Node :: F4 :: LevelManager.instance.FEEDBACKLOOP = false");
                            LevelManager.instance.feedbackLoop = false;
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 8X :: SemiActivateConnection() -> " + leftConnection);
                            leftConnection.SemiActivateConnection();
                        }
                    }
                }

                if(!previousConnection.recentlyActivated)
                {
                    if(previousConnection.semiActive)
                    {
                        if(leftConnection && !currentConnectionIsSemiActive)
                        {
                            Debug.Log("Node :: T4b :: LevelManager.instance.FEEDBACKLOOP = true");
                            LevelManager.instance.feedbackLoop = true;
                        }

                        Debug.Log("Node :: " + name + " :: ChangeMaterial() 8A :: ActivatePreviousConnection() -> " + previousConnection);
                        previousConnection.ActivateConnection();
                    }
                    else
                    {
                        if(!SemiActiveConnectionCheck(firstTransmitter))
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 8B :: SemiActivatePreviousConnection() -> " + previousConnection);
                            previousConnection.SemiActivateConnection();
                        }

                        if(secondTransmitter != Transmitter.None && !SemiActiveConnectionCheck(secondTransmitter))
                        {
                            Debug.Log("Node :: " + name + " :: ChangeMaterial() 8C :: SemiActivatePreviousConnection() -> " + previousConnection);
                            previousConnection.SemiActivateConnection();
                        }
                    }

                    previousConnection.recentlyActivated = true;
                }
            }
        }

        private bool SemiActiveConnectionCheck(Transmitter transmitter)
        {
            if(transmitter == Transmitter.Front)
            {
                if(nodeType == NodeType.I) 
                {
                    if(currentOrientation == Orientation.Forward || currentOrientation == Orientation.Backward)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.L)
                {
                    if(currentOrientation == Orientation.Forward || currentOrientation == Orientation.Left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.T)
                {
                    if(currentOrientation != Orientation.Forward)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.X)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(transmitter == Transmitter.Right)
            {
                if(nodeType == NodeType.I) 
                {
                    if(currentOrientation == Orientation.Right || currentOrientation == Orientation.Left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.L)
                {
                    if(currentOrientation == Orientation.Forward || currentOrientation == Orientation.Right)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.T)
                {
                    if(currentOrientation != Orientation.Right)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.X)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(transmitter == Transmitter.Back)
            {
                if(nodeType == NodeType.I) 
                {
                    if(currentOrientation == Orientation.Forward || currentOrientation == Orientation.Backward)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.L)
                {
                    if(currentOrientation == Orientation.Right || currentOrientation == Orientation.Backward)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.T)
                {
                    if(currentOrientation != Orientation.Backward)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.X)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(transmitter == Transmitter.Left)
            {
                if(nodeType == NodeType.I) 
                {
                    if(currentOrientation == Orientation.Right || currentOrientation == Orientation.Left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.L)
                {
                    if(currentOrientation == Orientation.Backward || currentOrientation == Orientation.Left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.T)
                {
                    if(currentOrientation != Orientation.Left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(nodeType == NodeType.X)
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

        private void StatusCheck()
        {
            frontConnectionStatus = frontConnection && frontConnection.active ? true : false;
            rightConnectionStatus = rightConnection && rightConnection.active ? true : false;
            backConnectionStatus = backConnection && backConnection.active ? true : false;
            leftConnectionStatus = leftConnection && leftConnection.active ? true : false;
        }

        private void LocateTransmitters()
        {
            if(frontConnection && frontConnection.active && frontConnection.transmitter != this && !frontConnection.looped)
            {
                firstTransmitter = Transmitter.Front;
            }
            else if(rightConnection && rightConnection.active && rightConnection.transmitter != this && !rightConnection.looped)
            {
                firstTransmitter = Transmitter.Right;
            }
            else if(backConnection && backConnection.active && backConnection.transmitter != this && !backConnection.looped)
            {
                firstTransmitter = Transmitter.Back;
            }
            else if(leftConnection && leftConnection.active && leftConnection.transmitter != this && !leftConnection.looped)
            {
                firstTransmitter = Transmitter.Left;
            }
            else
            {
                firstTransmitter = Transmitter.None;
            }

            Debug.Log("Node :: " + name + " :: LocateTransmitters() :: firstTransmitter = " + firstTransmitter);

            if(frontConnection && frontConnection.active && frontConnection.transmitter != this && !frontConnection.looped && firstTransmitter != Transmitter.Front)
            {
                secondTransmitter = Transmitter.Front;
            }
            else if(rightConnection && rightConnection.active && rightConnection.transmitter != this && !rightConnection.looped && firstTransmitter != Transmitter.Right)
            {
                secondTransmitter = Transmitter.Right;
            }
            else if(backConnection && backConnection.active && backConnection.transmitter != this && !backConnection.looped && firstTransmitter != Transmitter.Back)
            {
                secondTransmitter = Transmitter.Back;
            }
            else if(leftConnection && leftConnection.active && leftConnection.transmitter != this && !leftConnection.looped && firstTransmitter != Transmitter.Left)
            {
                secondTransmitter = Transmitter.Left;
            }
            else
            {
                secondTransmitter = Transmitter.None;
            }

            Debug.Log("Node :: " + name + " :: LocateTransmitters() :: secondTransmitter = " + secondTransmitter);
        }

        public void RotateNode()
        {
            StartCoroutine(RotateNodeCoroutine());
        }

        public void LockedAnimation(float duration, AnimationCurve curve)
        {
            StartCoroutine(LockedRotation(duration, curve));
        }

        private void ConnectionCheck(Transmitter transmitter)
        {
            if(transmitter == Transmitter.Front) // FRONT CONNECTION CHECK
            {
                if(nodeType == NodeType.I)
                {
                    BackConnectionChange(frontConnection);
                }
                else if(nodeType == NodeType.L) 
                {
                    if(currentOrientation == Orientation.Forward)
                    {
                        LeftConnectionChange(frontConnection);
                        RightConnectionChange(frontConnection);
                    }
                    else if(currentOrientation == Orientation.Right)
                    {
                        RightConnectionChange(frontConnection);
                    }
                    else if(currentOrientation == Orientation.Left)
                    {
                        LeftConnectionChange(frontConnection);
                    }
                }
                else if(nodeType == NodeType.T)
                {
                    if(currentOrientation == Orientation.Forward)
                    {
                        RightConnectionChange(frontConnection);
                        BackConnectionChange(frontConnection);
                    }
                    else if(currentOrientation == Orientation.Right)
                    {
                        BackConnectionChange(frontConnection);
                        LeftConnectionChange(frontConnection);
                    }
                    else if(currentOrientation == Orientation.Backward)
                    {
                        BackConnectionChange(frontConnection);
                        RightConnectionChange(frontConnection);
                    }
                    else if(currentOrientation == Orientation.Left)
                    {
                        LeftConnectionChange(frontConnection);
                        BackConnectionChange(frontConnection);
                    }
                }
            }
            else if(transmitter == Transmitter.Right) // RIGHT CONNECTION CHECK
            {
                if(nodeType == NodeType.I)
                {
                    LeftConnectionChange(rightConnection);
                }
                else if(nodeType == NodeType.L) 
                {
                    if(currentOrientation == Orientation.Forward)
                    {
                        FrontConnectionChange(rightConnection);
                    }
                    else if(currentOrientation == Orientation.Right)
                    {
                        FrontConnectionChange(rightConnection);
                        BackConnectionChange(rightConnection);
                    }
                    else if(currentOrientation == Orientation.Backward)
                    {
                        BackConnectionChange(rightConnection);
                    }
                }
                else if(nodeType == NodeType.T)
                {
                    if(currentOrientation == Orientation.Forward)
                    {
                        FrontConnectionChange(rightConnection);
                        LeftConnectionChange(rightConnection);
                    }
                    else if(currentOrientation == Orientation.Right)
                    {
                        BackConnectionChange(rightConnection);
                        LeftConnectionChange(rightConnection);
                    }
                    else if(currentOrientation == Orientation.Backward)
                    {
                        FrontConnectionChange(rightConnection);
                        LeftConnectionChange(rightConnection);
                    }
                    else if(currentOrientation == Orientation.Left)
                    {
                        LeftConnectionChange(rightConnection);
                        BackConnectionChange(rightConnection);
                    }
                }
            }
            else if(transmitter == Transmitter.Back) // BACK CONNECTION CHECK
            {
                if(nodeType == NodeType.I)
                {
                    FrontConnectionChange(backConnection);
                }
                else if(nodeType == NodeType.L) 
                {
                    if(currentOrientation == Orientation.Right)
                    {
                        RightConnectionChange(backConnection);
                    }
                    else if(currentOrientation == Orientation.Backward)
                    {
                        RightConnectionChange(backConnection);
                        LeftConnectionChange(backConnection);
                    }
                    else if(currentOrientation == Orientation.Left)
                    {
                        LeftConnectionChange(backConnection);
                    }
                }
                else if(nodeType == NodeType.T)
                {
                    if(currentOrientation == Orientation.Forward)
                    {
                        FrontConnectionChange(backConnection);
                        LeftConnectionChange(backConnection);
                    }
                    else if(currentOrientation == Orientation.Right)
                    {
                        RightConnectionChange(backConnection);
                        FrontConnectionChange(backConnection);
                    }
                    else if(currentOrientation == Orientation.Backward)
                    {
                        LeftConnectionChange(backConnection);
                        FrontConnectionChange(backConnection);
                    }
                    else if(currentOrientation == Orientation.Left)
                    {
                        FrontConnectionChange(backConnection);
                        RightConnectionChange(backConnection);
                    }
                }
            }
            else if(transmitter == Transmitter.Left) // LEFT CONNECTION CHECK
            {
                if(nodeType == NodeType.I)
                {
                    RightConnectionChange(leftConnection);
                }
                else if(nodeType == NodeType.L) 
                {
                    if(currentOrientation == Orientation.Forward)
                    {
                        FrontConnectionChange(leftConnection);
                    }
                    else if(currentOrientation == Orientation.Backward)
                    {
                        BackConnectionChange(leftConnection);
                    }
                    else if(currentOrientation == Orientation.Left)
                    {
                        FrontConnectionChange(leftConnection);
                        BackConnectionChange(leftConnection);
                    }
                }
                else if(nodeType == NodeType.T)
                {
                    if(currentOrientation == Orientation.Forward)
                    {
                        RightConnectionChange(leftConnection);
                        BackConnectionChange(leftConnection);
                    }
                    else if(currentOrientation == Orientation.Right)
                    {
                        RightConnectionChange(leftConnection);
                        FrontConnectionChange(leftConnection);
                    }
                    else if(currentOrientation == Orientation.Backward)
                    {
                        BackConnectionChange(leftConnection);
                        RightConnectionChange(leftConnection);
                    }
                    else if(currentOrientation == Orientation.Left)
                    {
                        FrontConnectionChange(leftConnection);
                        RightConnectionChange(leftConnection);
                    }
                }
            }
        }
    
    #endregion
    
    #region Coroutines
    
        private IEnumerator RotateNodeCoroutine()
        {
            LevelManager.instance.rotatingNode = this;

            source.RecalculatePath(this); // RECALCULATE EVERYTHING
            
            rotating = true;

            // CHANGE ORIENTATION
            if(currentOrientation == Orientation.Forward)
            {
                currentOrientation = Orientation.Right;
            }
            else if(currentOrientation == Orientation.Right)
            {
                currentOrientation = Orientation.Backward;
            }
            else if(currentOrientation == Orientation.Backward)
            {
                currentOrientation = Orientation.Left;
            }
            else if(currentOrientation == Orientation.Left)
            {
                currentOrientation = Orientation.Forward;
            }

            source.ResetConnections();
            
            LocateTransmitters();

            ConnectionCheck(firstTransmitter);

            yield return null;

            if(frontConnection && frontConnection.semiActive && SemiActiveConnectionCheck(Transmitter.Front) && frontConnection.transmitter != this && !frontConnection.recentlyActivated)
            {
                Debug.Log("Node :: " + name + " :: ChangeMaterial() 11R :: ActivateConnection() -> " + frontConnection);
                frontConnection.ActivateConnection();
            }
            else if(rightConnection && rightConnection.semiActive && SemiActiveConnectionCheck(Transmitter.Right) && rightConnection.transmitter != this && !rightConnection.recentlyActivated)
            {
                Debug.Log("Node :: " + name + " :: ChangeMaterial() 12R :: ActivateConnection() -> " + rightConnection);
                rightConnection.ActivateConnection();
            }
            else if(backConnection && backConnection.semiActive && SemiActiveConnectionCheck(Transmitter.Back) && backConnection.transmitter != this && !backConnection.recentlyActivated)
            {
                Debug.Log("Node :: " + name + " :: ChangeMaterial() 13R :: ActivateConnection() -> " + backConnection);
                backConnection.ActivateConnection();
            }
            else if(leftConnection && leftConnection.semiActive && SemiActiveConnectionCheck(Transmitter.Left) && leftConnection.transmitter != this && !leftConnection.recentlyActivated)
            {
                Debug.Log("Node :: " + name + " :: ChangeMaterial() 14R :: ActivateConnection() -> " + leftConnection);
                leftConnection.ActivateConnection();
            }

            StartCoroutine(Rotation());

            LevelManager.instance.rotatingNode = null;
        }

        private IEnumerator Rotation()
        {
            SoundEffectManager.instance.PlaySoundEffect("Rotate Node", gameObject);

            float rotationPercentage = 0f;
            float smoothRotationPercentage = 0f;
            float rotationSpeed = 1f / rotationDuration;

            Quaternion currentRotation = transform.rotation;
            Quaternion desiredRotation = currentRotation * Quaternion.Euler(0, 90, 0);

            while(rotationPercentage < 1f)
            {
                rotationPercentage += Time.deltaTime * rotationSpeed;
                smoothRotationPercentage = animationCurve.Evaluate(rotationPercentage);

                transform.rotation = Quaternion.Lerp(currentRotation, desiredRotation, smoothRotationPercentage);

                yield return null;
            }

            rotating = false;
        }

        private IEnumerator LockedRotation(float duration, AnimationCurve curve)
        {
            SoundEffectManager.instance.PlaySoundEffect("Locked Node", gameObject);

            rotating = true;
            
            float animationPercentage = 0f;
            float smoothAnimationPercentage = 0f;
            float animationSpeed = 1f / duration;

            Quaternion currentRotation = transform.rotation;
            Quaternion desiredRotation = transform.rotation * Quaternion.Euler(0, 12, 0);

            while(animationPercentage < 1f)
            {
                animationPercentage += Time.deltaTime * animationSpeed;
                smoothAnimationPercentage = curve.Evaluate(animationPercentage);

                transform.rotation = Quaternion.Lerp(currentRotation, desiredRotation, smoothAnimationPercentage);

                yield return null;
            }

            animationPercentage = 0f;
            smoothAnimationPercentage = 0f;
            animationSpeed = 1f / duration;

            currentRotation = transform.rotation;
            desiredRotation = transform.rotation * Quaternion.Euler(0, -12, 0);

            while(animationPercentage < 1f)
            {
                animationPercentage += Time.deltaTime * animationSpeed;
                smoothAnimationPercentage = curve.Evaluate(animationPercentage);

                transform.rotation = Quaternion.Lerp(currentRotation, desiredRotation, smoothAnimationPercentage);

                yield return null;
            }

            animationPercentage = 0f;
            smoothAnimationPercentage = 0f;
            animationSpeed = 1f / duration;

            currentRotation = transform.rotation;
            desiredRotation = transform.rotation * Quaternion.Euler(0, 12, 0);

            while(animationPercentage < 1f)
            {
                animationPercentage += Time.deltaTime * animationSpeed;
                smoothAnimationPercentage = curve.Evaluate(animationPercentage);

                transform.rotation = Quaternion.Lerp(currentRotation, desiredRotation, smoothAnimationPercentage);

                yield return null;
            }

            rotating = false;
        }

        private IEnumerator ColorChange()
        {
            SoundEffectManager.instance.PlaySoundEffect("Unlock Node", gameObject);

            float transitionPercentage = 0f;
            float smoothTransitionPercentage = 0f;
            float transitionSpeed = 1f / rotationDuration;

            while(transitionPercentage < 1f)
            {
                transitionPercentage += Time.deltaTime * transitionSpeed;
                smoothTransitionPercentage = animationCurve.Evaluate(transitionPercentage);

                meshRenderer.material.color = Color.Lerp(lockedMaterial.color, unlockedMaterial.color, smoothTransitionPercentage);

                yield return null;
            }
        }
    
    #endregion
}