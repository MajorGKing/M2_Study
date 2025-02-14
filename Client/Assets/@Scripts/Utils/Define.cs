public class Define
{
    public const char MAP_TOOL_WALL = '0';
    public const char MAP_TOOL_NONE = '1';

    public const int HERO_DEFAULT_MOVE_DEPTH = 8;

    public const int MAX_LOBBY_HERO_COUNT = 10;
    public const int MAX_QUICKSLOT_COUNT = 5;

    public enum EEventType
    {
        None,

		OnClickAttackButton,
		OnClickAutoButton,

        InventoryChanged,
        CurrencyChanged,
        StatChanged,
    }

    public enum EToastColor
    {
        Black,
        Red,
        Purple,
        Magenta,
        Blue,
        Green,
        Yellow,
        Orange
    }

    public enum EToastPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum EHeroMoveState
    {
        None,
        TargetMonster,
        MoveToDesiredPos,
    }

    public enum EJoystickState
    {
        None,
        PointerDown,
        Drag,
        PointerUp,
        Attack,
        Auto,
        Pickup,
    }

    public enum EScene
    {
        Unknown,
        TitleScene,
        LoadingScene,
        GameScene,
    }

    public enum ESound
    {
        Bgm,
        SubBgm,
        Effect,
        Max,
    }

    public enum ETouchEvent
    {
        PointerUp,
        PointerDown,
        Click,
        LongPressed,
        BeginDrag,
        Drag,
        EndDrag,
    }

    public enum ELanguage
    {
        Korean,
        English,
        French,
        SimplifiedChinese,
        TraditionalChinese,
        Japanese,
    }

    public enum ELayer
    {
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Dummy1 = 3,
        Water = 4,
        UI = 5,
        Hero = 6,
        Monster = 7,
        Boss = 8,
        //
        Env = 11,
        Obstacle = 12,
        //
        Projectile = 20,
    }
}

public static class SortingLayers
{
    public const int SPELL_INDICATOR = 200;
    public const int HERO = 300;
    public const int NPC = 300;
    public const int MONSTER = 300;
    public const int BOSS = 300;
    public const int GATHERING_RESOURCES = 300;
    public const int PROJECTILE = 310;
    public const int DROP_ITEM = 310;
    public const int SKILL_EFFECT = 315;
    public const int DAMAGE_FONT = 410;
}

public static class AnimName
{
    public const string ATTACK = "attack";
    public const string IDLE = "idle";
    public const string MOVE = "move";
    public const string DAMAGED = "hit";
    public const string DEAD = "dead";
    public const string EVENT_ATTACK_A = "event_attack";
    public const string EVENT_ATTACK_B = "event_attack";
    public const string EVENT_SKILL_A = "event_attack";
    public const string EVENT_SKILL_B = "event_attack";
}