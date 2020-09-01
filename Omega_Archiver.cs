using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace OmegaArchiver
{
    static class Globals
    {
        public static int movedfilecount = 0;
        public static bool lsmode;
        public static bool lastmod;
        public static List<string> errormessages = new List<string>();
    }
    class Program
    {
        public static string GetTargetDirectory()
        {
            string target;
            do
            {
                Console.Write("Target folder path: ");
                target = Console.ReadLine();
                target = FoldernameFixer(target);
            } while (!Directory.Exists(target));
            return target;
        }

        public static string GetDestinationDirectory()
        {
            string dest;
            do
            {
                Console.Write("Archive folder path: ");
                dest = Console.ReadLine();
                dest = FoldernameFixer(dest);
            } while (!Directory.Exists(dest));
            return dest;
        }

        public static DateTime GetCriticalDate()
        {
            DateTime t;
            do
            {
                Console.Write("Critical time ( YYYY.MM.DD ): ");
            } while (!DateTime.TryParse(Console.ReadLine(), out t));
            return t;
        }

        public static void AskLightSpeedMode()
        {
            string ls;
            do
            {
                Console.Write("Allow LightSpeed mode? ( Y / N ): ");
                ls = Console.ReadLine();
            } while (!(ls == "Y" || ls == "y" || ls == "N" || ls == "n"));
            if (ls == "Y" || ls == "y")
            {
                Globals.lsmode = true;
            }
            else
            {
                Globals.lsmode = false;
            }
        }

        public static void AskArchiveMode()
        {
            string archmode;
            do
            {
                Console.WriteLine("The default setting looks for LastAccessTime.");
                Console.Write("Would you like to change this to LastDateModified? ( Y / N ): ");
                archmode = Console.ReadLine();
            } while (!(archmode == "Y" || archmode == "y" || archmode == "N" || archmode == "n"));
            if (archmode == "Y" || archmode == "y")
            {
                Globals.lastmod = true;
            }
            else
            {
                Globals.lastmod = false;
            }
        }

        public static void SpeedControl()
        {
            if (!Globals.lsmode)
            {
                Thread.Sleep(500);
            }
        }

        public static DateTime CriticalDate(string path)
        {
            DateTime t;
            if (!Globals.lastmod)
            {
                t = File.GetLastAccessTime(path);
            }
            else
            {
                t = File.GetLastWriteTime(path);
            }
            return t;
        }

        public static string FoldernameFixer(string foldername)
        {
            if (foldername[foldername.Length - 1] == '/' || foldername[foldername.Length - 1] == 92) // '\' == 92
            {
                foldername = foldername.Remove(foldername.Length - 1, 1);
            }
            return foldername;
        }

        public static void DivisionLine(int linelength)
        {
            Console.Write("\n");
            for (int i = 0; i < linelength - 1; ++i)
            {
                Console.Write("-");
            }
            Console.Write("\n");
        }

        public static void LastSummarize(string target, string dest, DateTime t)
        {
            Console.Clear();
            Console.WriteLine("DOUBLE CHECK BEFORE MOVING FILES!!!\n");
            Console.WriteLine("Target folder:\t" + target);
            Console.WriteLine("Archive folder:\t" + dest);
            Console.Write("\nArchive mode:\t");
            if (Globals.lastmod)
            {
                Console.Write("LastDateModified\n");
            }
            else
            {
                Console.Write("LastAccessTime\n");
            }
            Console.WriteLine("Critical date:\t" + t);
            Console.Write("Speed mode:\t");
            if (Globals.lsmode)
            {
                Console.Write("LightSpeed");
            }
            else
            {
                Console.Write("Normal");
            }
            Console.WriteLine("\n\nIf everything seems correct, press any button!");
            Console.WriteLine("If something is incorrect press Ctrl+C to exit!\n");
            Console.ReadKey();
            Console.Clear();
        }

        public static void GracefulFolderDelete(string currentfolder)
        {
            SpeedControl();
            if (!Directory.EnumerateFileSystemEntries(currentfolder).Any())
            {
                SpeedControl();
                try
                {
                    Directory.Delete(currentfolder);
                    Console.WriteLine("\n" + currentfolder + " empty folder deleted!");
                }
                catch
                {
                    string errormsg = currentfolder + " folder cannot be deleted for some reason!";
                    Globals.errormessages.Add(errormsg);
                    Console.WriteLine("\n" + errormsg);
                    Console.WriteLine("Continue...");
                }
            }
        }

        public static void FileRecursion(string targetroot, string destroot, DateTime t, string forbiddenroot)
        {
            string[] filelist = Directory.GetFiles(targetroot);
            int trlen = targetroot.Length;
            foreach (string currentfile in filelist)
            {
                if (CriticalDate(currentfile) <= t)
                {
                    if (!Directory.Exists(destroot))
                    {
                        SpeedControl();
                        Directory.CreateDirectory(destroot);
                    }
                    SpeedControl();
                    try
                    {
                        File.Move(currentfile, destroot + currentfile.Remove(0, trlen));
                        Globals.movedfilecount++;
                        Console.WriteLine("\n" + currentfile + "\nmoved to:\n" + destroot + currentfile.Remove(0, trlen));
                    }
                    catch
                    {
                        string errormsg = currentfile + " cannot be moved for some reason!";
                        Globals.errormessages.Add(errormsg);
                        Console.WriteLine("\n" + errormsg);
                        Console.WriteLine("Continue...");
                    }
                }
            }
            string[] folderlist = Directory.GetDirectories(targetroot);
            foreach (string currentfolder in folderlist)
            {
                if (currentfolder != forbiddenroot)
                {
                    FileRecursion(currentfolder, destroot + currentfolder.Remove(0, trlen), t, forbiddenroot);
                }
            }
        }

        public static void FolderRecursion(string targetroot, string forbiddenroot)
        {
            string[] folderlist = Directory.GetDirectories(targetroot);
            foreach (string currentfolder in folderlist)
            {
                SpeedControl();
                if (currentfolder != forbiddenroot)
                {
                    FolderRecursion(currentfolder, forbiddenroot);
                }
            }
            GracefulFolderDelete(targetroot);
        }

        public static void EndResults()
        {
            DivisionLine(60);
            Console.WriteLine("\n\nArchiving completed!\n\n");
            Console.WriteLine("Number of files moved: " + Globals.movedfilecount);
            Console.WriteLine("\nNumber of errors:" + Globals.errormessages.Count());
            foreach (string errormsg in Globals.errormessages)
            {
                Console.WriteLine(errormsg);
            }
            Console.WriteLine("\n\n\nPress any button to continue!");
            Console.ReadKey();
            Console.Clear();
            Globals.movedfilecount = 0;
            Globals.errormessages.Clear();
        }

        static void Main(string[] args)
        {
            while (true)
            {
                string target = GetTargetDirectory();
                string dest = GetDestinationDirectory();
                AskArchiveMode();
                DateTime t = GetCriticalDate();
                AskLightSpeedMode();
                LastSummarize(target, dest, t);
                FileRecursion(target, dest, t, dest);
                DivisionLine(60);
                FolderRecursion(target, dest);
                EndResults();
            }
        }
    }
}