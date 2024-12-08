using System;
using System.Collections.Generic;
using System.Text;

namespace NASB_Parser_To_xNode
{
    public static class Consts
    {

        public static List<string> commentHeaderText = new List<string>{
            "",
            "",
            "This file was generated using NASB_Parser_to_xNode by megalon2d",
            "https://github.com/megalon/NASB_Parser_to_xNode",
            "",
            "",
        };

        public static List<string> basicTypes = new List<string> { 
            "bool", 
            "int", 
            "string", 
            "float", 
            "double",
            "MovesetParser.Unity.Vector2",
            "MovesetParser.Unity.Vector3"
        };

        public static NASBParserFolder[] folders = {
            new NASBParserFolder("CheckThings", "CheckThing"),
            new NASBParserFolder("FloatSources", "FloatSource"),
            new NASBParserFolder("Jumps", "Jump"),
            new NASBParserFolder("Misc", "Misc"),
            new NASBParserFolder("StateActions", "StateAction"),
            new NASBParserFolder("ObjectSources", "ObjectSource"),
            new NASBParserFolder("Unity", "Unity")
        };

        public static List<string> looseFiles = new List<string>{ "AgentState", "IdState", "TimedAction", "Moveset" };

        public static List<string> enumOnlyFiles = new List<string> {
            "GIEV",
            "HurtType",
            "Ease",
            "StageLayer",
            "WrapMode",
            "InputType",
            "LagType",
            "ManipLag",
            "SpeedType",
            "Search",
            "Stick",
            "LocalFXAction",
            "Manage",
            "Phase"
            };

        public static List<string> classesToIgnore = new List<string> { "ByteUtility" };

        public static Dictionary<string, string> classesToNamespaces = new Dictionary<string, string> {
            {"FloatSource", "FloatSources"},
            {"Jump", "Jumps"},
            {"Misc", "Misc" },
            {"CheckThing", "CheckThings"},
            {"StateAction", "StateActions"},
            {"ObjectSource", "ObjectSources"},
            {"Unity", "Unity" }
        };

        public static List<string> specialInputTypes = new List<string>
        {
            { "AnimConfig" },
            { "InputTrigger" },
            { "InputValidator" },
            { "SpawnMovement" },
            { "SAGUAMessageObject" },
            { "MovementConfig" },
            { "HurtBone"},
            { "HurtSetSetup" },
            { "LookForInput" },
        };

        public static string[] checkThingTypeIds = new string[]
        {
            "CheckThing",
            "CTMultiple",
            "CTCompareFloat",
            "CTDoubleTap",
            "CTInput",
            "CTInputSeries",
            "CTCheckTech",
            "CTGrab",
            "CTGrabbedAgent",
            "CTSkin",
            "CTMove"
        };

        public static string[] floatSourceTypeIds = new string[]
        {
            "FloatSource",
            "FSAgent",
            "FSBones",
            "FSAttack",
            "FSFrame",
            "FSInput",
            "FSFunc",
            "FSMovement",
            "FSCombat",
            "FSGrabs",
            "FSData",
            "FSScratch",
            "FSAnim",
            "FSSpeed",
            "FSPhysics",
            "FSCollision",
            "FSTimer",
            "FSLag",
            "FSEffects",
            "FSColors",
            "FSOnHit",
            "FSRandom",
            "FSCameraInfo",
            "FSSports",
            "FSVector2Mag",
            "FSCPUHelp",
            "FSItem",
            "FSMode",
            "FSJumps",
            "FSRootAnim",
            "FSLastAtk"
        };

        public static string[] jumpTypeIds = new string[]
        {
            "Jump",
            "HeightJump",
            "HoldJump",
            "AirDashJump",
            "KnockbackJump",
            "DelayedJump",
            "ClampMomentumJump"
        };

        public static string[] objectSourceTypeIds = new string[]
        {
            "ObjectSource",
            "OSFloat",
            "OSVector2"
        };

        public static string[] stateActionTypeIds = new string[]
        {
            "StateAction",
            "SADebugMessage",
            "SAPlayAnim",
            "SAPlayRootAnim",
            "SASnapAnimWeights",
            "SAStandardInput",
            "SAInputAction",
            "SADeactivateInputAction",
            "SAAddInputEventFromFrame",
            "SACancelToState",
            "SACustomCall",
            "SATimedAction",
            "SAOrderSensitive",
            "SAProxyMove",
            "SACheckThing",
            "SAActiveAction",
            "SADeactivateAction",
            "SASetFloatTarget",
            "SAOnBounce",
            "SAOnLeaveEdge",
            "SAOnStoppedAtEdge",
            "SAOnLand",
            "SAOnCancel",
            "SARefreshAttack",
            "SAEndAttack",
            "SASetHitboxCount",
            "SAConfigHitbox",
            "SASetAtkProp",
            "SAManipHitBox",
            "SAUpdateHurtboxes",
            "SASetupHurtboxes",
            "SAManipHurtBox",
            "SABoneState",
            "SABoneScale",
            "SASpawnAgent",
            "SALocalFX",
            "SASpawnFX",
            "SASetHitboxFX",
            "SAPlaySFX",
            "SASetHitboxSFX",
            "SAColorTint",
            "SAFindFloor",
            "SAHurtGrabbed",
            "SALaunchGrabbed",
            "SAStateCancelGrabbed",
            "SAEndGrab",
            "SAGrabNotifyEscape",
            "SAIgnoreGrabbed",
            "SAEventKO",
            "SAEventKOGrabbed",
            "SACameraShake",
            "SAResetOnHits",
            "SAOnHit",
            "SAFastForwardState",
            "SATimingTweak",
            "SAMapAnimation",
            "SAAlterMoveDT",
            "SAAlterMoveVel",
            "SASetStagePart",
            "SASetStagePartsDefault",
            "SAJump",
            "SAStopJump",
            "SAManageAirJump",
            "SALeaveGround",
            "SAUnHogEdge",
            "SAPlaySFXTimeline",
            "SAFindLastHorizontalInput",
            "SASetCommandGrab",
            "SACameraPunch",
            "SASpawnAgent2",
            "SAManipDecorChain",
            "SAUpdateHitboxes",
            "SASampleAnim",
            "SAForceExtraInputCheck",
            "SALaunchGrabbedCustom",
            "SAMapAnimationSimple",
            "SAPersistLocalFX",
            "SAOnLeaveParent",
            "SAPlayVoiceLine",
            "SAPlayCategoryVoiceLine",
            "SAStopVoiceLines"
        };
    }
}
