// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0620708cec3537ffb2b2b46f22bed25ffab29f6453962b772ad2bdb771330fbb
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces.Services;
using System;
using System.Diagnostics;
using System.Windows;

namespace LagoVista.PickAndPlace.App.PCB
{
    public class Pcb2GCodeService : IPcb2GCodeService
    {
        static Process _eagleULPProcess;
        static PcbMillingProject _project;

        public void CreateGCode(PcbMillingProject project)
        {
            if (String.IsNullOrEmpty(project.EagleBRDFileLocalPath))
            {
                MessageBox.Show("Please add an Eagle Board File to your Project.");
                return;
            }

            if (!System.IO.File.Exists(project.EagleBRDFileLocalPath))
            {
                MessageBox.Show("Could not find Eagle Board File, please check your settings and try again.");
                return;
            }

            _project = project;
            if (String.IsNullOrEmpty(Properties.Settings.Default.EagleConExecutable) ||
                !System.IO.File.Exists(Properties.Settings.Default.EagleConExecutable))
            {
                MessageBox.Show("Please locate the file EagleCon.exe from your Eagle Directory, this is usually installed in C:\\ ");

                var dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = "eaglecon.exe";
                dlg.Filter = "EXE FIles (*.exe)|*.exe";
                var result = dlg.ShowDialog();
                if (String.IsNullOrEmpty(dlg.FileName))
                {
                    return;
                }

                var info = new System.IO.FileInfo(dlg.FileName);

                if (!System.IO.File.Exists(dlg.FileName) || info.Name.ToLower() != "eaglecon.exe")
                {
                    MessageBox.Show("Sorry, please find the file EagleCon.exe");
                    return;
                }

                Properties.Settings.Default.EagleConExecutable = dlg.FileName;
                Properties.Settings.Default.Save();
            }

            if (String.IsNullOrEmpty(Properties.Settings.Default.PCBGCodeULP) ||
                !System.IO.File.Exists(Properties.Settings.Default.PCBGCodeULP))
            {
                MessageBox.Show("Please locate the file pcb-gcode-setup.ulp ");

                var dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "Eagle ULP (*.ulp)|*.ulp";
                var result = dlg.ShowDialog();
                if (String.IsNullOrEmpty(dlg.FileName))
                {
                    return;
                }

                var info = new System.IO.FileInfo(dlg.FileName);

                if (!System.IO.File.Exists(dlg.FileName) || info.Name.ToLower() != "pcb-gcode-setup.ulp")
                {
                    MessageBox.Show("Sorry, please find the file pcb-gcode-setup.ulp");
                    return;
                }

                Properties.Settings.Default.PCBGCodeULP = dlg.FileName.ToLower();
                Properties.Settings.Default.Save();
            }

            var eagleConFileInfo = new System.IO.FileInfo(Properties.Settings.Default.EagleConExecutable);


            var ulpParams = $@"-C ""RUN '{Properties.Settings.Default.PCBGCodeULP}'; set confirm no; quit"" ";
            var fullArgs = $@"{ulpParams} ""{project.EagleBRDFileLocalPath}""";

            _eagleULPProcess = new Process();
            _eagleULPProcess.EnableRaisingEvents = true;
            _eagleULPProcess.Exited += EagleULP_Exited;
            _eagleULPProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = $"{eagleConFileInfo.DirectoryName}\\{eagleConFileInfo.Name}",
                WorkingDirectory = eagleConFileInfo.DirectoryName,
                Arguments = fullArgs,
            };

            _eagleULPProcess.Start();
        }

        private static void EagleULP_Exited(object sender, EventArgs e)
        {
            if (_project == null)
            {
                MessageBox.Show("Sorry, there was an error associating the generated files to your project.  Please manually attach them under project settings.");
                return;
            }

            var boardFileInfo = new System.IO.FileInfo(_project.EagleBRDFileLocalPath);
            var baseBoardName = boardFileInfo.Name.Replace(".brd", "");

            var topEtchFilePath = System.IO.Path.Combine(boardFileInfo.DirectoryName, $"{baseBoardName}.top.etch.tap");
            var bottomEtchFilePath = System.IO.Path.Combine(boardFileInfo.DirectoryName, $"{baseBoardName}.bot.etch.tap");
            var drillFilePath = System.IO.Path.Combine(boardFileInfo.DirectoryName, $"{baseBoardName}.top.drill.tap");
            var millingFilePath = System.IO.Path.Combine(boardFileInfo.DirectoryName, $"{baseBoardName}.top.mill.tap");
            var drillFileBottomPath = System.IO.Path.Combine(boardFileInfo.DirectoryName, $"{baseBoardName}.bot.drill.tap");
            var millingFileBottomPath = System.IO.Path.Combine(boardFileInfo.DirectoryName, $"{baseBoardName}.bot.mill.tap");

            if (!System.IO.File.Exists(topEtchFilePath) ||
                !System.IO.File.Exists(bottomEtchFilePath))
            {
                MessageBox.Show("Sorry, we could not locate the newly created etching files if you save them to a different location or with a different file name, you can manually assign them with the project settings.");
                _project = null;
                return;
            }

            var topEtchFileInfo = new System.IO.FileInfo(topEtchFilePath);
            if ((DateTime.Now - topEtchFileInfo.LastWriteTime) > TimeSpan.FromMinutes(5))
            {
                if (MessageBox.Show("The generated etching files we found are older than 5 minutes, this could be because the process failed or the application sat idle.  Would you like to use these files?", "Use Old Files?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    _project = null;
                    return;
                }
            }

            _project.TopEtchingFile = null;
            _project.TopEtchingFileLocalPath = topEtchFilePath;
            _project.BottomEtchingFile = null;
            _project.BottomEtchingFileLocalPath = bottomEtchFilePath;
            _project.MillingFile = null;
            _project.MillingFileLocalPath = millingFilePath;
            _project.MillingBottomFile = null;
            _project.MillingBottomFileLocalPath = millingFileBottomPath;
            _project.DrillFile = null;
            _project.DrillFileLocalPath = drillFilePath;
            _project.DrillBottomlFile = null;
            _project.DrillBottomFileLocalPath = drillFileBottomPath;

            _project = null;
        }
    }
}
