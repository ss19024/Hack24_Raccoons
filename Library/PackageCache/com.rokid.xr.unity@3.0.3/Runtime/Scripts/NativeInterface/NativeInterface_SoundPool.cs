using System;
using System.IO;
using Rokid.UXR.Module;
using UnityEngine;

namespace Rokid.UXR.Native
{
    public partial class NativeInterface
    {
        public static partial class NativeAPI
        {
            const string _logPrefix = "RKNativeSoundPool: ";

			// Set DEBUG to "true" to enable activity logging
			static bool DEBUG = false;

#if UNITY_ANDROID && !UNITY_EDITOR
			const int _loadPriority = 1;
			const int _sourceQuality = 0;

			static AndroidJavaObject _assetFileDescriptor;
			static AndroidJavaObject _assets;
			static AndroidJavaObject _soundPool = null;
			static bool _hasOBB;
			static int _streamMusic = new AndroidJavaClass("android.media.AudioManager").GetStatic<int>("STREAM_MUSIC");
			
			public static void makeSoundPool(int maxStreams = 16)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "makePool(" + maxStreams + ")");
				
				var context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");

				if (Application.streamingAssetsPath.Substring(Application.streamingAssetsPath.Length - 12) == ".obb!/assets")
				{
					_hasOBB = true;
					int versionCode = context.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getPackageInfo", context.Call<string>("getPackageName"), 0).Get<int>("versionCode");
					_assets = new AndroidJavaClass("com.android.vending.expansion.zipfile.APKExpansionSupport").CallStatic<AndroidJavaObject>("getAPKExpansionZipFile", context, versionCode, 0);
				}
				else
				{
					_hasOBB = false;
					_assets = context.Call<AndroidJavaObject>("getAssets");
				}

				if (_soundPool != null){
					// _soundPool.Call("release");
					Debug.Log(_logPrefix + "_soundPool != null , return directly!");
					return;
				}
					
				_soundPool = new AndroidJavaObject("com.rokid.uxr.base.util.RKAsynSoundPool", maxStreams, _streamMusic, _sourceQuality);
			}

			public static int loadSound(string audioFile, bool usePersistentDataPath = false, Action<int> callback = null)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "load(\"" + audioFile + "\", " + usePersistentDataPath + "\", " + callback + ")");

				if (_soundPool == null)
					throw new InvalidOperationException(_logPrefix + "Use makePool() before load()!");

				if (callback != null)
					_soundPool.Call("setOnLoadCompleteListener", new OnLoadCompleteListener(callback));

				if (usePersistentDataPath)
					return _soundPool.Call<int>("load", Path.Combine(Application.persistentDataPath, audioFile), _loadPriority);

				if (_hasOBB)
					_assetFileDescriptor = _assets.Call<AndroidJavaObject>("getAssetFileDescriptor", Path.Combine("assets", audioFile));
				else
					_assetFileDescriptor = _assets.Call<AndroidJavaObject>("openFd", audioFile);

				return _soundPool.Call<int>("load", _assetFileDescriptor, _loadPriority);
			}

			public static int playSound(int fileID, float leftVolume = 1, float rightVolume = -1, int priority = 1, int loop = 0, float rate = 1)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "play(" + fileID + ", " + leftVolume + ", " + rightVolume + ", " + priority + ", " + loop + ", " + rate + ")");

				if (rightVolume == -1)
					rightVolume = leftVolume;

				return _soundPool.Call<int>("play", fileID, leftVolume, rightVolume, priority, loop, rate);
			}
			
			public static void resumeSound(int streamID)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "resume(" + streamID + ")");
				
				_soundPool.Call("resume", streamID);
			}

			public static void pauseSound(int streamID)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "pause(" + streamID + ")");
				
				_soundPool.Call("pause", streamID);
			}

			public static void stopSound(int streamID)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "stop(" + streamID + ")");
				
				_soundPool.Call("stop", streamID);
			}

			public static bool unloadSound(int fileID)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "unload(" + fileID + ")");

				return _soundPool.Call<bool>("unload", fileID);
			}
			
			public static void releaseSoundPool()
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "releasePool()");

				_soundPool.Call("release");
				_soundPool.Dispose();
				_soundPool = null;
			}

#else

			public static void makeSoundPool(int maxStreams = 16)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "makePool(" + maxStreams + ")");
			}

			public static int loadSound(string audioFile, bool usePersistentDataPath = false, Action<int> callback = null)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "load(\"" + audioFile + "\", " + usePersistentDataPath + "\", " + callback + ")");
				return 1;
			}

			public static int playSound(int fileID, float leftVolume = 1, float rightVolume = -1, int priority = 1, int loop = 0, float rate = 1)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "play(" + fileID + ", " + leftVolume + ", " + rightVolume + ", " + priority + ", " + loop + ", " + rate + ")");

				return 0;
			}
			
			public static void resumeSound(int streamID)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "resume(" + streamID + ")");
			}

			public static void pauseSound(int streamID)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "pause(" + streamID + ")");
			}

			public static void stopSound(int streamID)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "stop(" + streamID + ")");
			}

			public static void releaseSoundPool()
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "releasePool()");
			}

			public static bool unloadSound(int fileID)
			{
				if (DEBUG)
					Debug.Log(_logPrefix + "unload(" + fileID + ")");

				return true;
			}
#endif
        }
    }
}

