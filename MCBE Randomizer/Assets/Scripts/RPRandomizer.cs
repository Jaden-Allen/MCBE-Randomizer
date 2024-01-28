using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class RPRandomizer : MonoBehaviour
{
    public TextAsset manifestJson;
    public Slider progressBar;
    public TMP_InputField seedField;
    public Button importPackButton;
    public Button exportPackButton;
    public TMP_Text debugText;

    string rootPath;
    string packPath;
    string generatedPath;
    string mcpackPath;

    string importedPackPath = "";

    string texturesPath;
    string blocksPath;
    string itemsPath;

    string fileType;

    List<string> blockPaths = new List<string>();
    List<string> itemPaths = new List<string>();

    List<string> newBlockPaths = new List<string>();
    List<string> newItemPaths = new List<string>();

    string itemsPathDir = "";
    string blocksPathDir = "";

    bool packImported = false;
    bool packGenerating = false;
    int progressIndex = 0;
    int seed = 0;
    private void Start()
    {
        rootPath = Application.persistentDataPath;
        packPath = Path.Combine(rootPath, "Pack");
        generatedPath = Path.Combine(rootPath, "Randomized");
        texturesPath = Path.Combine(generatedPath, "textures");
        blocksPath = Path.Combine(generatedPath, "textures", "blocks");
        itemsPath = Path.Combine(generatedPath, "textures", "items");
        mcpackPath = Path.Combine(rootPath, "Randomized.mcpack");

#if UNITY_EDITOR
        fileType = "application/octet-stream";
#endif
#if UNITY_ANDROID
        fileType = "application/octet-stream";
#endif
    }
    private void Update()
    {
        progressBar.maxValue = blockPaths.Count + itemPaths.Count;
        progressBar.value = progressIndex;

        if (importedPackPath != "")
        {
            packImported = true;
        }
        else
        {
            packImported = false;
        }

        exportPackButton.interactable = packImported && !packGenerating;
        importPackButton.interactable = !packGenerating;
    }
    public void SetSeed()
    {
        if (packGenerating)
        {
            return;
        }
        seed = seedField.text.GetHashCode();
        Debug.Log(seed);
    }
    public void GenerateEmptyPack()
    {
        if (Directory.Exists(generatedPath))
        {
            Directory.Delete(generatedPath, true);
        }
        if (File.Exists(mcpackPath))
        {
            File.Delete(mcpackPath);
        }

        Directory.CreateDirectory(generatedPath);
        Directory.CreateDirectory(texturesPath);
        Directory.CreateDirectory(blocksPath);
        Directory.CreateDirectory(itemsPath);

        string manifest = manifestJson.ToString();
        manifest = manifest.Replace("[NAME]", "Randomized Pack!");
        manifest = manifest.Replace("[UUID1]", Guid.NewGuid().ToString());
        manifest = manifest.Replace("[UUID2]", Guid.NewGuid().ToString());

        File.WriteAllText(Path.Combine(generatedPath, "manifest.json"), manifest);
    }
    public void ImportTexturePack()
    {
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("Operation was canceled");
            }
            else
            {
                importedPackPath = path;
                debugText.text = path.ToString();
            }
        }, new string[] { });
    }
    public void UncompressPack(string path)
    {
        try
        {
            ZipFile.ExtractToDirectory(path, packPath, true);
            debugText.text = "extracting to directory" + packPath;
        }
        catch
        {
            Debug.Log("Error Extracting pack");
            debugText.text = "Error Extracting pack";
            return;
        }

        StartCoroutine(GetTextures());
    }
    public void GeneratePack()
    {
        UncompressPack(importedPackPath);
    }
    public IEnumerator GetTextures()
    {
        packGenerating= true;
        blockPaths.Clear();
        newBlockPaths.Clear();
        itemPaths.Clear();
        newItemPaths.Clear();

        GenerateEmptyPack();
        string[] directories = Directory.GetDirectories(packPath);
        string rpPath = "";
        foreach (string dir in directories)
        {
            string[] files = Directory.GetFiles(dir);

            foreach (string file in files)
            {
                if (file.Contains("manifest.json"))
                {
                    string content = File.ReadAllText(file);

                    if (content.Contains("resources"))
                    {
                        rpPath = dir;
                        Debug.Log("Found RP Directory! " + dir);
                        debugText.text = "Found RP Directory! " + dir;
                    }
                }
                yield return null;
            }
        }
        if (rpPath != "")
        {

            string texturesPath = Path.Combine(rpPath, "textures");

            string[] texturesDirs = Directory.GetDirectories(texturesPath);
            foreach (string dir in texturesDirs)
            {
                if (dir.Contains("blocks"))
                {
                    blocksPathDir = dir;
                    string[] files = Directory.GetFiles(dir);
                    foreach (string file in files)
                    {
                        if (file.Contains("png"))
                        {
                            blockPaths.Add(file);
                            newBlockPaths.Add(file);
                            debugText.text = file;
                            yield return null;
                        }
                    }
                }
                if (dir.Contains("items"))
                {
                    itemsPathDir = dir;
                    string[] files = Directory.GetFiles(dir);
                    foreach (string file in files)
                    {
                        if (file.Contains("png"))
                        {
                            itemPaths.Add(file);
                            newItemPaths.Add(file);
                            debugText.text = file;
                            yield return null;
                        }
                    }
                }

            }
        }
        StartCoroutine(RandomizePack());
    }
    IEnumerator RandomizePack()
    {
        progressIndex = 0;

        foreach (string file in blockPaths)
        {
            string randomFilePath = newBlockPaths[RandomizerManager.RandomInt(0, newBlockPaths.Count -1, seed)];
            newBlockPaths.Remove(randomFilePath);
            randomFilePath = randomFilePath.Replace(blocksPathDir, "");
            List<char> charArray = randomFilePath.ToCharArray().ToList<char>();
            charArray.RemoveAt(0);
            string finalFilePath = new string(charArray.ToArray());
            File.Copy(file, Path.Combine(blocksPath, finalFilePath), true);
            debugText.text = finalFilePath;

            progressIndex++;

            yield return null;
        }
        foreach (string file in itemPaths)
        {
            string randomFilePath = newItemPaths[RandomizerManager.RandomInt(0, newItemPaths.Count - 1, seed)];
            newItemPaths.Remove(randomFilePath);
            randomFilePath = randomFilePath.Replace(itemsPathDir, "");
            List<char> charArray = randomFilePath.ToCharArray().ToList<char>();
            charArray.RemoveAt(0);
            string finalFilePath = new string(charArray.ToArray());
            File.Copy(file, Path.Combine(itemsPath, finalFilePath), true);
            debugText.text = finalFilePath;
            progressIndex++;

            yield return null;
        }

        Directory.Delete(packPath, true);
        ZipFile.CreateFromDirectory(generatedPath, Path.Combine(rootPath, "Randomized.mcpack"));
        Application.OpenURL(Path.Combine(rootPath, "Randomized.mcpack"));

        packGenerating = false;
    }

}

public static class RandomizerManager
{
    public static int RandomInt(int min, int max, int seed)
    {
        if (seed == 0)
        {
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        }
        else
        {
            UnityEngine.Random.InitState(seed);
        }
        
        return Mathf.RoundToInt((float)max * UnityEngine.Random.value);
    }
}