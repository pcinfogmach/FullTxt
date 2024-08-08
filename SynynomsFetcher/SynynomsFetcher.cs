using System.Windows;
using System.Collections.Generic;
using WordInterop = Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;
using System;
using Microsoft.Office.Interop.Word;

namespace SynynomsFetcher
{
    internal class SynynomsFetcher
    {
        public List<string> MsWordSynynoms(string word)
        {
            var result = new List<string>();
                WordInterop.Application wordApp = null;
                bool newApp = false;

                try
                {
                    try
                    {
                        wordApp = (WordInterop.Application)Marshal.GetActiveObject("Word.Application");
                    }
                    catch (COMException)
                    {
                        wordApp = new WordInterop.Application();
                        newApp = true;
                }

                var synonymInfo = wordApp.get_SynonymInfo(word, WdLanguageID.wdHebrew);
                if (synonymInfo.Found)
                {
                    foreach (var meaning in synonymInfo.MeaningList as Array)
                    {
                        foreach (var synonym in synonymInfo.SynonymList[meaning] as Array)
                        {
                            result.Add(synonym.ToString());
                        }
                    }                  
                }

            }
                finally
                {
                    if (newApp && wordApp != null)
                    {
                        wordApp.Quit();
                        Marshal.ReleaseComObject(wordApp);
                    }
                }
            return result;
        }
    }
}
