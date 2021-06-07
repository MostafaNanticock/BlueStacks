using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using CodeTitans.JSon;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
    public class FileImporter
    {
        public static DragEventHandler MakeDragDropHandler(Form form)
        {
            return delegate(object obj, DragEventArgs evt)
            {
                Thread thread = new Thread((ThreadStart)delegate
                {
                    FileImporter.HandleDragDropAsync(evt, form);
                });
                thread.IsBackground = true;
                thread.Start();
            };
        }

        private static void HandleDragDropAsync(DragEventArgs evt, Form form)
        {
            if (BlueStacks.hyperDroid.Common.Utils.IsSharedFolderEnabled())
            {
                try
                {
                    Array array = (Array)evt.Data.GetData(DataFormats.FileDrop);
                    if (array != null)
                    {
                        string text = array.GetValue(0).ToString();
                        string fileName = Path.GetFileName(text);
                        string sharedFolderDir = Strings.SharedFolderDir;
                        string sharedFolderName = Strings.SharedFolderName;
                        string destinationFileName = Path.Combine(sharedFolderDir, fileName);
                        string mimeFromFile = BlueStacks.hyperDroid.Common.Utils.GetMimeFromFile(text);
                        Logger.Info("DragDrop File: {0}, mime: {1}", text, mimeFromFile);
                        FileSystem.CopyFile(text, destinationFileName, UIOption.AllDialogs);
                        string text2 = "/mnt/sdcard/windows/" + sharedFolderName + "/" + fileName;
                        Logger.Info("dragDrop androidPath: " + text2);
                        string url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + Strings.FileDropUrl;
                        JSonWriter jSonWriter = new JSonWriter();
                        jSonWriter.WriteArrayBegin();
                        jSonWriter.WriteObjectBegin();
                        jSonWriter.WriteMember("filepath", text2);
                        jSonWriter.WriteMember("mime", mimeFromFile);
                        jSonWriter.WriteObjectEnd();
                        jSonWriter.WriteArrayEnd();
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        dictionary.Add("data", jSonWriter.ToString());
                        Logger.Info("Sending drag drop request: " + jSonWriter.ToString());
                        try
                        {
                            Client.Post(url, dictionary, null, false);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Failed to send FileDrop request. err: " + ex.ToString());
                        }
                        UIHelper.RunOnUIThread(form, delegate
                        {
                            form.Activate();
                        });
                    }
                }
                catch (Exception ex2)
                {
                    Logger.Error("Error in DragDrop function: " + ex2.Message);
                }
            }
        }

        public static void HandleDragEnter(object obj, DragEventArgs evt)
        {
            if (evt.Data.GetDataPresent(DataFormats.FileDrop))
            {
                evt.Effect = DragDropEffects.Copy;
            }
            else
            {
                Logger.Debug("FileDrop DataFormat not supported");
                string[] formats = evt.Data.GetFormats();
                Logger.Debug("Supported formats:");
                string[] array = formats;
                foreach (string msg in array)
                {
                    Logger.Debug(msg);
                }
                evt.Effect = DragDropEffects.None;
            }
        }
    }
}
