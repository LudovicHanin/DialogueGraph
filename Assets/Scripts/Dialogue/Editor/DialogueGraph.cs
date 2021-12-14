using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
    #region F/P

    private float widthWindow = 0f, heightWindow = 0f;
    private MiniMap miniMap = null;
    private DialogueGraphView graphView;
    private string fileName = "New Narrative";

    #endregion

    #region UnityMethods
    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMiniMap();
        GenerateBlackBoard();
    }
    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }
    private void Update()
    {
        if (Math.Abs(widthWindow - position.width) > 10 || Math.Abs(heightWindow - position.height) > 10)
        {
            widthWindow = position.width;
            heightWindow = position.height;
            UpdateMiniMapPosition();
        }
    }

    #endregion

    #region CustomMethods
    #region Public
    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        DialogueGraph _window = GetWindow<DialogueGraph>();
        _window.titleContent = new GUIContent("Dialogue Graph");
    }
    #endregion
    #region Private
    private void GenerateBlackBoard()
    {
        Blackboard _blackboard = new Blackboard();
        _blackboard.Add(new BlackboardSection {title = "Exposed Properties"});
        _blackboard.addItemRequested = _blackboard1 => { graphView.AddPropertyToBlackboard(new ExposedProperty()); };
        _blackboard.editTextRequested = (_blackboard1, _element, newValue) =>
        {
            string _oldPropertyName = ((BlackboardField) _element).text;
            if (graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "This property name already exists, please chose another one!",
                    "OK");
                return;
            }

            int _propertyIndex = graphView.ExposedProperties.FindIndex(x => x.PropertyName == _oldPropertyName);
            graphView.ExposedProperties[_propertyIndex].PropertyName = newValue;
            ((BlackboardField) _element).text = newValue;
        };
        _blackboard.SetPosition(new Rect(10, 30, 200, 300));
        graphView.Add(_blackboard);
        graphView.Blackboard = _blackboard;
    }
    private void GenerateMiniMap()
    {
        miniMap = new MiniMap {anchored = true};
        // Vector2 _cords = graphView.contentViewContainer.WorldToLocal(new Vector2( this.position.width-10, 30));
        // miniMap.SetPosition(new Rect(_cords.x, _cords.y, 200, 140));
        UpdateMiniMapPosition();
        graphView.Add(miniMap);
    }
    private void UpdateMiniMapPosition()
    {
        Vector2 _cords = graphView.contentViewContainer.WorldToLocal(new Vector2(position.width - 10, 50));
        miniMap.SetPosition(new Rect(_cords.x, _cords.y, 200, 140));
    }
    private void ConstructGraphView()
    {
        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }
    private void GenerateToolbar()
    {
        Toolbar _toolBar = new Toolbar();

        TextField _fileNameTextField = new TextField("File Name:");
        _fileNameTextField.SetValueWithoutNotify(fileName);
        _fileNameTextField.MarkDirtyRepaint();
        _fileNameTextField.RegisterValueChangedCallback(evt => fileName = evt.newValue);
        _toolBar.Add(_fileNameTextField);

        _toolBar.Add(new Button(() => RequestDataOperation(true)) {text = "Save Data"});
        _toolBar.Add(new Button(() => RequestDataOperation(false)) {text = "Load Data"});

        rootVisualElement.Add(_toolBar);
    }
    private void RequestDataOperation(bool _save)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "ok");
            return;
        }

        GraphSaveUtility _saveUtility = GraphSaveUtility.GetInstance(graphView);
        if (_save)
        {
            _saveUtility.SaveGraph(fileName);
        }
        else
        {
            _saveUtility.LoadGraph(fileName);
        }
    }
    #endregion
    #endregion


}