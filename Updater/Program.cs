using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Updater;

internal static class Program {
    public static int Main(string[] args) {
        if (args.Length < 4) {
            Console.WriteLine("first argument is PID of the called. Used to wait for it to exit.\n" +
                              "second argument must be old package folder and third argument the location of the new files.\n" +
                              "fourth argument is path to file that will be executed after install complete.\n" +
                              "Subsequent arguments are args to pass to the final executed program to ensure seamless" +
                              "trasition between versions\n. " +
                              "Press enter to exit");
            Console.ReadLine();
            return -1;
        }

        string currentPackage = args[1];
        if (!currentPackage.EndsWith('/') && !currentPackage.EndsWith('\\')) {
            currentPackage += '/';
        }
        currentPackage = currentPackage.Replace("\\", "/");
        
        string newPackage = args[2];
        if (!newPackage.EndsWith('/') && !newPackage.EndsWith('\\')) {
            newPackage += '/';
        }
        newPackage = newPackage.Replace("\\", "/");

        string? executable = args.Length > 3 ? args[3] : null;
        string[] passingArgs = args.Length > 4 ? args[4..] : [];
        int pid = int.Parse(args[0]);

        // Console.WriteLine($"Starting with:\n" +
        //                   $" - PID: {pid}\n" +
        //                   $" - Current Folder: \"{currentPackage}\" (root={Path.IsPathRooted(currentPackage)}/qualified={Path.IsPathFullyQualified(currentPackage)})\n" +
        //                   $" - New Folder: \"{newPackage}\" (root={Path.IsPathRooted(newPackage)}/qualified={Path.IsPathFullyQualified(newPackage)})\n" +
        //                   $" - Executable: \"{executable}\"\n" +
        //                   $" - Args: \"{passingArgs}\"");

        if (!Path.IsPathRooted(currentPackage) || !Path.IsPathRooted(newPackage)) {
            Console.WriteLine("Both paths must be absolute. Press enter to exit");
            Console.ReadLine();
            return -1;
        }

        if (!Directory.Exists(newPackage)) {
            Console.WriteLine("The specified new package directory does not exists. Press enter to exit");
            Console.ReadLine();
            return -1;
        }

        if (!Directory.Exists(currentPackage)) {
            Console.WriteLine("The specified current directory does not exist. Press enter to exit");
            Console.ReadLine();
            return -1;
        }

        // before update, wait for executable to exit
        try {
            Process process = Process.GetProcessById(pid);
            process.WaitForExit();
        }
        catch (ArgumentException) {
            // already exited
        }
        
        try {
            if (!Update(currentPackage, newPackage)) {
                Console.WriteLine("Error updating :( Press enter to exit");
                Console.ReadLine();
                return -1;
            }
        }
        catch (Exception ex) {
            Console.WriteLine("Fatal error updating! " + ex.Message + "\n"+
                              "Press enter to exit");
            Console.ReadLine();
            return -1;
        }

        Console.WriteLine("Update successfull!");
        if (executable is null) return 0;
        Console.WriteLine("Launching application");
        try {
            Launch(executable, currentPackage, passingArgs); // currentPackage here has the newPackage files
        }
        catch (Exception ex) {
            Console.WriteLine("Fatal error launching! " + ex.Message + "\n"+
                              "Press enter to exit");
            Console.ReadLine();
            return -1;
        }

        return 0;
    }

    private static bool Update(string oldPath, string newPath) {
        
        // delete old files but not updater.exe
        foreach (string file in Directory.EnumerateFiles(oldPath)) {
            if (!file.ToLower().EndsWith("updater.exe")) {
                File.Delete(file);
            }
        }
        foreach (string folder in Directory.EnumerateDirectories(oldPath)) {
            Delete(folder);
        }
        
        // move in new files
        Move(oldPath, newPath);
        return true;

        void Move(string old, string @new) {
            foreach (string entry in Directory.EnumerateFiles(@new)) {
                string sourceFilename = Path.GetFileName(entry);
                string destFilename = sourceFilename;
                if (sourceFilename.Contains("Updater.exe")) {
                    destFilename = "Updater2.exe";
                }

                string sourcePath = Path.Combine(@new, sourceFilename);
                string destinationPath = Path.Combine(old, destFilename);

                try {
                    File.Move(sourcePath, destinationPath);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Couldnt move file: {sourcePath}->{destinationPath}. Error: \"{ex.Message}\"");
                }
            }

            foreach (string entry in Directory.EnumerateDirectories(@new)) {
                string? directoryName = Path.GetDirectoryName(entry);
                if (directoryName is null) {
                    Console.WriteLine($"Error. Skipping directory {entry}");
                    continue;
                }
                Move(Path.Combine(old, directoryName), Path.Combine(@new, directoryName));
            }
        }

        void Delete(string folder) {
            foreach (string file in Directory.EnumerateFiles(folder)) {
                File.Delete(file);
            }
            foreach (string dir in Directory.EnumerateDirectories(folder)) {
                Delete(dir);
            }
        }

        // remove old artifacts
        void SetNormalAttributes(DirectoryInfo info) {
            foreach (DirectoryInfo subDir in info.GetDirectories()) {
                subDir.Attributes = FileAttributes.Normal;
                SetNormalAttributes(subDir);
            }
            foreach (FileInfo file in info.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }
    }

    [DoesNotReturn]
    private static void Launch(string executable, string appFolder, string[] args) {
        ProcessStartInfo startInfo = new() {
            Arguments = string.Join(' ', args),
            FileName = executable,
            WorkingDirectory = appFolder
        };
        Process.Start(startInfo);
        Environment.Exit(0);
    }
    
}