
#if UNITY_EDITOR || UNITY_SWITCH
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: AlwaysLinkAssembly]

namespace UnityEngine.InputSystem.Switch
{
    /// <summary>
    /// Adds support for Switch NPad controllers.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
#if UNITY_DISABLE_DEFAULT_INPUT_PLUGIN_INITIALIZATION
    public
#else
    internal
#endif
    static class SwitchConsoleSupport
    {
        static SwitchConsoleSupport()
        {
#if UNITY_EDITOR || UNITY_SWITCH
            InputSystem.RegisterLayout<NPad>(
                matches: new InputDeviceMatcher()
                    .WithInterface("Switch")
                    .WithManufacturer("Nintendo")
                    .WithProduct("Wireless Controller"));
#endif
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeInPlayer()
        {
            s_UnityVersion.ParseUnityVersion();
        }

        struct UnityVersion
        {
            public enum EType
            {
                Unknown,
                Alpha,
                Beta,
                Final,
                Patch,
                Experimental
            }

            // [Version].[Major].[Minor][Type][Revision][Suffix]
            // Example 2020.3.1f1
            // Type being one of:
            //  a: alpha
            //  b: beta
            //  f: public ("final") release
            //  p: patch release(after final)
            //  x: experimental
            private int Version { get; set; }    // Main version (2018, 2019, 2020, 2021 etc)
            private int Major { get; set; }      // Tech-stream or LTS, e.g .1, .2, .3, .4
            private int Minor { get; set; }
            private int Revision { get; set; }
            private EType Type{ get; set; }

            // Must match main version
            public bool IsVersionOrHigher(int versionIn, int majorIn = 0, int minorIn = 0, EType typeIn = EType.Unknown, int revisionIn = 0)
            {
				return Version == versionIn && Major >= majorIn && Minor >= minorIn && ((Type > typeIn || Type == typeIn && Revision >= revisionIn) || Type == EType.Unknown);
            }
            public bool IsMainVersionOrLater(int versionIn)
            {
                return Version >= versionIn;
            }

            // Package min Unity version compatibility is 2019.4 LTS so we don't expect to parse older version strings
            public void ParseUnityVersion()
            {
                string unityVer = Application.unityVersion;

                // Parse up to first '.' to read Unity version (year)
                var startIdx = 0;
                var endIdx = unityVer.IndexOf('.');
                var verStr = unityVer.Substring(startIdx, endIdx - startIdx);

                if(int.TryParse(verStr, out var version))
                {
                    Version = version;
                }
                else
                {
                    Debug.LogWarning("Failed to parse Unity version: " + unityVer);
                }

                // Parse up to second '.' for major revision
                startIdx = endIdx + 1;
                endIdx = unityVer.IndexOf('.', startIdx);
                var majStr = unityVer.Substring(startIdx, endIdx - startIdx);

                if (int.TryParse(majStr, out var major))
                {
                    Major = major;
                }

                // Break down final part of version string, minor revision, type & suffix ([Minor][Type][Revision][Suffix])
                startIdx = endIdx + 1;

                // Check for -dots suffix and discard.
                var suffixIdx = unityVer.IndexOf('-', startIdx);
                string minStr = (suffixIdx != -1) ? unityVer.Substring(startIdx, suffixIdx - startIdx) : unityVer.Substring(startIdx);

                // Check for China suffix ('c') and discard.
                suffixIdx = minStr.IndexOf('c');
                if (suffixIdx != -1)
                {
                    minStr = minStr.Substring(0, suffixIdx);
                }

                Minor = 0;
                Revision = 0;
                Type = EType.Unknown;

                char[] versionTypes = { 'a', 'b', 'f', 'p', 'x' };  // Known version identifiers
                var typeIdx = minStr.IndexOfAny(versionTypes);

                if (typeIdx != -1)
                {
                    var type = minStr[typeIdx];
                    switch (type)
                    {
                        case 'a': Type = EType.Alpha; break;
                        case 'b': Type = EType.Beta; break;
                        case 'f': Type = EType.Final; break;
                        case 'p': Type = EType.Patch; break;
                        case 'x': Type = EType.Experimental; break;
                        default:
                            Debug.LogWarningFormat("Unrecognized type identifier ({0}) in Unity version string: {1}", type, unityVer);
                            break;
                    }

                    if (int.TryParse(minStr.Substring(0, typeIdx), out var minor))
                    {
                        Minor = minor;
                    }
                    else
                    {
                        Debug.LogWarning("Failed to parse minor version from Unity version string: " + unityVer);
                    }

                    if (int.TryParse(minStr.Substring(typeIdx + 1), out var revision))
                    {
                        Revision = revision;
                    }
                    else
                    {
                        Debug.LogWarning("Failed to parse revision from Unity version string: " + unityVer);
                    }
                }

                //Debug.LogFormat("Version: {0} Major: {1} Minor: {2} Type: {3} Revision: {4}\n", Version, Major, Minor, Type, Revision);
            }
        }

        static UnityVersion s_UnityVersion;

        // Attributes queries require a fix in the Switch player available in the following versions onwards
        // (The attributes field is reusing part of the struct that was padding in older versions)
        // 2021.2.0a10, 2021.1.1f1, 2020.3.4f1, 2019.4.24f1, Not supported in 2018 or older
        public static bool NpadAttributesSupported()
        {
            if (s_UnityVersion.IsMainVersionOrLater(2022) // Will be supported in all versions of 2022 onward
                || s_UnityVersion.IsVersionOrHigher(2021, 2, 0, UnityVersion.EType.Alpha, 10)
                || s_UnityVersion.IsVersionOrHigher(2020, 3, 4, UnityVersion.EType.Final, 1)
                || s_UnityVersion.IsVersionOrHigher(2019, 4, 24, UnityVersion.EType.Final, 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
#endif
