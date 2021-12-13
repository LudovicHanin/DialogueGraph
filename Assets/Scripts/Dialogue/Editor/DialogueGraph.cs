using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;
    
    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        DialogueGraph _window = GetWindow<DialogueGraph>();
        _window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void ConstructGraphView()
    {
        _graphView = new DialogueGraphView
        {
            name = "Dialogue Graph"
        };
        
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        Toolbar _toolBar = new Toolbar();
        Button _nodeCreateButton = new Button(() =>
        {
            _graphView.CreateNode("Dialogue Node");
        });
        _nodeCreateButton.text = "Create Node";
        _toolBar.Add(_nodeCreateButton);
        
        rootVisualElement.Add(_toolBar);
    }
    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
}
