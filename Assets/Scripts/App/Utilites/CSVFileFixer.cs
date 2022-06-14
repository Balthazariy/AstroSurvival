using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TandC.RunIfYouWantToLive.Helpers
{
    public class CSVFileFixer
    {

#if UNITY_EDITOR
        [MenuItem("Helpers/FixAllCSVFiles")]
        private static void FixAllCSVFiles()
        {
              FixFilesInDirectory(new DirectoryInfo(Application.dataPath + "/Resources/Data/Localizations"));

           // FixCSV(Application.dataPath + "/Resources/Data/Localizations/English/InfoPart/Text.csv");

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }


        private static void FixFilesInDirectory(DirectoryInfo dir)
        {
            foreach (var directory in dir.GetDirectories())
            {
                foreach(var file in directory.GetFiles())
                {
                    if(file.FullName.EndsWith(".csv") && !file.Name.Contains("Blocks"))
                    {
                        FixCSV(file.FullName);
                    }
                }

                FixFilesInDirectory(directory);
            }
        }


#endif


        public static void FixCSV(string path)
        {
            var lines = File.ReadAllLines(path);
            
            for(int i = 0; i < lines.Length; i++)
            {
                if(i == 0)
                {
                    lines[i] += ",|";
                }
                else
                {
                    lines[i] += ",";
                }
            }

            File.WriteAllLines(path, lines);
        }

    }
}