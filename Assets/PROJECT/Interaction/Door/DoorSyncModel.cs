using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

public enum EDoorState
{
    Open,
    Closed
}



[RealtimeModel]
public partial class DoorSyncModel
{
    [RealtimeProperty(1, true, true)]
    bool _operateDoor;

    [RealtimeProperty(2, true, true)]
    EDoorState _openOrClosed;
}

/* ----- Begin Normal Autogenerated Code ----- */
public partial class DoorSyncModel : RealtimeModel {
    public bool operateDoor {
        get {
            return _cache.LookForValueInCache(_operateDoor, entry => entry.operateDoorSet, entry => entry.operateDoor);
        }
        set {
            if (this.operateDoor == value) return;
            _cache.UpdateLocalCache(entry => { entry.operateDoorSet = true; entry.operateDoor = value; return entry; });
            InvalidateReliableLength();
            FireOperateDoorDidChange(value);
        }
    }
    
    public EDoorState openOrClosed {
        get {
            return _cache.LookForValueInCache(_openOrClosed, entry => entry.openOrClosedSet, entry => entry.openOrClosed);
        }
        set {
            if (this.openOrClosed == value) return;
            _cache.UpdateLocalCache(entry => { entry.openOrClosedSet = true; entry.openOrClosed = value; return entry; });
            InvalidateReliableLength();
            FireOpenOrClosedDidChange(value);
        }
    }
    
    public delegate void PropertyChangedHandler<in T>(DoorSyncModel model, T value);
    public event PropertyChangedHandler<bool> operateDoorDidChange;
    public event PropertyChangedHandler<EDoorState> openOrClosedDidChange;
    
    private struct LocalCacheEntry {
        public bool operateDoorSet;
        public bool operateDoor;
        public bool openOrClosedSet;
        public EDoorState openOrClosed;
    }
    
    private LocalChangeCache<LocalCacheEntry> _cache = new LocalChangeCache<LocalCacheEntry>();
    
    public enum PropertyID : uint {
        OperateDoor = 1,
        OpenOrClosed = 2,
    }
    
    public DoorSyncModel() : this(null) {
    }
    
    public DoorSyncModel(RealtimeModel parent) : base(null, parent) {
    }
    
    protected override void OnParentReplaced(RealtimeModel previousParent, RealtimeModel currentParent) {
        UnsubscribeClearCacheCallback();
    }
    
    private void FireOperateDoorDidChange(bool value) {
        try {
            operateDoorDidChange?.Invoke(this, value);
        } catch (System.Exception exception) {
            UnityEngine.Debug.LogException(exception);
        }
    }
    
    private void FireOpenOrClosedDidChange(EDoorState value) {
        try {
            openOrClosedDidChange?.Invoke(this, value);
        } catch (System.Exception exception) {
            UnityEngine.Debug.LogException(exception);
        }
    }
    
    protected override int WriteLength(StreamContext context) {
        int length = 0;
        if (context.fullModel) {
            FlattenCache();
            length += WriteStream.WriteVarint32Length((uint)PropertyID.OperateDoor, _operateDoor ? 1u : 0u);
            length += WriteStream.WriteVarint32Length((uint)PropertyID.OpenOrClosed, (uint) _openOrClosed);
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.operateDoorSet) {
                length += WriteStream.WriteVarint32Length((uint)PropertyID.OperateDoor, entry.operateDoor ? 1u : 0u);
            }
            if (entry.openOrClosedSet) {
                length += WriteStream.WriteVarint32Length((uint)PropertyID.OpenOrClosed, (uint) entry.openOrClosed);
            }
        }
        return length;
    }
    
    protected override void Write(WriteStream stream, StreamContext context) {
        var didWriteProperties = false;
        
        if (context.fullModel) {
            stream.WriteVarint32((uint)PropertyID.OperateDoor, _operateDoor ? 1u : 0u);
            stream.WriteVarint32((uint)PropertyID.OpenOrClosed, (uint) _openOrClosed);
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.operateDoorSet || entry.openOrClosedSet) {
                _cache.PushLocalCacheToInflight(context.updateID);
                ClearCacheOnStreamCallback(context);
            }
            if (entry.operateDoorSet) {
                stream.WriteVarint32((uint)PropertyID.OperateDoor, entry.operateDoor ? 1u : 0u);
                didWriteProperties = true;
            }
            if (entry.openOrClosedSet) {
                stream.WriteVarint32((uint)PropertyID.OpenOrClosed, (uint) entry.openOrClosed);
                didWriteProperties = true;
            }
            
            if (didWriteProperties) InvalidateReliableLength();
        }
    }
    
    protected override void Read(ReadStream stream, StreamContext context) {
        while (stream.ReadNextPropertyID(out uint propertyID)) {
            switch (propertyID) {
                case (uint)PropertyID.OperateDoor: {
                    bool previousValue = _operateDoor;
                    _operateDoor = (stream.ReadVarint32() != 0);
                    bool operateDoorExistsInChangeCache = _cache.ValueExistsInCache(entry => entry.operateDoorSet);
                    if (!operateDoorExistsInChangeCache && _operateDoor != previousValue) {
                        FireOperateDoorDidChange(_operateDoor);
                    }
                    break;
                }
                case (uint)PropertyID.OpenOrClosed: {
                    EDoorState previousValue = _openOrClosed;
                    _openOrClosed = (EDoorState) stream.ReadVarint32();
                    bool openOrClosedExistsInChangeCache = _cache.ValueExistsInCache(entry => entry.openOrClosedSet);
                    if (!openOrClosedExistsInChangeCache && _openOrClosed != previousValue) {
                        FireOpenOrClosedDidChange(_openOrClosed);
                    }
                    break;
                }
                default: {
                    stream.SkipProperty();
                    break;
                }
            }
        }
    }
    
    #region Cache Operations
    
    private StreamEventDispatcher _streamEventDispatcher;
    
    private void FlattenCache() {
        _operateDoor = operateDoor;
        _openOrClosed = openOrClosed;
        _cache.Clear();
    }
    
    private void ClearCache(uint updateID) {
        _cache.RemoveUpdateFromInflight(updateID);
    }
    
    private void ClearCacheOnStreamCallback(StreamContext context) {
        if (_streamEventDispatcher != context.dispatcher) {
            UnsubscribeClearCacheCallback(); // unsub from previous dispatcher
        }
        _streamEventDispatcher = context.dispatcher;
        _streamEventDispatcher.AddStreamCallback(context.updateID, ClearCache);
    }
    
    private void UnsubscribeClearCacheCallback() {
        if (_streamEventDispatcher != null) {
            _streamEventDispatcher.RemoveStreamCallback(ClearCache);
            _streamEventDispatcher = null;
        }
    }
    
    #endregion
}
/* ----- End Normal Autogenerated Code ----- */
