﻿#define DEBUG_FRAME_DELAY
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lockstep.ECS;
using Lockstep.Logging;
using Lockstep.Math;
using Lockstep.Serialization;
using Lockstep.Util;
using NetMsg.Common;
using Logger = Lockstep.Logging.Logger;

namespace Lockstep.Game {
    public class SimulationService : BaseGameService, ISimulation {
        public static SimulationService Instance { get; private set; }
        public SimulationService(){
            Instance = this;
        }
        
        public World World => _world;
        private Contexts _context;
        private GameStateContext _gameStateContext => _context.gameState;
        private GameLog GameLog = new GameLog();
        private byte _localActorId;

        public bool Running {
            get;
            set;
        }
        private IServiceContainer _services;
        private float _tickDt;
        private float _accumulatedTime;
        private World _world;
        private FrameBuffer cmdBuffer;
        private int _localTick;
        private int _gameId;
        private List<long> allHashCodes = new List<long>();
        private int firstHashTick = 0;

        private byte[] _allActors;
        private int _actorCount;
        private int CurTick = 0;

        
        public override void DoStart(){
            
            cmdBuffer = new FrameBuffer();
            if (_constStateService.IsVideoMode) {
                FrameBuffer.SnapshotFrameInterval = _constStateService.SnapshotFrameInterval;
            }
        }

//重播 消息
        private Msg_RepMissFrame _videoFrames;

        private void OnEvent_BorderVideoFrame(object param){
            _videoFrames = param as Msg_RepMissFrame;
        }

        void OnEvent_OnServerFrame(object param){
            var msg = param as Msg_ServerFrames;
            cmdBuffer.PushServerFrames(msg.frames);
        }

        void OnEvent_OnServerMissFrame(object param){
            Debug.Log($"OnEvent_OnServerMissFrame");
            var msg = param as Msg_RepMissFrame;
            cmdBuffer.PushServerFrames(msg.frames, false);
            _networkService.SendMissFrameRepAck(cmdBuffer.GetMissServerFrameTick());
        }

        void OnEvent_OnGameCreate(object param){
            if (param is Msg_G2C_Hello msg) {
                OnGameCreate(60, msg.LocalId, msg.UserCount);
            }
            if (param is Msg_G2C_GameStartInfo smsg) {
                OnGameCreate(60, 0, smsg.UserCount);
            }
            EventHelper.Trigger(EEvent.SimulationInit, null);
        }

        void OnEvent_OnAllPlayerFinishedLoad(object param){
            Debug.Log($"OnEvent_OnAllPlayerFinishedLoad");
            StartSimulate();
        }

        void OnEvent_LevelLoadDone(object param){
            Debug.Log($"OnEvent_LevelLoadDone " + _constStateService.IsReconnecting);
            if (_constStateService.IsReconnecting || _constStateService.IsVideoMode) {
                StartSimulate();
            }
        }

        public void StartSimulate(){
            if (Running) return;
            _world.StartSimulate();
            Running = true;
            SetPurchaseTimestamp();
            Debug.Log($"Game Start");
            EventHelper.Trigger(EEvent.SimulationStart, null);
        }

        public override void DoAwake(IServiceContainer services){
            _services = services;
            _context = Contexts.sharedInstance;
        }

        public const int MinMissFrameReqTickDiff = 10;

        public const int MaxSimulationMsPerFrame = 20;

        private bool IsTimeout(){
            return LTime.realtimeSinceStartup > _frameDeadline;
        }

        private float _frameDeadline;

        public float timestampOnPurcue;
        public int tickOnPursue;

        public bool PursueServer(int minTickToBackup){
            if (_world.Tick >= cmdBuffer.curServerTick)
                return true;
            while (_world.Tick <= cmdBuffer.curServerTick) {
                var tick = _world.Tick;
                var sFrame = cmdBuffer.GetServerFrame(tick);
                if (sFrame == null)
                    return false;
                cmdBuffer.PushLocalFrame(sFrame);
                Simulate(sFrame, tick >= minTickToBackup);
                if (IsTimeout()) {
                    return false;
                }
            }

            SetPurchaseTimestamp();
            return true;
        }

        private void SetPurchaseTimestamp(){
            var ping = 35;
            var tickClientShouldPredict = 2; //(ping * 2) / NetworkDefine.UPDATE_DELTATIME + 1;
            tickOnPursue = _world.Tick + tickClientShouldPredict;
            timestampOnPurcue = LTime.realtimeSinceStartup;
        }

        public float GetPursueProgress(){
            return _world.Tick * 1.0f / cmdBuffer.curServerTick;
        }

        public bool IsFinishPursue = false;
        public override void DoUpdate(int deltaTimeMs){
            if (!Running) {
                return;
            }

            if (_constStateService.IsVideoMode) {
                return;
            }

            //cmdBuffer.Ping = _gameMsgService.Ping;
            cmdBuffer.UpdateFramesInfo();
            var missFrameTick = cmdBuffer.GetMissServerFrameTick();
            //客户端落后服务器太多帧 请求丢失帧
            if (cmdBuffer.IsNeedReqMissFrame()) {
                _networkService.SendMissFrameReq(missFrameTick);
            }

            //if (!cmdBuffer.CanExecuteNextFrame()) { //因为网络问题 需要等待服务器发送确认包 才能继续往前
            //    return;
            //}
            _frameDeadline = LTime.realtimeSinceStartup + MaxSimulationMsPerFrame;

            var minTickToBackup = missFrameTick - FrameBuffer.SnapshotFrameInterval;
            //追帧 无输入
            var isPursueServer = !PursueServer(minTickToBackup);
            if (isPursueServer) {
                _constStateService.IsPursueFrame = true;
                Debug.Log($"PurchaseServering curTick:" + _world.Tick);
                EventHelper.Trigger(EEvent.PursueFrameProcess,GetPursueProgress());
                return;
            }

            if (_constStateService.IsPursueFrame) {
                EventHelper.Trigger(EEvent.PursueFrameDone);
            }
            _constStateService.IsPursueFrame = false;

            var frameDeltaTime = (LTime.realtimeSinceStartup - timestampOnPurcue) * 1000;
            var targetTick =(float) System.Math.Ceiling(frameDeltaTime / NetworkDefine.UPDATE_DELTATIME) + tickOnPursue;
            //正常跑帧
            while (_world.Tick < targetTick) {
                var curTick = _world.Tick;
                cmdBuffer.UpdateFramesInfo();
                //校验服务器包  如果有预测失败 则需要进行回滚
                if (cmdBuffer.IsNeedRevert) {
                    _world.RollbackTo(cmdBuffer.nextTickToCheck, missFrameTick);
                    _world.CleanUselessSnapshot(System.Math.Min(cmdBuffer.nextTickToCheck - 1, _world.Tick));

                    minTickToBackup = System.Math.Max(minTickToBackup, _world.Tick + 1);
                    while (_world.Tick < missFrameTick) {
                        var sFrame = cmdBuffer.GetServerFrame(_world.Tick);
                        Logging.Debug.Assert(sFrame != null && sFrame.tick == _world.Tick,
                            $" logic error: server Frame  must exist tick {_world.Tick}");
                        //服务器超前 客户端 应该追上去 将服务器中的输入作为客户端输入
                        cmdBuffer.PushLocalFrame(sFrame);
                        Simulate(sFrame, _world.Tick >= minTickToBackup);
                    }

                    while (_world.Tick < curTick) {
                        var frame = cmdBuffer.GetLocalFrame(_world.Tick);
                        FillInputWithLastFrame(frame); //加上输入预判 减少回滚
                        Logging.Debug.Assert(frame != null && frame.tick == _world.Tick,
                            $" logic error: local frame must exist tick {_world.Tick}");
                        Predict(frame, _world.Tick > minTickToBackup);
                    }
                }

                {
                    if (_world.Tick == curTick) { //当前帧 没有被执行 需要执行之
                        ServerFrame cFrame = null;
                        var sFrame = cmdBuffer.GetServerFrame(_world.Tick);
                        if (sFrame != null) {
                            cFrame = sFrame;
                        }
                        else {
                            var input = new Msg_PlayerInput(curTick, _localActorId, _inputService.GetInputCmds());
                            cFrame = new ServerFrame();
                            var inputs = new Msg_PlayerInput[_actorCount];
                            inputs[_localActorId] = input;
                            cFrame.Inputs = inputs;
                            cFrame.tick = curTick;
                            FillInputWithLastFrame(cFrame);
#if DEBUG_FRAME_DELAY
                            input.timeSinceStartUp = LTime.realtimeSinceStartup;
#endif
                            if (curTick > cmdBuffer.maxServerTickInBuffer) { //服务器的输入还没到 需要同步输入到服务器
                                SendInput(input);
                            }
                        }

                        cmdBuffer.PushLocalFrame(cFrame);
                        Predict(cFrame);
                    }
                }
            } //end of while(_world.Tick < targetTick)

            CheckAndSendHashCodes();
        }


        private void SendInput(Msg_PlayerInput input){
            //TODO 合批次 一起发送 且连同历史未确认包一起发送
            _networkService.SendInput(input);
        }

        private bool isInitVideo = false;

        public void JumpTo(int tick){
            if (tick + 1 == _world.Tick || tick == _world.Tick) return;
            tick = LMath.Min(tick, _videoFrames.frames.Length - 1);
            var time = LTime.realtimeSinceStartup + 0.05f;
            if (!isInitVideo) {
                _constStateService.IsVideoLoading = true;
                while (_world.Tick < _videoFrames.frames.Length) {
                    var sFrame = _videoFrames.frames[_world.Tick];
                    Simulate(sFrame, true);
                    if (LTime.realtimeSinceStartup > time) {
                        EventHelper.Trigger(EEvent.VideoLoadProgress,_world.Tick*1.0f/_videoFrames.frames.Length);
                        return;
                    }
                }
                _constStateService.IsVideoLoading = false;
                EventHelper.Trigger(EEvent.VideoLoadDone);
                isInitVideo = true;
            }

            if (_world.Tick > tick) {
                _world.RollbackTo(tick, _videoFrames.frames.Length, false);
            }

            while (_world.Tick <= tick) {
                var sFrame = _videoFrames.frames[_world.Tick];
                Simulate(sFrame, false);
            }

            _viewService.RebindAllEntities();
            timestampOnLastJumpTo = LTime.timeSinceLevelLoad;
            tickOnLastJumpTo = tick;
        }

        private float timestampOnLastJumpTo;
        private int tickOnLastJumpTo;

        public void RunVideo(){
            if (tickOnLastJumpTo == _world.Tick) {
                timestampOnLastJumpTo = LTime.realtimeSinceStartup;
                tickOnLastJumpTo = _world.Tick;
            }

            var frameDeltaTime = (LTime.timeSinceLevelLoad - timestampOnLastJumpTo) * 1000;
            var targetTick = System.Math.Ceiling(frameDeltaTime / NetworkDefine.UPDATE_DELTATIME) + tickOnLastJumpTo;
            while (_world.Tick <= targetTick) {
                if (_world.Tick < _videoFrames.frames.Length) {
                    var sFrame = _videoFrames.frames[_world.Tick];
                    Simulate(sFrame, false);
                }
                else {
                    break;
                }
            }
        }

        private void Simulate(ServerFrame frame, bool isNeedGenSnap = true){
            ProcessInputQueue(frame);
            _world.Simulate(isNeedGenSnap);
            var tick = _world.Tick;
            cmdBuffer.SetClientTick(tick);
            SetHashCode();
            if (isNeedGenSnap && tick % FrameBuffer.SnapshotFrameInterval == 0) {
                _world.CleanUselessSnapshot(System.Math.Min(cmdBuffer.nextTickToCheck - 1, _world.Tick));
            }
        }

        private void Predict(ServerFrame frame, bool isNeedGenSnap = true){
            ProcessInputQueue(frame);
            _world.Predict(isNeedGenSnap);
            var tick = _world.Tick;
            cmdBuffer.SetClientTick(tick);
            SetHashCode();
            //清理无用 snapshot
            if (isNeedGenSnap && tick % FrameBuffer.SnapshotFrameInterval == 0) {
                _world.CleanUselessSnapshot(System.Math.Min(cmdBuffer.nextTickToCheck - 1, _world.Tick));
            }
        }

        public override void DoDestroy(){
            Running = false;
        }

        public void OnGameCreate(int targetFps, byte localActorId, byte actorCount,
            bool isNeedRender = true){
            FrameBuffer.DebugMainActorID = localActorId;
            var allActors = new byte[actorCount];
            for (byte i = 0; i < actorCount; i++) {
                allActors[i] = i;
            }

            //初始化全局配置
            _gameConstStateService.allActorIds = allActors;
            _gameConstStateService.actorCount = allActors.Length;

            _localActorId = localActorId;
            _allActors = allActors;

            _localTick = 0;
            GameLog.LocalActorId = localActorId;
            GameLog.AllActorIds = allActors;

            _actorCount = allActors.Length;
            _tickDt = 1000f / targetFps;
            _world = new World(_context, _services, allActors, new GameLogicSystems(_context, _services));
        }

        private void FillInputWithLastFrame(ServerFrame frame){
            int tick = frame.tick;
            var inputs = frame.Inputs;
            var lastFrameInputs = tick == 0 ? null : cmdBuffer.GetFrame(tick - 1)?.Inputs;
            var curFrameInput = inputs[_localActorId];
            //将所有角色 给予默认的输入
            for (int i = 0; i < _actorCount; i++) {
                inputs[i] = new Msg_PlayerInput(tick, _allActors[i], lastFrameInputs?[i]?.Commands?.ToList());
            }

            inputs[_localActorId] = curFrameInput;
        }


        public void CheckAndSendHashCodes(){
            if (cmdBuffer.nextTickToCheck > firstHashTick) {
                var count = LMath.Min(allHashCodes.Count, (int) (cmdBuffer.nextTickToCheck - firstHashTick),
                    (480 / 8));
                if (count > 0) {
                    _networkService.SendHashCodes(firstHashTick, allHashCodes, 0, count);
                    firstHashTick = firstHashTick + count;
                    allHashCodes.RemoveRange(0, count);
                }
            }
        }

        public void SetHash(int tick, long hash){
            if (tick < firstHashTick) {
                return;
            }

            var idx = (int) (tick - firstHashTick);
            if (allHashCodes.Count <= idx) {
                for (int i = 0; i < idx + 1; i++) {
                    allHashCodes.Add(0);
                }
            }

            allHashCodes[idx] = hash;
        }

        public void SetHashCode(){
            var nextTick = _world.Tick;
            var iTick = (int) nextTick - 1;
            for (int i = allHashCodes.Count; i <= iTick; i++) {
                allHashCodes.Add(0);
            }

            var hash = _gameStateContext.hashCode.value;
            allHashCodes[iTick] = _gameStateContext.hashCode.value;
            SetHash(nextTick - 1, hash);
        }


        public void DumpGameLog(Stream outputStream, bool closeStream = true){
            var serializer = new Serializer();
            serializer.Write(_gameStateContext.hashCode.value);
            serializer.Write(_gameStateContext.tick.value);
            outputStream.Write(serializer.Data, 0, serializer.Length);

            GameLog.WriteTo(outputStream);

            if (closeStream) {
                outputStream.Close();
            }
        }

        private void ProcessInputQueue(ServerFrame frame){
            var inputs = frame.Inputs;
            foreach (var input in inputs) {
                GameLog.Add(frame.tick, input);
                if (input.Commands == null) continue;
                foreach (var command in input.Commands) {
                    Logger.Trace(this, input.ActorId + " >> " + input.Tick + ": " + input.Commands.Count());
                    var inputEntity = _context.input.CreateEntity();
                    _inputService.Execute(command, inputEntity);
                    inputEntity.AddTick(input.Tick);
                    inputEntity.AddActorId(input.ActorId);
                    inputEntity.isDestroyed = true;
                }
            }
        }
    }
}