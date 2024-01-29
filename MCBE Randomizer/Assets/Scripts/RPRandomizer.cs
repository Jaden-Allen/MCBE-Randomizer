using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class RPRandomizer : MonoBehaviour
{
    [Header("Main Vars")]
    public TextAsset manifestJson;
    public Slider progressBar;
    public TMP_InputField seedField;
    public Button importPackButton;
    public Button exportPackButton;
    public TMP_Text debugText;

    [Header("Items Toggles")]
    public CustomToggle itemsToggle;
    public CustomToggle toolsToggle;
    public CustomToggle armourToggle;
    public CustomToggle foodToggle;
    [Header("Blocks Toggles")]
    public CustomToggle blocksToggle;
    public CustomToggle animatedToggle;
    public CustomToggle transparentToggle;

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

    string importedPackName = "";

    bool packImported = false;
    bool packGenerating = false;
    int progressIndex = 0;
    int seed = 0;
    string seedString = "";
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

        exportPackButton.interactable = packImported && !packGenerating && (itemsToggle.toggle.isOn || blocksToggle.toggle.isOn);
        importPackButton.interactable = !packGenerating;

        itemsToggle.toggle.interactable = !packGenerating;
        toolsToggle.toggle.interactable = !packGenerating;
        armourToggle.toggle.interactable = !packGenerating;
        foodToggle.toggle.interactable = !packGenerating;

        blocksToggle.toggle.interactable = !packGenerating;
        animatedToggle.toggle.interactable = !packGenerating;
        transparentToggle.toggle.interactable = !packGenerating;
    }
    public void SetSeed()
    {
        if (packGenerating)
        {
            return;
        }
        seed = seedField.text.GetHashCode();
        seedString = seedField.text;
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
                debugText.text = "Pack Imported!";
            }
        }, new string[] { });
    }
    public async void UncompressPack(string path)
    {
        try
        {
            debugText.text = "Extracting Pack this could take a while...";

            progressBar.maxValue = 2;
            progressBar.value = 1;

            await Task.Run(() => ZipFile.ExtractToDirectory(path, packPath, true));
        }
        catch (Exception e)
        {
            Debug.Log("Error Extracting pack" + e);
            debugText.text = "Error Extracting pack";
            return;
        }

        StartCoroutine(GetTextures());
    }
    public void GeneratePack()
    {
        UncompressPack(importedPackPath);
        packGenerating = true;
    }
    public IEnumerator GetTextures()
    {
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
                    var jo = JObject.Parse(content);
                    importedPackName = jo["header"]["name"].ToString();

                    if (content.Contains("resources"))
                    {
                        rpPath = dir;
                        debugText.text = "Found RP Directory!";
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
                if (dir.Contains("blocks") && blocksToggle.toggle.isOn)
                {
                    blocksPathDir = dir;
                    string[] files = Directory.GetFiles(dir);
                    foreach (string file in files)
                    {
                        bool shouldContinue = false;
                        if (file.Contains("png"))
                        {
                            if (!animatedToggle.toggle.isOn)
                            {
                                foreach (string item in BlockTextures.Animated)
                                {
                                    if (file.Contains(item))
                                    {
                                        shouldContinue = true;
                                        break;
                                    }
                                }
                            }
                            if (!transparentToggle.toggle.isOn)
                            {
                                foreach (string item in BlockTextures.Transparent)
                                {
                                    if (file.Contains(item))
                                    {
                                        shouldContinue = true;
                                        break;
                                    }
                                }
                            }
                            if (shouldContinue)
                            {
                                continue;
                            }
                            blockPaths.Add(file);
                            newBlockPaths.Add(file);
                            string fileName = file;
                            fileName = fileName.Replace(dir, "");
                            List<char> charArray = fileName.ToCharArray().ToList<char>();
                            charArray.RemoveAt(0);
                            string finalFilePath = new string(charArray.ToArray());
                            debugText.text = "Found: " + finalFilePath;
                            yield return null;
                        }
                    }
                }
                if (dir.Contains("items") && itemsToggle.toggle.isOn)
                {
                    itemsPathDir = dir;
                    string[] files = Directory.GetFiles(dir);
                    foreach (string file in files)
                    {
                        bool shouldContinue = false;
                        if (file.Contains("png"))
                        {
                            if (!toolsToggle.toggle.isOn)
                            {
                                foreach (string item in ItemTextures.Tools)
                                {
                                    if (file.Contains(item))
                                    {
                                        shouldContinue = true;
                                        break;
                                    }
                                }
                            }
                            if (!toolsToggle.toggle.isOn)
                            {
                                foreach (string item in ItemTextures.Armour)
                                {
                                    if (file.Contains(item))
                                    {
                                        shouldContinue = true;
                                        break;
                                    }
                                }
                            }
                            if (!toolsToggle.toggle.isOn)
                            {
                                foreach (string item in ItemTextures.Foods)
                                {
                                    if (file.Contains(item))
                                    {
                                        shouldContinue = true;
                                        break;
                                    }
                                }
                            }
                            if (shouldContinue)
                            {
                                continue;
                            }

                            itemPaths.Add(file);
                            newItemPaths.Add(file);
                            string fileName = file; 
                            fileName= fileName.Replace(dir, "");
                            List<char> charArray = fileName.ToCharArray().ToList<char>();
                            charArray.RemoveAt(0);
                            string finalFilePath = new string(charArray.ToArray());
                            debugText.text = "Found: " + finalFilePath;
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
        if (blocksToggle.toggle.isOn)
        {
            foreach (string file in blockPaths)
            {
                string randomFilePath = newBlockPaths[RandomizerManager.RandomInt(0, newBlockPaths.Count - 1, seed)];
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
        }
        if (itemsToggle.toggle.isOn)
        {
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
        }
        
        GenerateManifest();

        Directory.Delete(packPath, true);
        ZipFile.CreateFromDirectory(generatedPath, Path.Combine(rootPath, "Randomized.mcpack"));
        debugText.text = "Pack randomizing finished, attempting to open pack!";
        //Application.OpenURL(Path.Combine(rootPath, "Randomized.mcpack"));
        AndroidContentOpenerWrapper.OpenContent(Path.Combine(rootPath, "Randomized.mcpack"));

        packGenerating = false;
    }
    void GenerateManifest()
    {
        string manifest = manifestJson.ToString();
        manifest = manifest.Replace("[NAME]", importedPackName + " - Randomized!");
        manifest = manifest.Replace("[DESCRIPTION]", "A fully randomized resource pack! Seed: " + seedString + ", HashCode: " + seed);
        manifest = manifest.Replace("[UUID1]", Guid.NewGuid().ToString());
        manifest = manifest.Replace("[UUID2]", Guid.NewGuid().ToString());

        File.WriteAllText(Path.Combine(generatedPath, "manifest.json"), manifest);
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
public static class ItemTextures
{
    public static readonly List<string> Tools = new List<string>
    {
        "axe",
        "hoe",
        "pickaxe",
        "shovel",
        "sword"
    };
    public static readonly List<string> Armour = new List<string>
    {
        "chestplate",
        "helmet",
        "leggings",
        "boots"
    };
    public static readonly List<string> Foods = new List<string>
    {
        "beef",
        "pork",
        "chicken",
        "mutton",
        "raw_rabbit",
        "cooked_rabbit",
        "cod",
        "salmon",
        "fish",
        "apple",
        "carrot",
        "potato",
        "melon",
        "stew",
        "beet",
        "berry",
        "popped",
        "flesh",
        "fermented",
        "cookie",
        "cake",
        "pie",
        "bread",
        "kelp"
    };
}
public static class BlockTextures
{
    public static readonly List<string> Animated = new List<string>
    {
        "bubble",
        "fire",
        "water",
        "lava",
        "command",
        "wind",
        "portal",
        "prismarine",
        "skulk",
        "seagrass",
        "smoker_front_on"
    };
    public static readonly List<string> Transparent = new List<string>
    {
        "glass"
    };
}