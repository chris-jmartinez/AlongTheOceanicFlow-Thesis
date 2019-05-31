/*******************************************
Generate a .png sprite sheet for every
imported .gif under Gif2Sprite/
Allow to split spritesheet if gif is too large
Auto generate .anim file after conversion
********************************************/

/*
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
#if UNITY_EDITOR_WIN
using System.Drawing;
#elif UNITY_EDITOR_OSX

#endif
using System.Collections.Generic;

public class ConvertWindow : EditorWindow
{
    private int splitValue = 1;
    bool autoGenerateAnimation = true;
    private int[] splitOptions = { 1, 2, 4, 8, 16, 32 };
    private string[] splitOptionsDisplay = { "1", "2", "4", "8", "16", "32" };
    private string assetPath;
    private string filename;
    private string animationOutputPath = "Assets/Animations";
    private float frameRate = 10.0f;
    private Gif2Sprite gifProcessor = null;
    public void Init(string path, Gif2Sprite processor)
    {
        assetPath = path;
        filename = assetPath.Substring(assetPath.LastIndexOf('/') + 1, assetPath.LastIndexOf('.') - assetPath.LastIndexOf('/') - 1);
        gifProcessor = processor;
        Show();
    }

    void OnGUI()
    {
        string sourcePath = Application.dataPath + assetPath.Substring(6);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Convert Settings", EditorStyles.boldLabel);
        GUILayout.Label(assetPath, EditorStyles.boldLabel);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        splitValue = EditorGUILayout.IntPopup("Split", splitValue, splitOptionsDisplay, splitOptions);
        autoGenerateAnimation = EditorGUILayout.Toggle("Auto Generate Animation", autoGenerateAnimation);
        #region OutputFolder Selection
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Animation Output Folder");
        GUIStyle alignLeft = new GUIStyle(GUI.skin.button);
        alignLeft.alignment = TextAnchor.MiddleLeft;
        if (GUILayout.Button(animationOutputPath.Length == 0 ? "Click To Select Path" : animationOutputPath, alignLeft))
        {
            string returnPath = EditorUtility.OpenFolderPanel("Select Output Directory", animationOutputPath.Length == 0 ? Application.dataPath : Application.dataPath + animationOutputPath.Substring("Assets".Length), "");
            if (returnPath.Length > 0)
            {
                if (returnPath.Contains(Application.dataPath))
                {
                    returnPath = returnPath.Substring(Application.dataPath.Length);
                    returnPath = "Assets" + returnPath;
                }
                animationOutputPath = returnPath;
            }
        }
        GUILayout.EndHorizontal();
        #endregion

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
        else if (GUILayout.Button("Convert"))
        {
            animationOutputPath += "/" + filename + ".anim";
            if (autoGenerateAnimation)
            {
                // Delete existing animation
                AssetDatabase.DeleteAsset(animationOutputPath);
            }
#if UNITY_EDITOR_WIN
            using (Image gifImg = Image.FromFile(sourcePath))
            {
                System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(gifImg.FrameDimensionsList[0]);
                int frameCount = gifImg.GetFrameCount(dimension);
                byte[] times = gifImg.GetPropertyItem(0x5100).Value;
                int gifDuration = 0;
                for(int i = 0; i < frameCount; i++)
                {
                    gifDuration += BitConverter.ToInt32(times, i * 4);
                }
                frameRate = (float)frameCount / gifDuration * 100.0f;
                for (int k = 0; k < splitValue; k++)
                {
                    int frameCountPerSplit = frameCount / splitValue;
                    int extraFrame = frameCount % splitValue;
                    if(k < extraFrame)
                    {
                        frameCountPerSplit++;
                    }
                    int spriteDimension = (Mathf.CeilToInt(Mathf.Sqrt(frameCountPerSplit)));
                    int finalWidth = spriteDimension * (gifImg.Width + 2);
                    int finalHeight = spriteDimension * (gifImg.Height + 2);
                    using (Bitmap finalSprite = new Bitmap(finalWidth, finalHeight))
                    {
                        using (System.Drawing.Graphics canvas = System.Drawing.Graphics.FromImage(finalSprite))
                        {
                            int row = 0;
                            int col = 0;
                            for (int i = 0; i < frameCountPerSplit; i++)
                            {
                                row = i / spriteDimension;
                                col = i - row * spriteDimension;
                                gifImg.SelectActiveFrame(dimension, i + k * frameCountPerSplit + (k >= extraFrame? extraFrame : 0));
                                using (Bitmap frame = new Bitmap(gifImg))
                                {
                                    canvas.DrawImage(frame, col * (gifImg.Width + 2) + 1, row * (gifImg.Height + 2) + 1);
                                }
                            }
                            canvas.Save();
                            string targetPath = sourcePath.Substring(0, sourcePath.Length - 4) + "-" + k + ".png";
                            finalSprite.Save(targetPath, System.Drawing.Imaging.ImageFormat.Png);
                            // Tried AssetDatabase.Refresh(); seems like it will cause problem in assetimporter
                            // use ImportAsset() instead
                            string importPath = assetPath.Substring(0, assetPath.Length - 4) + "-" + k + ".png";
                            AssetDatabase.ImportAsset(importPath);
                            gifProcessor.OnProcessTexture(importPath, filename, frameCountPerSplit, spriteDimension, gifImg.Width, gifImg.Height, autoGenerateAnimation, animationOutputPath, frameRate);
                        }
                    }
                }
            }
#elif UNITY_EDITOR_OSX

#endif
            Close();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }
}

public class Gif2Sprite : AssetPostprocessor
{
	void OnPostprocessTexture(Texture2D texture)
    {
        string lowerCaseAssetPath = assetPath.ToLower();
        if (!lowerCaseAssetPath.Contains("gif2sprite"))
            return;
        // split gif into sprite sheet
        if (lowerCaseAssetPath.EndsWith(".gif"))
        {
            ConvertWindow convertSettings = EditorWindow.CreateInstance<ConvertWindow>();
            convertSettings.Init(assetPath, this);
        }
    }

    void ConcatAnimation(string sourceSpritePath, string targetPath, float frameRate)
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sourceSpritePath).OfType<Sprite>().ToArray();
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(targetPath);
        EditorCurveBinding curveBinding = new EditorCurveBinding()
        {
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };
        AnimationClipSettings settings = new AnimationClipSettings()
        {
            loopTime = true
        };
        if (clip == null)
        {
            clip = new AnimationClip();
            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                ObjectReferenceKeyframe keyFrame = new ObjectReferenceKeyframe();
                keyFrame.time = i / frameRate;
                keyFrame.value = sprites[i];
                keyFrames[i] = keyFrame;
            }
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
            AssetDatabase.CreateAsset(clip, targetPath);
            AssetDatabase.ImportAsset(targetPath);
        }
        else
        {
            ObjectReferenceKeyframe[] keyFrames = AnimationUtility.GetObjectReferenceCurve(clip, curveBinding);
            List<ObjectReferenceKeyframe> keyFramesList = keyFrames.ToList();
            for(int i = 0; i < sprites.Length; i++)
            {
                ObjectReferenceKeyframe keyFrame = new ObjectReferenceKeyframe();
                keyFrame.time = (i + keyFrames.Length) / frameRate;
                keyFrame.value = sprites[i];
                keyFramesList.Add(keyFrame);
            }
            keyFrames = keyFramesList.ToArray();
            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
            AssetDatabase.ImportAsset(targetPath);
        }
    }

    public void OnProcessTexture(string assetPath, string textureName, int frameCount, int dimension, int frameWidth, int frameHeight, bool autoGenerateAnimation, string animationOutputPath, float frameRate)
    {
        TextureImporter textureImporter = TextureImporter.GetAtPath(assetPath.Substring(0, assetPath.Length - 3) + "png") as TextureImporter;
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;
        List<SpriteMetaData> spriteData = new List<SpriteMetaData>();
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                if (j + i * dimension >= frameCount)
                {
                    // break out of nested forloop
                    i = dimension;
                    break;
                }
                SpriteMetaData metaData = new SpriteMetaData();
                metaData.name = textureName + "_" + (j + i * dimension).ToString();
                // left bottom is [0, 0]
                metaData.rect = new Rect(j * (frameWidth + 2) + 1, (dimension - i - 1) * (frameHeight + 2) + 1, frameWidth, frameHeight);
                spriteData.Add(metaData);
            }
        }
        textureImporter.spritesheet = spriteData.ToArray();
        AssetDatabase.ImportAsset(assetPath);
        if(autoGenerateAnimation)
        {
            ConcatAnimation(assetPath, animationOutputPath, frameRate);
        }
    }
}
*/