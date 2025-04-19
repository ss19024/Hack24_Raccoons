using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features
{
    public sealed class RokidImageDatabase : MutableRuntimeReferenceImageLibrary
    {
        public const string dataStoreKey = "com.rokid.openxr.arfoundation";

        private static List<ManagedReferenceImage> ManagedReferenceImages = new List<ManagedReferenceImage>();

        struct AddImageJob : IJob
        {
            [DeallocateOnJobCompletion] 
            public NativeArray<byte> ImageName;
            
            [DeallocateOnJobCompletion] 
            public NativeArray<byte> ImageData;
            
            public int WidthInPixels;

            public int HeightInPixels;

            public float WidthInMeters;

            public float HeightInMeters;

            public ManagedReferenceImage ManagedReferenceImage;

            string GetStringFromBytes(NativeArray<byte> nativeArray)
            {
                string name = Encoding.UTF8.GetString(nativeArray.ToArray());
                nativeArray.Dispose();
                return name;
            }

            public unsafe void Execute()
            {
                string nameStr = GetStringFromBytes(ImageName);
                Debug.Log("RK-ARFoundation-Marker: AddImageJob Execute() " +
                          ", ImageData.size=" + ImageData.ToArray().Length +
                          ", WidthInPixels=" + WidthInPixels + ", HeightInPixels=" + HeightInPixels +
                          ", WidthInMeters=" + WidthInMeters + ", HeightInMeters=" +HeightInMeters  + 
                          ", nameStr=" + nameStr);

                ManagedReferenceImages.Add(ManagedReferenceImage);
                ImageTracking_AddImage(nameStr, ImageData.ToArray(), WidthInPixels, HeightInPixels,
                    WidthInPixels, WidthInMeters, HeightInMeters);
            }

            [DllImport("rokid_openxr_api")]
            static unsafe extern int ImageTracking_AddImage(string imageName,
                byte[] pixels, int widthInPixels, int heightInPixels, int strideInPixels,
                float widthInMeters, float heightInMeters);
        }


        static byte[] GetLibraryData(XRReferenceImageLibrary library)
        {
            if (library.dataStore.TryGetValue(dataStoreKey, out var bytes))
            {
                Debug.Log("RK-ARFoundation-Marker: ImageDatabase get library bytes.size=" + bytes.Length);
                return bytes;
            }

            Debug.LogError("RK-ARFoundation-Marker: ImageDatabase can't get library");
            return null;
        }

        public unsafe RokidImageDatabase(XRReferenceImageLibrary serializedLibrary)
        {
            if (serializedLibrary == null)
            {
                Debug.Log("RK-ARFoundation-Marker: RokidImageDatabase serializedLibrary is null, so create db in runtime");
                ImageTracking_SetDatabase(null, 0);
                return;
            }

            Debug.Log("RK-ARFoundation-Marker: New RokidImageDatabase,  serializedLibrary.count=" +
                      serializedLibrary.count + ", dataStore.Count=" + serializedLibrary.dataStore.Count);

            var libraryBlob = GetLibraryData(serializedLibrary);
            if (libraryBlob == null || libraryBlob.Length == 0)
            {
                Debug.LogError($"[Error]RK-ARFoundation-Marker: RokidImageDatabase Failed to load {nameof(XRReferenceImageLibrary)} '{serializedLibrary.name}': library does not contain any data.");
                return;
            }

            ManagedReferenceImages = serializedLibrary.ToArrayList();
            Debug.Log("RK-ARFoundation-Marker: ManagedReferenceImages=" + ManagedReferenceImages.Count);

            ImageTracking_Destroy();
            
            ImageTracking_SetDatabase(libraryBlob, libraryBlob.Length);
            Debug.Log("RK-ARFoundation-Marker: ImageTracking_SetDatabase done libraryBlob size =" + libraryBlob.Length);
        }

        ~RokidImageDatabase()
        {
            int n = count;
            for (int i = 0; i < n; ++i)
            {
                ManagedReferenceImages[i].Dispose();
            }
        }

        static readonly TextureFormat[] k_SupportedFormats =
        {
            TextureFormat.Alpha8,
            TextureFormat.R8,
            TextureFormat.RFloat,
            TextureFormat.RGB24,
            TextureFormat.RGBA32,
            TextureFormat.ARGB32,
            TextureFormat.BGRA32,
        };

        public override bool supportsValidation => false;

        public override int supportedTextureFormatCount => k_SupportedFormats.Length;

        protected override TextureFormat GetSupportedTextureFormatAtImpl(int index) => k_SupportedFormats[index];

        unsafe NativeArray<byte> GetUTF8Bytes(string s)
        {
            var byteCount = Encoding.UTF8.GetByteCount(s);
            var utf8Bytes = new NativeArray<byte>(byteCount + 1, Allocator.Persistent);
            fixed (char* chars = s)
            {
                try
                {
                    Encoding.UTF8.GetBytes(chars, s.Length, (byte*)utf8Bytes.GetUnsafePtr(), byteCount);
                }
                catch
                {
                    utf8Bytes.Dispose();
                    throw;
                }
            }

            return utf8Bytes;
        }


        protected override AddReferenceImageJobState ScheduleAddImageWithValidationJobImpl(NativeSlice<byte> imageBytes,
            Vector2Int sizeInPixels, TextureFormat format, XRReferenceImage referenceImage, JobHandle inputDeps)
        {
            Debug.Log("RK-ARFoundation-Marker: ScheduleAddImageWithValidationJobImpl imageBytes.size="+imageBytes.Length
                                    +", sizeInPixels="+sizeInPixels+", format="+format+", refImage guid="+referenceImage.guid
                                    +", name="+referenceImage.name+", size="+referenceImage.size+", texture="+referenceImage.texture+", textureGuid="+referenceImage.textureGuid);
            

            var grayscaleImage = new NativeArray<byte>(
                sizeInPixels.x * sizeInPixels.y,
                Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);

            inputDeps = ConversionJob.Schedule(imageBytes, sizeInPixels, format, grayscaleImage, inputDeps);

            inputDeps = new AddImageJob
            {
                ManagedReferenceImage = new ManagedReferenceImage(referenceImage),
                ImageName = GetUTF8Bytes(referenceImage.guid.ToString().Replace('-'.ToString(), "")),
                ImageData = grayscaleImage,
                WidthInPixels = sizeInPixels.x,
                HeightInPixels = sizeInPixels.y,
                WidthInMeters = referenceImage.size.x,
                HeightInMeters = referenceImage.size.y
            }.Schedule(inputDeps);

            // do not support validation
            return CreateAddJobState(IntPtr.Zero, inputDeps);
        }

        protected override JobHandle ScheduleAddImageJobImpl(
            NativeSlice<byte> imageBytes,
            Vector2Int sizeInPixels,
            TextureFormat format,
            XRReferenceImage referenceImage,
            JobHandle inputDeps)
        {
            return ScheduleAddImageWithValidationJobImpl(imageBytes, sizeInPixels, format, referenceImage, inputDeps).jobHandle;
        }

        protected override XRReferenceImage GetReferenceImageAt(int index)
        {
            XRReferenceImage referenceImage = ManagedReferenceImages[index].ToReferenceImage();
            // Debug.Log("RK-ARFoundation-Marker: ImageDatabase GetReferenceImageAt index="+index+", image name="+referenceImage.name+", guid="+referenceImage.guid);
            return referenceImage;
        }

        public override int count
        {
            get
            {
                return ManagedReferenceImages.Count;
            }
        }
                    
        [DllImport("rokid_openxr_api")]
        static unsafe extern int ImageTracking_SetDatabase(byte[] imageDatabase, int size);
        
        [DllImport("rokid_openxr_api")]
        static extern void ImageTracking_Destroy();
    }
}