using System;
using System.Collections.Generic;
using System.Text;

namespace NASB_Parser_To_xNode
{
    public static class Consts
    {
        public static void GenerateClassToTypeIdDict()
        {
            // Generate dictionary where class name is mapped to type id
            foreach (string key in Consts.checkThingsIds.Keys)
            {
                Consts.classToTypeId.Add(Consts.checkThingsIds[key], key);
            }

            foreach (string key in Consts.floatSourceIds.Keys)
            {
                Consts.classToTypeId.Add(Consts.floatSourceIds[key], key);
            }

            foreach (string key in Consts.stateActionIds.Keys)
            {
                Consts.classToTypeId.Add(Consts.stateActionIds[key], key);
            }

            foreach (string key in Consts.jumpId.Keys)
            {
                Consts.classToTypeId.Add(Consts.jumpId[key], key);
            }

            foreach (string key in Consts.objectSourceIds.Keys)
            {
                Consts.classToTypeId.Add(Consts.objectSourceIds[key], key);
            }
        }

        public static List<string> basicTypes = new List<string> { "bool", "int", "string", "float", "double", "NASB_Parser.Vector3" };

        public static NASBParserFolder[] folders = {
            new NASBParserFolder("FloatSources", "FloatSource"),
            new NASBParserFolder("Jumps", "Jump"),
            new NASBParserFolder("CheckThings", "CheckThing"),
            new NASBParserFolder("StateActions", "StateAction"),
            new NASBParserFolder("ObjectSources", "ObjectSource")
        };

        public static List<string> looseFiles = new List<string>{ "AgentState", "IdState", "TimedAction", "SerialMoveset", "TimedAction" };

        public static Dictionary<string, string> classesToNamespaces = new Dictionary<string, string> {
            {"FloatSource", "FloatSources"},
            {"Jump", "Jumps"},
            {"CheckThing", "CheckThings"},
            {"StateAction", "StateActions"},
            {"ObjectSource", "ObjectSources"}
        };

        public static Dictionary<string, string> checkThingsIds = new Dictionary<string, string>
        {
            { "MultipleId", "CTMultiple" },
            { "CompareId", "CTCompareFloat" },
            { "DoubleTapId", "CTDoubleTapId" },
            { "InputId", "CTInput" },
            { "InputSeriesId", "CTInputSeries" },
            { "TechId", "CTCheckTech" },
            { "GrabId", "CTGrabId" },
            { "GrabAgentId", "CTGrabbedAgent" },
            { "SkinId", "CTSkin" },
            { "MoveId", "CTMove" },
            { "BaseIdentifier", "CheckThing" },
        };

        public static Dictionary<string, string> floatSourceIds = new Dictionary<string, string>
        {
            { "AgentId", "FSAgent" },
            { "BonesId", "FSBones" },
            { "AttackId", "FSAttack" },
            { "FrameId", "FSFrame" },
            { "InputId", "FSInput" },
            { "FuncId", "FSFunc" },
            { "MovementId", "FSMovement" },
            { "CombatId", "FSCombat" },
            { "GrabsId", "FSGrabs" },
            { "DataId", "FSData" },
            { "ScratchId", "FSScratch" },
            { "AnimId", "FSAnim" },
            { "SpeedId", "FSSpeed" },
            { "PhysicsId", "FSPhysics" },
            { "CollisionId", "FSCollision" },
            { "TimerId", "FSTimer" },
            { "LagId", "FSLag" },
            { "EffectsId", "FSEffects" },
            { "ColorsId", "FSColors" },
            { "OnHitId", "FSOnHit" },
            { "RandomId", "FSRandom" },
            { "CameraId", "FSCameraInfo" },
            { "SportsId", "FSSports" },
            { "Vector2Mag", "FSVector2Mag" },
            { "CPUHelpId", "FSCpuHelp" },
            { "ItemId", "FSItem" },
            { "ModeId", "FSMode" },
            { "JumpsId", "FSJumps" },
            { "RootAnimId", "FSRootAnim" },
            { "FloatId", "FSValue" },
        };

        public static Dictionary<string, string> stateActionIds = new Dictionary<string, string>
        {
            { "DebugId", "SADebugMessage" },
            { "PlayAnimId", "SAPlayAnim" },
            { "RootAnimId", "SAPlayRootAnim" },
            { "SnapAnimWeightsId", "SASnapAnimWeights" },
            { "StandardInputId", "SAStandardInput" },
            { "InputId", "SAInputAction" },
            { "DeactInputId", "SADeactivateInputAction" },
            { "InputEventFromFrameId", "SAAddInputEventFromFrame" },
            { "CancelStateId", "SACancelToState" },
            { "CustomCallId", "SACustomCall" },
            { "TimerId", "SATimedAction" },
            { "OrderId", "SAOrderedSensitive" },
            { "ProxyId", "SAProxyMove" },
            { "CheckId", "SACheckThing" },
            { "ActiveActionId", "SAActiveAction" },
            { "DeactivateActionId", "SADeactivateAction" },
            { "SetFloatId", "SASetFloatTarget" },
            { "OnBounceId", "SAOnBounce" },
            { "OnLeaveEdgeId", "SAOnLeaveEdge" },
            { "OnStoppedAtEdgeId", "SAOnStoppedAtLedge" },
            { "OnLandId", "SAOnLand" },
            { "OnCancelId", "SAOnCancel" },
            { "RefreshAtkId", "SARefreshAttack" },
            { "EndAtkId", "SAEndAttack" },
            { "SetHitboxCountId", "SASetHitboxCount" },
            { "ConfigHitboxId", "SAConfigHitbox" },
            { "SetAtkPropId", "SASetAttackProp" },
            { "ManipHitboxId", "SAManipHitbox" },
            { "UpdateHurtsetId", "SAUpdateHurtboxes" },
            { "SetupHurtsetId", "SASetupHurtboxes" },
            { "ManipHurtboxId", "SAManipHurtbox" },
            { "BoneStateId", "SABoneState" },
            { "BoneScaleId", "SABoneScale" },
            { "SpawnAgentId", "SASpawnAgent" },
            { "LocalFXId", "SALocalFX" },
            { "SpawnFXId", "SASpawnFX" },
            { "HitboxFXId", "SASetHitboxFX" },
            { "SFXId", "SAPlaySFX" },
            { "HitboxSFXId", "SASetHitboxSFX" },
            { "ColorTintId", "SAColorTint" },
            { "FindFloorId", "SAFindFloor" },
            { "HurtGrabbedId", "SAHurtGrabbed" },
            { "LaunchGrabbedId", "SALaunchGrabbed" },
            { "StateCancelGrabbedId", "SAStateCancelGrabbed" },
            { "EndGrabId", "SAEndGrab" },
            { "GrabNotifyEscapeId", "SAGrabNotifyEscape" },
            { "IgnoreGrabbedId", "SAIgnoreGrabbed" },
            { "EventKOId", "SAEventKO" },
            { "EventKOGrabbedId", "SAEventKOGrabbed" },
            { "CameraShakeId", "SACameraShake" },
            { "ResetOnHitId", "SAResetOnHits" },
            { "OnHitId", "SAOnHit" },
            { "FastForwardId", "SAFastForwardState" },
            { "TimingTweakId", "SATimingTweak" },
            { "MapAnimId", "SAMapAnimation" },
            { "AlterMoveDtId", "SAAlterMoveDT" },
            { "AlterMoveVelId", "SAAlterMoveVel" },
            { "SetStagePartId", "SASetStagePart" },
            { "SetStagePartsDefaultId", "SASetStagePartsDefault" },
            { "JumpId", "SAJump" },
            { "StopJumpId", "SAStopJump" },
            { "ManageAirJumpId", "SAManageAirJump" },
            { "LeaveGroundId", "SALeaveGround" },
            { "UnhogEdgeId", "SAUnHogEdge" },
            { "SFXTimelineId", "SAPlaySFXTimeline" },
            { "FindLastHorizontalInputId", "SAFindLastHorizontalInput" },
            { "SetCommandGrab", "SASetCommandGrab" },
            { "CameraPunchId", "SACameraPunch" },
            { "SpawnAgent2Id", "SASpawnAgent2" },
            { "ManipDecorChainId", "SAManipDecorChain" },
            { "UpdateHitboxesId", "SAUpdateHitboxes" },
            { "SampleAnimId", "SASampleAnim" },
            { "ForceExtraInputId", "SAForceExtraInputCheck" },
            { "LaunchGrabbedCustomId", "SALaunchGrabbedCustom" },
            { "BaseIdentifier", "StateAction" },
        };

        public static Dictionary<string, string> jumpId = new Dictionary<string, string>
        {
            { "HeightId", "HeightJump" },
            { "HoldId", "HoldJump" },
            { "AirdashId", "AirDashJump" },
            { "KnockbackId", "KnockbackJump" },
            { "BaseIdentifier", "Jump" },
        };

        public static Dictionary<string, string> objectSourceIds = new Dictionary<string, string>
        {
            { "FloatId", "OSFloat" },
            { "Vector2Id", "OSVector2" },
            { "BaseIdentifier", "ObjectSource" },
        };

        // Class names mapped to their TypeID
        public static Dictionary<string, string> classToTypeId = new Dictionary<string, string>();
    }
}
