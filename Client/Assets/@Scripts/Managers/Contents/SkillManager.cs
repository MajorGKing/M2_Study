using System;
using Data;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Scripts.Data;
using Scripts.Data.SO;
using UnityEngine;

public class SkillManager
{
    Dictionary<int/*templateId*/, Skill> _skills = new Dictionary<int, Skill>();

    public int MainSkillTemplateId { get; set; }
}
