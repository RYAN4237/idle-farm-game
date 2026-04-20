using UnityEngine;
using UnityEditor;
using System.IO;

public class ImportAudio
{
    public static void Execute()
    {
        string downloadsPath = @"C:\Users\Administrator\Downloads";
        string audioDestPath = @"C:\Users\Administrator\Idle Game\Assets\Audio";
        string sfxDestPath   = audioDestPath + @"\SFX";
        string musicDestPath = audioDestPath + @"\Music";

        // 确保目录存在
        Directory.CreateDirectory(sfxDestPath);
        Directory.CreateDirectory(musicDestPath);

        // 要复制的文件映射：源文件名 -> (目标目录, 新文件名)
        var files = new System.Collections.Generic.Dictionary<string, (string dir, string name)>
        {
            { "sfx-MusicEthereal.ogg", (musicDestPath, "BGM_Ethereal.ogg") },
            { "sfx-Explosion.ogg",     (sfxDestPath,   "SFX_Explosion.ogg") },
            { "sfx-Thruster.ogg",      (sfxDestPath,   "SFX_Thruster.ogg") },
        };

        int copied = 0;
        foreach (var kv in files)
        {
            string src  = Path.Combine(downloadsPath, kv.Key);
            string dest = Path.Combine(kv.Value.dir, kv.Value.name);

            if (File.Exists(src))
            {
                File.Copy(src, dest, overwrite: true);
                Debug.Log($"✅ Copied: {kv.Key} → {dest}");
                copied++;
            }
            else
            {
                Debug.LogWarning($"⚠️ Not found: {src}");
            }
        }

        // 刷新 AssetDatabase 让 Unity 识别新文件
        AssetDatabase.Refresh();
        Debug.Log($"✅ ImportAudio complete. {copied} file(s) copied.");
    }
}
