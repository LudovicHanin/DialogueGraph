using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    private readonly Vector2 defaultNodeSize = new Vector2(150, 200);
    
    public DialogueGraphView()
    {
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        AddElement(GenerateEntryPointNode());
    }

    private Port GeneratePort(DialogueNode _node, Direction _portDirection,
        Port.Capacity _capacity = Port.Capacity.Single)
    {
        return _node.InstantiatePort(Orientation.Horizontal, _portDirection, _capacity, typeof(float));
    }
    private DialogueNode GenerateEntryPointNode()
    {
        DialogueNode _node = new DialogueNode
        {
            title = "START",
            GUID = Guid.NewGuid().ToString(),
            DialogueText = "ENTRYPOINT",
            EntryPoint = true
        };

        Port _generatedPort = GeneratePort(_node, Direction.Output);
        _generatedPort.portName = "Next";
        _node.outputContainer.Add(_generatedPort);
        
        _node.RefreshExpandedState();
        _node.RefreshPorts();
        
        _node.SetPosition(new Rect(100, 200, 100, 150));
        return _node;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> _compatiblePorts = new List<Port>();
        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node) _compatiblePorts.Add(port);
        });
        return _compatiblePorts;
    }

    public void CreateNode(string _nodeName)
    {
        AddElement(CreateDialogueNode(_nodeName));
    }
    private DialogueNode CreateDialogueNode(string _nodeName)
    {
        DialogueNode _dialogueNode = new DialogueNode
        {
            title = _nodeName,
            DialogueText = _nodeName,
            GUID = Guid.NewGuid().ToString()
        };

        Port _inputPort = GeneratePort(_dialogueNode, Direction.Input, Port.Capacity.Multi);
        _inputPort.portName = "Input";
        _dialogueNode.inputContainer.Add(_inputPort);
        _dialogueNode.RefreshExpandedState();
        _dialogueNode.RefreshPorts();
        _dialogueNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

        return _dialogueNode;
    }
}
