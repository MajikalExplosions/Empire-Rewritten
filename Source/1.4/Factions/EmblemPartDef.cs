using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Verse;
using Logger = Empire_Rewritten.Utils.Logger;

namespace Empire_Rewritten.Factions
{

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class EmblemPartDef : Def
    {
        // TODO Change this to use GraphicData (see FacilityDef)
        private string texturePath;
        public EmblemPartCategory category;

        private Texture2D _textureCache;

        public Texture2D Texture
        {
            get
            {
                if (_textureCache == null)
                {
                    _textureCache = ContentFinder<Texture2D>.Get(texturePath);
                    if (!_textureCache.isReadable) _textureCache = _duplicateReadable(_textureCache);
                    _textureCache.filterMode = FilterMode.Bilinear;
                    _textureCache.anisoLevel = 2;
                    if (_textureCache.width != Emblem.TEXTURE_SIZE || _textureCache.height != Emblem.TEXTURE_SIZE)
                    {
                        Logger.Warn($"EmblemPartDef {defName} has a texture that is not {Emblem.TEXTURE_SIZE}x{Emblem.TEXTURE_SIZE} pixels (currently {_textureCache.width}x{_textureCache.height}). Resizing to fit.");
                        TextureUtils.Scale(ref _textureCache, Emblem.TEXTURE_SIZE, Emblem.TEXTURE_SIZE);
                        //_textureCache.Resize(Emblem.TEXTURE_SIZE, Emblem.TEXTURE_SIZE);
                    }
                    _textureCache.Apply(true);
                }
                if (!_textureCache.isReadable)
                {
                    Logger.Warn($"Emblem texture is still unreadable.");
                    _textureCache = _duplicateReadable(_textureCache);
                }
                return _textureCache;
            }
        }

        public override string ToString()
        {
            return category + ": " + defName + " (" + label + ")";
        }

        // See https://discussions.unity.com/t/create-modify-texture2d-to-read-write-enabled-at-runtime/141382/4
        private Texture2D _duplicateReadable(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }

    public enum EmblemPartCategory
    {
        Icon,
        Pattern,
        Shape
    }

}