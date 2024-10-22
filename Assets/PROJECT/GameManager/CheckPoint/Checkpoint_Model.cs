using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

[RealtimeModel]
public partial class Checkpoint_Model
{
    [RealtimeProperty(1, true, true)]
    Color _teamPassedThrough;
}

/* ----- Begin Normal Autogenerated Code ----- */
public partial class Checkpoint_Model : RealtimeModel {
    public UnityEngine.Color teamPassedThrough {
        get {
            return _cache.LookForValueInCache(_teamPassedThrough, entry => entry.teamPassedThroughSet, entry => entry.teamPassedThrough);
        }
        set {
            if (this.teamPassedThrough == value) return;
            _cache.UpdateLocalCache(entry => { entry.teamPassedThroughSet = true; entry.teamPassedThrough = value; return entry; });
            InvalidateReliableLength();
            FireTeamPassedThroughDidChange(value);
        }
    }
    
    public delegate void PropertyChangedHandler<in T>(Checkpoint_Model model, T value);
    public event PropertyChangedHandler<UnityEngine.Color> teamPassedThroughDidChange;
    
    private struct LocalCacheEntry {
        public bool teamPassedThroughSet;
        public UnityEngine.Color teamPassedThrough;
    }
    
    private LocalChangeCache<LocalCacheEntry> _cache = new LocalChangeCache<LocalCacheEntry>();
    
    public enum PropertyID : uint {
        TeamPassedThrough = 1,
    }
    
    public Checkpoint_Model() : this(null) {
    }
    
    public Checkpoint_Model(RealtimeModel parent) : base(null, parent) {
    }
    
    protected override void OnParentReplaced(RealtimeModel previousParent, RealtimeModel currentParent) {
        UnsubscribeClearCacheCallback();
    }
    
    private void FireTeamPassedThroughDidChange(UnityEngine.Color value) {
        try {
            teamPassedThroughDidChange?.Invoke(this, value);
        } catch (System.Exception exception) {
            UnityEngine.Debug.LogException(exception);
        }
    }
    
    protected override int WriteLength(StreamContext context) {
        int length = 0;
        if (context.fullModel) {
            FlattenCache();
            length += WriteStream.WriteBytesLength((uint)PropertyID.TeamPassedThrough, WriteStream.ColorToBytesLength());
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.teamPassedThroughSet) {
                length += WriteStream.WriteBytesLength((uint)PropertyID.TeamPassedThrough, WriteStream.ColorToBytesLength());
            }
        }
        return length;
    }
    
    protected override void Write(WriteStream stream, StreamContext context) {
        var didWriteProperties = false;
        
        if (context.fullModel) {
            stream.WriteBytes((uint)PropertyID.TeamPassedThrough, WriteStream.ColorToBytes(_teamPassedThrough));
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.teamPassedThroughSet) {
                _cache.PushLocalCacheToInflight(context.updateID);
                ClearCacheOnStreamCallback(context);
            }
            if (entry.teamPassedThroughSet) {
                stream.WriteBytes((uint)PropertyID.TeamPassedThrough, WriteStream.ColorToBytes(entry.teamPassedThrough));
                didWriteProperties = true;
            }
            
            if (didWriteProperties) InvalidateReliableLength();
        }
    }
    
    protected override void Read(ReadStream stream, StreamContext context) {
        while (stream.ReadNextPropertyID(out uint propertyID)) {
            switch (propertyID) {
                case (uint)PropertyID.TeamPassedThrough: {
                    UnityEngine.Color previousValue = _teamPassedThrough;
                    _teamPassedThrough = ReadStream.ColorFromBytes(stream.ReadBytes());
                    bool teamPassedThroughExistsInChangeCache = _cache.ValueExistsInCache(entry => entry.teamPassedThroughSet);
                    if (!teamPassedThroughExistsInChangeCache && _teamPassedThrough != previousValue) {
                        FireTeamPassedThroughDidChange(_teamPassedThrough);
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
        _teamPassedThrough = teamPassedThrough;
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
