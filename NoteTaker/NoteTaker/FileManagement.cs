using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NoteTakerProject
{
    static class FileManagement
    {
        //The text file path
        public static string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Notes.txt");
        public static string logPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        public static int Numérotation;

        public static void Numéro()
        {
            if (new FileInfo(logPath).Length == 0)
            {
                //If file doesn't exist it then Numérotation start with 1
                Numérotation = 1;
            }
            else
            {
                // It should check from the last line of the file what is the first number(like 1,2...) in that line before the character ")" then store it
                try
                {
                    using (TextReader reader = File.OpenText(logPath))
                    {
                        string text = File.ReadLines(logPath).Last();
                        string[] bits = text.Split(')');
                        int x = int.Parse(bits[0]);
                        Numérotation = x;
                        Numérotation++;
                    }
                }
                catch(Exception Ex)
                {
                    throw Ex;
                }
            }
        }

        public static void OrganizeNumérotation()
        {
            int NumérotationFromTheUserTextFile, NumérotationInOrder = 1;
            string[] Strings = File.ReadAllLines(logPath);
            for(int i=0; i<Strings.Length; i++)
            {
                using (TextReader reader = File.OpenText(logPath))
                {
                    string text = File.ReadLines(logPath).Skip(i).Take(1).First();
                    string[] bits = text.Split(')');
                    if(int.TryParse(bits[0], out int x)) 
                    {
                        NumérotationFromTheUserTextFile = x;
                        if (NumérotationFromTheUserTextFile != NumérotationInOrder)
                        {
                            //"CultureInfo.InvariantCulture" adapt the variable to the custom user's machine
                            Strings[i] = Strings[i].Replace(NumérotationFromTheUserTextFile.ToString(CultureInfo.InvariantCulture) + ")", NumérotationInOrder.ToString(CultureInfo.InvariantCulture) + ")");
                        }
                        NumérotationInOrder++;
                    }
                }
            }    
            File.WriteAllLines(logPath, Strings);
        }
        public static void InsertValue(string noted)
        {
            List<string> lines = File.ReadAllLines(logPath).ToList();
            
            //Write in the next line after a new line(write at a new line after another line)
            //And a new insert way rather than this (Numérotation + ") " + txtNote.Text )
            lines.Add($"{Numérotation}) {noted}");
            File.WriteAllLines(logPath, lines);
            Numérotation++;
        }

        public static void CreateFile()
        {
            StreamWriter AppText = new StreamWriter(logPath);
            AppText.Close();
        }

        //Check for the "NoteTaker" in the registry of the user
        public static bool IsStartupItem(string key)
        {
            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rkApp.GetValue(key) == null)
                // The value doesn't exist, the application is not set to run at startup
                return false;
            else
                // The value exists, the application is set to run at startup
                return true;
        }
    }
}
