using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    public readonly Vector2 DefaultNodeSize = new Vector2(150, 200);

    public Blackboard Blackboard;
    public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
    private NodeSearchWindow searchWindow;
    
    public DialogueGraphView(EditorWindow _editorWindow)
    {
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
        
        AddElement(GenerateEntryPointNode());
        AddSearchWindow(_editorWindow);
    }

    private void AddSearchWindow(EditorWindow _editorWindow)
    {
        searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        searchWindow.Init(_editorWindow, this);
        nodeCreationRequest = context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
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
        
        _node.capabilities &= ~Capabilities.Movable;
        _node.capabilities &= ~Capabilities.Deletable;
        
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

    public void CreateNode(string _nodeName, Vector2 _position)
    {
        AddElement(CreateDialogueNode(_nodeName, _position));
    }
    public DialogueNode CreateDialogueNode(string _nodeName, Vector2 _position)
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

        _dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        
        Button _button = new Button(() =>
        {
            AddChoicePort(_dialogueNode);
        });
        _button.text = "New Choice";
        _dialogueNode.titleContainer.Add(_button);

        TextField _textField = new TextField(string.Empty);
        _textField.RegisterValueChangedCallback(evt =>
        {
            _dialogueNode.DialogueText = evt.newValue;
            _dialogueNode.title = evt.newValue;
        });
        _textField.SetValueWithoutNotify(_dialogueNode.title);
        _dialogueNode.mainContainer.Add(_textField);
        
        _dialogueNode.RefreshExpandedState();
        _dialogueNode.RefreshPorts();
        _dialogueNode.SetPosition(new Rect(_position, DefaultNodeSize));

        return _dialogueNode;
    }

    public void AddChoicePort(DialogueNode dialogueNode, string _overridenPortName = "")
    {
        Port _generatePort = GeneratePort(dialogueNode, Direction.Output);

        Label _oldLabel = _generatePort.contentContainer.Q<Label>("type");
        _generatePort.contentContainer.Remove(_oldLabel);

        int _outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;

        string choicePortName = string.IsNullOrEmpty(_overridenPortName) ? $"Choice {_outputPortCount + 1}" : _overridenPortName;

        TextField _textField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };
        _textField.RegisterValueChangedCallback(evt => _generatePort.portName = evt.newValue);
        _generatePort.contentContainer.Add(new Label("  "));
        _generatePort.contentContainer.Add(_textField);
        Button _deleteButton = new Button(() => RemovePort(dialogueNode, _generatePort))
        {
            text = "X"
        };
        _generatePort.contentContainer.Add(_deleteButton);
        
        _generatePort.portName = choicePortName;
        dialogueNode.outputContainer.Add(_generatePort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }

    private void RemovePort(DialogueNode _dialogueNode, Port _generatePort)
    {
        IEnumerable<Edge> _targetEdge = edges.ToList()
            .Where(x => x.output.portName == _generatePort.portName && x.output.node == _generatePort.node);

        if (_targetEdge.Any())
        {
            Edge _edge = _targetEdge.First();
            _edge.input.Disconnect(_edge);
            RemoveElement(_targetEdge.First());
        }
        
        _dialogueNode.outputContainer.Remove(_generatePort);
        _dialogueNode.RefreshPorts();
        _dialogueNode.RefreshExpandedState();
    }

    public void ClearBlackBoardAndExposedProperties()
    {
        ExposedProperties.Clear();
        Blackboard.Clear();
    }
    
    public void AddPropertyToBlackboard(ExposedProperty _exposedProperty)
    {
        string _localPropertyName = _exposedProperty.PropertyName;
        string _localPropertyValue = _exposedProperty.PropertyValue;
        while (ExposedProperties.Any(x => x.PropertyName == _localPropertyName))
            _localPropertyName = $"{_localPropertyName}(1)";
        
        ExposedProperty _property = new ExposedProperty();
        _property.PropertyName = _localPropertyName;
        _property.PropertyValue = _localPropertyValue;
        ExposedProperties.Add(_property);

        VisualElement _container = new VisualElement();
        BlackboardField _blackboardField = new BlackboardField
        {
            text = _localPropertyName,
            typeText = "string"
        };
        _container.Add(_blackboardField);

        TextField _propertyValueTextField = new TextField("Value:")
        {
            value = _localPropertyValue
        };
        _propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            int _changingPropertyIndex = ExposedProperties.FindIndex(x => x.PropertyName == _property.PropertyName);
            ExposedProperties[_changingPropertyIndex].PropertyValue = evt.newValue;
        });
        BlackboardRow _blackboardValueRow = new BlackboardRow(_blackboardField, _propertyValueTextField);
        _container.Add(_blackboardValueRow);
        Blackboard.Add(_container);
    }
}
