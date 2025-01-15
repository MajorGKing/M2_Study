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

    #region ��ų ���
    public List<SkillData> GetAllSkillDatas(bool excludeMainSkill = true)
    {
        List<SkillData> skillDatas = new List<SkillData>();

        foreach (Skill skill in GetAllSkills(excludeMainSkill))
            skillDatas.Add(skill.SkillData);

        return skillDatas;
    }

    public List<Skill> GetAllSkills(bool excludeMainSkill = true)
    {
        List<Skill> skills = new List<Skill>();

        foreach (Skill skill in _skills.Values)
        {
            // �⺻��ų�� ����
            if (excludeMainSkill && skill.TemplateId == MainSkillTemplateId)
                continue;

            skills.Add(skill);
        }

        return skills;
    }

    public Skill GetMainSkill()
    {
        return GetSkill(MainSkillTemplateId);
    }

    public Skill GetSkill(int templateId)
    {
        if(_skills.TryGetValue(templateId, out Skill skill))
            return skill;

        return null;
    }

    public void UseSkill(int templateId, Creature target)
    {
        Skill skill = GetSkill(templateId);
        if(skill == null) 
            return;

        skill.UseSkill(target);
    }

    public float GetRemainingCoolTimeRatio(int templateId)
    {
        Skill skill = GetSkill(templateId);
        if (skill == null)
            return 0;

        return skill.GetRemainingCoolTimeRatio();
    }

    public void SetRemainingCooltime(int templateId, long remainTicks)
    {
        Skill skill = GetSkill(templateId);
        if (skill == null)
            return;

        skill.SetRemainingCooltime(remainTicks);
    }
    #endregion

    #region ��ų ��� & ��Ÿ�� ����
    public void Init(int heroTemplateId)
    {
        Clear();

        Managers.Data.HeroDic.TryGetValue(heroTemplateId, out HeroData heroData);

        MainSkillTemplateId = heroData.MainSkill.TemplateId;

        RegisterSkill(heroData.MainSkill.TemplateId);
        RegisterSkill(heroData.SkillA.TemplateId);
        RegisterSkill(heroData.SkillB.TemplateId);
        RegisterSkill(heroData.SkillC.TemplateId);
    }

    public bool RegisterSkill(int templateId)
    {
        if (_skills.ContainsKey(templateId))
            return false;
        if (Managers.Data.SkillDic.TryGetValue(templateId, out SkillData skillData) == false)
            return false;

        Skill skill = null;

        if (skillData.Projectile != null)
            skill = new ProjectileSkill(templateId, Managers.Object.MyHero);
        else
            skill = new NormalSkill(templateId, Managers.Object.MyHero);

        _skills.Add(templateId, skill);
        return true;
    }

    public bool CheckCooltime(int templateId)
    {
        Skill skill = GetSkill(templateId);
        if (skill == null)
            return false;

        return skill.CheckCooltime(); // TODO : CheckCooltime
    }

    public void UpdateCooltime(int templateId)
    {
        Skill skill = GetSkill(templateId);
        if (skill == null)
            return;

        skill.UpdateCooltime();
    }
    #endregion

    public int GetAvailableSkill()
    {
        foreach (var skill in _skills.Values)
        {
            if (skill.CheckCooltime())
                return skill.TemplateId;
        }

        return 0;
    }

    public void HandleEnterGame(S_EnterGame packet)
    {
        foreach (var cooltime in packet.Cooltimes)
        {
            Managers.Skill.SetRemainingCooltime(cooltime.SkillId, cooltime.RemainingTicks);
        }
    }

    public void Clear()
    {
        _skills.Clear();
    }
}