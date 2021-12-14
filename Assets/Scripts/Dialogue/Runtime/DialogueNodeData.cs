using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNodeData
{
    [SerializeField] string nodeGUID;
    [SerializeField] string dialogueText;
    [SerializeField] Vector2 position;

    public string NodeGUID
    {
        get => nodeGUID;
        set => nodeGUID = value;
    }

    public string DialogueText
    {
        get => dialogueText;
        set => dialogueText = value;
    }

    public Vector2 Position
    {
        get => position;
        set => position = value;
    }
}
