using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Empire_Rewritten.Resources;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.Emblems
{

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class EmblemPartDef : Def
    {
        //Requirements: 1) .png 2) Texture size 512x512
        private string texturePath;
        public EmblemPartCategory category;

        private Texture2D _textureCache;

        public Texture2D Texture
        {
            get {
                if (_textureCache == null)
                {
                    string path = GetFullPath();
                    if (File.Exists(path))
                    {
                        byte[] data = File.ReadAllBytes(path);
                        _textureCache = new Texture2D(2, 2, TextureFormat.Alpha8, true);
                        _textureCache.LoadImage(data);
                        _textureCache.Compress(true);
                        _textureCache.name = Path.GetFileNameWithoutExtension(path);
                        _textureCache.filterMode = FilterMode.Bilinear;
                        _textureCache.anisoLevel = 2;
                        _textureCache.Apply(true);
                    }
                    else Utils.Logger.Error("File " + path + " not found!");
                }
                return _textureCache;
            }
        }

        public override string ToString()
        {
            return category + ": " + defName + " (" + label + ")";
        }

        private string GetFullPath()
        {
            //TODO Make this better with https://github.com/Danimineiro/CustomMoteMaker/blob/master/Source/DCMM_Settings.cs
            foreach (ModContentPack mcp in LoadedModManager.RunningModsListForReading)
            {
                if (mcp.assemblies.loadedAssemblies.Contains(Assembly.GetExecutingAssembly()))
                {
                    Utils.Logger.Log(mcp.RootDir);
                    //Not quite! Check version and things.
                    return Path.Combine(mcp.RootDir, "1.3/", GenFilePaths.ContentPath<Texture2D>(), texturePath).Replace('\\', '/') + ".png";
                }
            }

            return "";
        }
    }

    public enum EmblemPartCategory
    {
        Icon,
        Pattern,
        Shape
    }
    
}