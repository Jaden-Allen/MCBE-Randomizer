using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Drawing;
using UnityEngine;


public class Manager : MonoBehaviour
{
    public string importedPackPath;

#if UNITY_STANDALONE_WIN
    public void ImportTexturePack()
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select a Resource Pack";
        ofd.Filter = "Mcpack (*.mcpack)|*.mcpack|Mcaddon (*.mcaddon*)|*.mcaddon*";

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            importedPackPath = ofd.FileName;
        }
    }
#endif
#if UNITY_ANDROID
    public void ImportTexturePack()
    {
        string fileType = NativeFilePicker.ConvertExtensionToFileType("mcpack");

        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("Operation was canceled");
            }
            else
            {
                importedPackPath = path;
                Debug.Log("Picked File: " + importedPackPath);
            }
        }, new string[] { "application/octet-stream" });
    }
    void SetPath()
    {

    }
#endif
}


