using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialogueGraphView graphView;
    private EditorWindow window;
    private Texture2D indentationIcon;

    public void Init(EditorWindow _window, DialogueGraphView _graphView)
    {
        window = _window;
        graphView = _graphView;

        indentationIcon = new Texture2D(1, 1);
        indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0)); //Dont forget to set the alpha to 0 as well
        indentationIcon.Apply();
    }
    
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> _tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
            new SearchTreeEntry(new GUIContent("Dialogue Node", indentationIcon))
            {
                userData = new DialogueNode(),
                level = 2
            }
        };
        return _tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        Vector2 _worldMousePosition =
            window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent, context.screenMousePosition-window.position.position);
        Vector2 _localMousePosition = graphView.contentViewContainer.WorldToLocal(_worldMousePosition);
        
        switch (SearchTreeEntry.userData)
        {
            case DialogueNode dialogueNode:
                graphView.CreateNode("Dialogue Node", _localMousePosition);
                return true;
            default: return false;
        }
    }
}
