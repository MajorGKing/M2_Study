using Google.Protobuf.Protocol;

public class NormalSkill : Skill
{
    public NormalSkill(int templateId, MyHero owner) : base(templateId, owner)
    {

    }

    public override ECanUseSkillFailReason CanUseSkill(Creature target)
    {
        ECanUseSkillFailReason result = base.CanUseSkill(target);
        if (result != ECanUseSkillFailReason.None)
            return result;

        return ECanUseSkillFailReason.None;
    }

    public override void UseSkill(Creature target)
    {
        if (CanUseSkill(target) == ECanUseSkillFailReason.None)
            return;

        ReqUseSkill();
    }
}
