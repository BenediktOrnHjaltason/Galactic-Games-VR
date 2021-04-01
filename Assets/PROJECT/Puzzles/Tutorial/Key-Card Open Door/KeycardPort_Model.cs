using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

[RealtimeModel]
public partial class KeycardPort_Model
{
    [RealtimeProperty(1, true, true)]
    bool _occupied;
}

/* ----- Begin Normal Autogenerated Code ----- */
public partial class KeycardPort_Model : RealtimeModel {
    public bool occupied {
        get {
            return _cache.LookForValueInCache(_occupied, entry => entry.occupiedSet, entry => entry.occupied);
        }
        set {
            if (this.occupied == value) return;
            _cache.UpdateLocalCache(entry => { entry.occupiedSet = true; entry.occupied = value; return entry; });
            InvalidateReliableLength();
            FireOccupiedDidChange(value);
        }
    }
    
    public delegate void PropertyChangedHandler<in T>(KeycardPort_Model model, T value);
    public event PropertyChangedHandler<bool> occupiedDidChange;
    
    private struct LocalCacheEntry {
        public bool occupiedSet;
        public bool occupied;
    }
    
    private LocalChangeCache<LocalCacheEntry> _cache = new LocalChangeCache<LocalCacheEntry>();
    
    public enum PropertyID : uint {
        Occupied = 1,
    }
    
    public KeycardPort_Model() : this(null) {
    }
    
    public KeycardPort_Model(RealtimeModel parent) : base(null, parent) {
    }
    
    protected override void OnParentReplaced(RealtimeModel previousParent, RealtimeModel currentParent) {
        UnsubscribeClearCacheCallback();
    }
    
    private void FireOccupiedDidChange(bool value) {
        try {
            occupiedDidChange?.Invoke(this, value);
        } catch (System.Exception exception) {
            UnityEngine.Debug.LogException(exception);
        }
    }
    
    protected override int WriteLength(StreamContext context) {
        int length = 0;
        if (context.fullModel) {
            FlattenCache();
            length += WriteStream.WriteVarint32Length((uint)PropertyID.Occupied, _occupied ? 1u : 0u);
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.occupiedSet) {
                length += WriteStream.WriteVarint32Length((uint)PropertyID.Occupied, entry.occupied ? 1u : 0u);
            }
        }
        return length;
    }
    
    protected override void Write(WriteStream stream, StreamContext context) {
        var didWriteProperties = false;
        
        if (context.fullModel) {
            stream.WriteVarint32((uint)PropertyID.Occupied, _occupied ? 1u : 0u);
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.occupiedSet) {
                _cache.PushLocalCacheToInflight(context.updateID);
                ClearCacheOnStreamCallback(context);
            }
            if (entry.occupiedSet) {
                stream.WriteVarint32((uint)PropertyID.Occupied, entry.occupied ? 1u : 0u);
                didWriteProperties = true;
            }
            
            if (didWriteProperties) InvalidateReliableLength();
        }
    }
    
    protected override void Read(ReadStream stream, StreamContext context) {
        while (stream.ReadNextPropertyID(out uint propertyID)) {
            switch (propertyID) {
                case (uint)PropertyID.Occupied: {
                    bool previousValue = _occupied;
                    _occupied = (stream.ReadVarint32() != 0);
                    bool occupiedExistsInChangeCache = _cache.ValueExistsInCache(entry => entry.occupiedSet);
                    if (!occupiedExistsInChangeCache && _occupied != previousValue) {
                        FireOccupiedDidChange(_occupied);
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
        _occupied = occupied;
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
