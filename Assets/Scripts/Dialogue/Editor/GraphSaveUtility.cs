using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

public class GraphSaveUtility
{
    #region F/P
    private DialogueGraphView targetGraphView;
    private DialogueContainer containerCache;
    private List<Edge> Edges => targetGraphView.edges.ToList();
    private List<DialogueNode> Nodes => targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();
    #endregion
    
    #region CustomMethods
    #region Public
    public static GraphSaveUtility GetInstance(DialogueGraphView _targetGraphView)
    {
        return new GraphSaveUtility
        {
            targetGraphView = _targetGraphView
        };
    }
    public void SaveGraph(string _fileName)
    {
        DialogueContainer _dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        if (!SaveNodes(_dialogueContainer)) return;

        SaveExposedProperties(_dialogueContainer);

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        
        AssetDatabase.CreateAsset(_dialogueContainer, $"Assets/Resources/{_fileName}.asset");
        AssetDatabase.SaveAssets();
    }
    public void LoadGraph(string _fileName)
    {
        containerCache = Resources.Load<DialogueContainer>(_fileName);

        if (containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target Dialogue graph file does not exists!", "ok");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
        CreateExposedProperties();
    }
    #endregion
    #region Private
    private void SaveExposedProperties(DialogueContainer _dialogueContainer)
    {
        _dialogueContainer.ExposedProperties.AddRange(targetGraphView.ExposedProperties);
    }
    private void CreateExposedProperties()
    {
        targetGraphView.ClearBlackBoardAndExposedProperties();
        foreach (var _exposedProperpty in containerCache.ExposedProperties)
        {
            targetGraphView.AddPropertyToBlackboard(_exposedProperpty);
        }
    }
    private void ConnectNodes()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            List<NodeLinkData> _connections = containerCache.NodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].GUID).ToList();
            for (int j = 0; j < _connections.Count; j++)
            {
                var _targetNodeGuid = _connections[j].TargetNodeGuid;
                var _targetNode = Nodes.First(x => x.GUID == _targetNodeGuid);
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port) _targetNode.inputContainer[0]);
                
                _targetNode.SetPosition(new Rect(containerCache.DialogueNodeData.First(x => x.NodeGUID == _targetNodeGuid).Position,
                    targetGraphView.DefaultNodeSize));
            }
        }
    }
    private void LinkNodes(Port _output, Port _input)
    {
        var _tempEdge = new Edge
        {
            output = _output,
            input = _input
        };
        
        _tempEdge?.input.Connect(_tempEdge);
        _tempEdge?.output.Connect(_tempEdge);
        
        targetGraphView.Add(_tempEdge);
    }
    private void CreateNodes()
    {
        foreach (DialogueNodeData nodeData in containerCache.DialogueNodeData)
        {
            //We pass position later on, so we can just use vec2 zero for now as position while loading nodes.
            var _tempNode = targetGraphView.CreateDialogueNode(nodeData.DialogueText, Vector2.zero);
            _tempNode.GUID = nodeData.NodeGUID;
            targetGraphView.AddElement(_tempNode);

            var nodePorts = containerCache.NodeLinks.Where(x => x.BaseNodeGuid == nodeData.NodeGUID).ToList();
            nodePorts.ForEach(x => targetGraphView.AddChoicePort(_tempNode, x.PortName));
        }
    }
    private bool SaveNodes(DialogueContainer _dialogueContainer)
    {
        if (!Edges.Any()) return false; //if there are no edge(no connection) then return
        
        Edge[] _conectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (int i = 0; i < _conectedPorts.Length; i++)
        {
            DialogueNode _outputNode = _conectedPorts[i].output.node as DialogueNode;
            DialogueNode _inputNode = _conectedPorts[i].input.node as DialogueNode;

            _dialogueContainer.NodeLinks.Add(new NodeLinkData
            {
                BaseNodeGuid = _outputNode.GUID,
                PortName = _conectedPorts[i].output.portName,
                TargetNodeGuid = _inputNode.GUID
            });
        }

        foreach (DialogueNode dialogueNode in Nodes.Where(node=>!node.EntryPoint))
        {
            _dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
            {
                NodeGUID = dialogueNode.GUID,
                DialogueText = dialogueNode.DialogueText,
                Position = dialogueNode.GetPosition().position
            });
        }

        return true;
    }
    private void ClearGraph()
    {
        //Set entry points guid back from the save. Discard existing guid.
        Nodes.Find(x => x.EntryPoint).GUID = containerCache.NodeLinks[0].BaseNodeGuid;

        foreach (DialogueNode node in Nodes)
        {
            if (node.EntryPoint) continue;
            
            //Remove edges that connected to this code
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge=>targetGraphView.RemoveElement(edge));
            
            //Then remove the node
            targetGraphView.RemoveElement(node);
        }
    }
    #endregion
    #endregion
}
