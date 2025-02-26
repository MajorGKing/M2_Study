using Google.Protobuf.Protocol;
using Scripts.Data;

public class ProjectileSkill : Skill
{
    public ProjectileSkill(int templateId, MyHero owner) : base(templateId, owner)
    {

    }

    public override ECanUseSkillFailReason CanUseSkill(Creature useTarget)
    {
        ECanUseSkillFailReason result = base.CanUseSkill(useTarget);
        if (result != ECanUseSkillFailReason.None)
            return result;

        if (SkillData.Projectile == null)
            return ECanUseSkillFailReason.InvalidData;

        return ECanUseSkillFailReason.None;
    }

    public override void UseSkill(Creature target)
    {
        if (CanUseSkill(target) != ECanUseSkillFailReason.None)
            return;

        ReqUseSkill();
    }
}
