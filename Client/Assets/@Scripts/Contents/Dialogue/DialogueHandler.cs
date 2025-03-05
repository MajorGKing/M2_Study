using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Contents.Dialog;
using Scripts.Data.SO;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    string _heroName;

    DialogueData _currentDialogueData;
    DialogueNode _currentNode = null;
    bool _isChoosing = false;
    private Action _onFinished;

    public void StartDialogue(DialogueData newDialogueData, Action onFinished)
    {
        _onFinished = onFinished;
        _currentDialogueData = newDialogueData;
        _currentNode = _currentDialogueData.GetRootNode();
        _currentDialogueData.SetInfo();
    }

    public void Quit()
    {
        _currentDialogueData = null;
        _currentNode = null;
        _isChoosing = false;
        _onFinished?.Invoke();
    }

    public bool IsActive()
    {
        return _currentDialogueData != null;
    }

    public bool IsChoosing()
    {
        return _isChoosing;
    }

    public string GetText()
    {
        if(_currentNode == null)
        {
            return "";
        }

        string language = Managers.GetText(_currentNode.GetTextId());
        return language;
    }

    public bool IsPlayerSpeaking()
    {
        return _currentNode.IsPlayerSpeaking();
    }

    public string GetCurrentConversantName()
    {
        if (IsChoosing() || _currentNode.IsPlayerSpeaking())
        {
            return _heroName;
        }
        else
        {
            return _currentDialogueData.OwnerName;
            // return currentConversant.GetName();
        }
    }

    public IEnumerable<DialogueNode> GetChoices()
    {
        return _currentDialogueData.GetPlayerChildren(_currentNode);
    }

    public void SelectChoice(DialogueNode chosenNode)
    {
        _currentNode = chosenNode;
        _isChoosing = false;
        Next();
    }

    public void Skip()
    {
        while (HasNext())
        {
            DialogueNode[] children = _currentDialogueData.GetAllChildren(_currentNode).ToArray();
            _currentNode = children.FirstOrDefault();
        }
    }

    public void Next()
    {
        if (HasNext() == false)
        {
            Quit();
            return;
        }

        DialogueNode[] children = _currentDialogueData.GetAllChildren(_currentNode).ToArray();
        int randomIndex = UnityEngine.Random.Range(0, children.Count());
        _currentNode = children[randomIndex];


        //플레이어가 선택하는 노드일때,
        // int numPlayerResponses = currentDialogue.GetPlayerChildren(currentNode).Count();
        // if (numPlayerResponses > 1)
        // {
        //     //TODO 필요 하면 선택하는 모드로 변경
        //     isChoosing = true;
        //     return;
        // }
        //
        // DialogueNode[] children = currentDialogue.GetAIChildren(currentNode).ToArray();
        // int randomIndex = UnityEngine.Random.Range(0, children.Count());
        // currentNode = children[randomIndex];
    }

    public bool HasNext()
    {
        if (_currentNode == null)
            return false;
        return _currentDialogueData.GetAllChildren(_currentNode).Count() > 0;
    }
}
