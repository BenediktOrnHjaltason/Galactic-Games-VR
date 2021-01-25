using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

[RealtimeModel]
public partial class HandSync_Model
{
    [RealtimeProperty(1, true, true)]
    bool _grabbingGrabHandle;
}

/* ----- Begin Normal Autogenerated Code ----- */
public partial class HandSync_Model : RealtimeModel {
    public bool grabbingGrabHandle {
        get {
            return _cache.LookForValueInCache(_grabbingGrabHandle, entry => entry.grabbingGrabHandleSet, entry => entry.grabbingGrabHandle);
        }
        set {
            if (this.grabbingGrabHandle == value) return;
            _cache.UpdateLocalCache(entry => { entry.grabbingGrabHandleSet = true; entry.grabbingGrabHandle = value; return entry; });
            InvalidateReliableLength();
            FireGrabbingGrabHandleDidChange(value);
        }
    }
    
    public delegate void PropertyChangedHandler<in T>(HandSync_Model model, T value);
    public event PropertyChangedHandler<bool> grabbingGrabHandleDidChange;
    
    private struct LocalCacheEntry {
        public bool grabbingGrabHandleSet;
        public bool grabbingGrabHandle;
    }
    
    private LocalChangeCache<LocalCacheEntry> _cache = new LocalChangeCache<LocalCacheEntry>();
    
    public enum PropertyID : uint {
        GrabbingGrabHandle = 1,
    }
    
    public HandSync_Model() : this(null) {
    }
    
    public HandSync_Model(RealtimeModel parent) : base(null, parent) {
    }
    
    protected override void OnParentReplaced(RealtimeModel previousParent, RealtimeModel currentParent) {
        UnsubscribeClearCacheCallback();
    }
    
    private void FireGrabbingGrabHandleDidChange(bool value) {
        try {
            grabbingGrabHandleDidChange?.Invoke(this, value);
        } catch (System.Exception exception) {
            UnityEngine.Debug.LogException(exception);
        }
    }
    
    protected override int WriteLength(StreamContext context) {
        int length = 0;
        if (context.fullModel) {
            FlattenCache();
            length += WriteStream.WriteVarint32Length((uint)PropertyID.GrabbingGrabHandle, _grabbingGrabHandle ? 1u : 0u);
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.grabbingGrabHandleSet) {
                length += WriteStream.WriteVarint32Length((uint)PropertyID.GrabbingGrabHandle, entry.grabbingGrabHandle ? 1u : 0u);
            }
        }
        return length;
    }
    
    protected override void Write(WriteStream stream, StreamContext context) {
        var didWriteProperties = false;
        
        if (context.fullModel) {
            stream.WriteVarint32((uint)PropertyID.GrabbingGrabHandle, _grabbingGrabHandle ? 1u : 0u);
        } else if (context.reliableChannel) {
            LocalCacheEntry entry = _cache.localCache;
            if (entry.grabbingGrabHandleSet) {
                _cache.PushLocalCacheToInflight(context.updateID);
                ClearCacheOnStreamCallback(context);
            }
            if (entry.grabbingGrabHandleSet) {
                stream.WriteVarint32((uint)PropertyID.GrabbingGrabHandle, entry.grabbingGrabHandle ? 1u : 0u);
                didWriteProperties = true;
            }
            
            if (didWriteProperties) InvalidateReliableLength();
        }
    }
    
    protected override void Read(ReadStream stream, StreamContext context) {
        while (stream.ReadNextPropertyID(out uint propertyID)) {
            switch (propertyID) {
                case (uint)PropertyID.GrabbingGrabHandle: {
                    bool previousValue = _grabbingGrabHandle;
                    _grabbingGrabHandle = (stream.ReadVarint32() != 0);
                    bool grabbingGrabHandleExistsInChangeCache = _cache.ValueExistsInCache(entry => entry.grabbingGrabHandleSet);
                    if (!grabbingGrabHandleExistsInChangeCache && _grabbingGrabHandle != previousValue) {
                        FireGrabbingGrabHandleDidChange(_grabbingGrabHandle);
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
        _grabbingGrabHandle = grabbingGrabHandle;
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
