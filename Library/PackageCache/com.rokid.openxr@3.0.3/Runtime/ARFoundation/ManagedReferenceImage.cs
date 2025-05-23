﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features
{
    [StructLayout(LayoutKind.Sequential)]
    struct ManagedReferenceImage : IDisposable
    {
        public ManagedReferenceImage(XRReferenceImage referenceImage)
        {
            guid = referenceImage.guid;
            textureGuid = referenceImage.textureGuid;
            size = referenceImage.specifySize ? referenceImage.size : Vector2.zero;
            name = GCHandle.ToIntPtr(GCHandle.Alloc(referenceImage.name));
            texture = GCHandle.ToIntPtr(GCHandle.Alloc(referenceImage.texture));
        }

        public unsafe XRReferenceImage ToReferenceImage()
        {
            Vector2? maybeSize;
            if (size.x > 0)
            {
                maybeSize = size;
            }
            else
            {
                maybeSize = null;
            }

            return new XRReferenceImage(
                AsSerializedGuid(guid),
                AsSerializedGuid(textureGuid),
                maybeSize,
                ResolveGCHandle<string>(name),
                ResolveGCHandle<Texture2D>(texture));
        }

        public void Dispose()
        {
            GCHandle.FromIntPtr(texture).Free();
            texture = IntPtr.Zero;

            GCHandle.FromIntPtr(name).Free();
            name = IntPtr.Zero;
        }

        unsafe SerializableGuid AsSerializedGuid(Guid guid)
        {
            TrackableId trackableId;
            *(Guid*)&trackableId = guid;
            return new SerializableGuid(trackableId.subId1, trackableId.subId2);
        }

        static T ResolveGCHandle<T>(IntPtr ptr) where T : class => (ptr == IntPtr.Zero) ? null : GCHandle.FromIntPtr(ptr).Target as T;

        public Guid guid;
        public Guid textureGuid;
        public Vector2 size;
        public IntPtr name;
        public IntPtr texture;
    }

    static class ManagedReferenceImageExtensions
    {
        public static List<ManagedReferenceImage> ToArrayList(this XRReferenceImageLibrary library)
        {
            var managedReferenceImageList = new List<ManagedReferenceImage>();
            for (var i = 0; i < library.count; ++i)
            {
                managedReferenceImageList.Add(new ManagedReferenceImage(library[i]));
            }

            return managedReferenceImageList;
        }
    }
}