using System.Collections.Generic;
using WordInterop = Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Word;
using System;

namespace FullText.Search
{
    internal class SynynomsFetcher
    {
        List<string> MsWordSynynoms(string word)
        {
            var result = new List<string>();
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
                WordInterop.Application wordApp = null;
                bool newApp = false;

                try
                {
                    try
                    {
                        wordApp = (WordInterop.Application)Marshal.GetActiveObject("Word.Application");
                        System.Windows.Application.Current.Exit += (s, e) => { try { Marshal.ReleaseComObject(wordApp); } catch { } };
                    }
                    catch (COMException)
                    {
                        wordApp = new WordInterop.Application();
                        newApp = true;
                        System.Windows.Application.Current.Exit += (s, e) => { try { wordApp.Quit(); Marshal.ReleaseComObject(wordApp); } catch { } };
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
            });
            return result;
        }
    }
}
