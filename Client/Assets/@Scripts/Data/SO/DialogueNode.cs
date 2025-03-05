using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Scripts.Contents.Dialog
{
    public class DialogueNode : ScriptableObject
    {
        public bool _isPlayerSpeaking = false;
        public string _text;
        public string _textTemplateId;
        public List<string> _children = new List<string>();
        public float x;
        public float y;
        [NonSerialized] public Rect _rect = new Rect(0, 0, 250, 160);

        public Rect GetRect()
        {
            _rect.x = x;
            _rect.y = y;
            return _rect;
        }

        public string GetTextId()
        {
            return _textTemplateId;
        }

        public string GetText()
        {
            return _text;
        }

        public List<string> GetChildren()
        {
            return _children;
        }

        public bool IsPlayerSpeaking()
        {
            return _isPlayerSpeaking;
        }

#if UNITY_EDITOR
        public void SetPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move Dialogue Node");
            _rect.position = newPosition;
            x = newPosition.x;
            y = newPosition.y;
            EditorUtility.SetDirty(this);
        }

        public void SetText(string newText)
        {
            if (newText != _text)
            {
                Undo.RecordObject(this, "Update Dialogue Text");
                _text = newText;
                EditorUtility.SetDirty(this);
            }
        }

        public void SetTextTemplateId(string newText)
        {
            if (newText != _textTemplateId)
            {
                Undo.RecordObject(this, "Update Dialogue TextTemplateId");
                _textTemplateId = newText;
                EditorUtility.SetDirty(this);
            }
        }

        public void AddChild(string childID)
        {
            Undo.RecordObject(this, "Add Dialogue Link");
            _children.Add(childID);
            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(string childID)
        {
            Undo.RecordObject(this, "Remove Dialogue Link");
            _children.Remove(childID);
            EditorUtility.SetDirty(this);
        }

        public void SetPlayerSpeaking(bool newIsPlayerSpeaking)
        {
            Undo.RecordObject(this, "Change Dialogue Speaker");
            _isPlayerSpeaking = newIsPlayerSpeaking;
            EditorUtility.SetDirty(this);
        }

#endif
    }
}
