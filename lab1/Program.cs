using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace lab1 {
    
    [Serializable]
    class Comparator : IComparer<string> {

        public int Compare(string s1, string s2) {
            if (s1.Length < s2.Length) {
                return -1;
            }
            if (s1.Length > s2.Length) {
                return 1;
            }
            return s1.CompareTo(s2);
        }
    }

    static class Program {

        public static void Main(string[] args) {
            PrintInfo(args[0], 0);
            DirectoryInfo directoryInfo = new DirectoryInfo(args[0]);
            Console.WriteLine("\n" + directoryInfo.GetOldest());
            SortedDictionary<string, int> dictionary = GetCollection(args[0]);
            Serialize(dictionary);
            dictionary = Deserialize();
            foreach (var element in dictionary) {
                Console.WriteLine(element.Key + " -> " + element.Value);
            }
        }

        private static void PrintInfo(string currentPath, int depth) {
            List<string> files = new List<string>(Directory.GetFiles(currentPath));
            List<string> directories = new List<string>(Directory.GetDirectories(currentPath));
            PrintFilesInfo(files, depth);
            PrintDirectoriesInfo(directories, depth);
        }

        private static DateTime GetOldest(this DirectoryInfo directoryInfo) {
            DateTime oldestDate = DateTime.MaxValue;
            oldestDate = (oldestDate > GetOldestDirectory(directoryInfo)
                ? GetOldestDirectory(directoryInfo)
                : oldestDate);
            oldestDate = (oldestDate > GetOldestFile(directoryInfo)
                ? GetOldestFile(directoryInfo)
                : oldestDate);
            return oldestDate;
        }

        private static SortedDictionary<string, int> GetCollection(string path) {
            SortedDictionary<string, int> dictionary = new SortedDictionary<string, int>(new Comparator());
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (var file in directoryInfo.GetFiles()) {
                dictionary.Add(file.Name, (int)file.Length);
            }
            foreach (var directory in directoryInfo.GetDirectories()) {
                int directorySize = directory.GetFiles().Length + directory.GetDirectories().Length;
                dictionary.Add(directory.Name, directorySize);
            }
            return dictionary;
        }

        private static void Serialize(SortedDictionary<string, int> dictionary) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = new FileStream("data.dat", FileMode.Create);
            binaryFormatter.Serialize(fileStream, dictionary);
            fileStream.Close();
        }

        private static SortedDictionary<string, int> Deserialize() {
            SortedDictionary<string, int> dictionary = new SortedDictionary<string, int>(new Comparator());
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = new FileStream("data.dat", FileMode.Open);
            return (SortedDictionary<string, int>)binaryFormatter.Deserialize(fileStream);
        }

        private static string GetRAHS(this FileSystemInfo fileSystemInfo) {
            string rahs = "";
            FileAttributes fileAttributes = fileSystemInfo.Attributes;
            rahs += ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? 'r' : '-');
            rahs += ((fileAttributes & FileAttributes.Archive) == FileAttributes.Archive ? 'a' : '-');
            rahs += ((fileAttributes & FileAttributes.Hidden) == FileAttributes.Hidden ? 'h' : '-');
            rahs += ((fileAttributes & FileAttributes.System) == FileAttributes.System ? 's' : '-');
            return rahs;
        }

        private static void PrintFilesInfo(List<string> files, int depth) {
            foreach (var file in files) {
                FileInfo fileInfo = new FileInfo(file);
                WriteDepth(depth);
                Console.WriteLine(fileInfo.Name + " " + fileInfo.Length + " " + fileInfo.GetRAHS());
            }
        }

        private static void PrintDirectoriesInfo(List<string> directories, int depth) {
            foreach (var directory in directories) {
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                int elementNumber = directoryInfo.GetDirectories().Length + directoryInfo.GetFiles().Length;
                WriteDepth(depth);
                Console.WriteLine(directoryInfo.Name + " " + elementNumber + " " + directoryInfo.GetRAHS());
                PrintInfo(directory, depth + 1);
            }
        }

        private static DateTime GetOldestDirectory(DirectoryInfo directoryInfo) {
            DateTime oldestDate = DateTime.MaxValue;
            foreach (var directory in directoryInfo.GetDirectories()){
                oldestDate = (oldestDate > directory.GetOldest() ? directory.GetOldest() : oldestDate);
            }
            return oldestDate;
        }
        
        private static DateTime GetOldestFile(DirectoryInfo directoryInfo) {
            DateTime oldestDate = DateTime.MaxValue;
            foreach (var file in directoryInfo.GetFiles()){
                oldestDate = (oldestDate > file.CreationTime ? file.CreationTime : oldestDate);
            }
            return oldestDate;
        }

        private static void WriteDepth(int depth) {
            for (int i = 0; i < depth; i++) {
                Console.Write("    ");
            }
        }
    }
}