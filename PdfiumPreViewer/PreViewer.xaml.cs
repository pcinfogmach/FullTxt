using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PdfiumPreViewer
{
    /// <summary>
    /// Interaction logic for PreViewer.xaml
    /// </summary>
    public partial class PreViewer : UserControl
    {
        public PreViewer()
        {
            InitializeComponent();
            Viewer.FileName = "C:\\Users\\Admin\\Desktop\\TextSeekersTests\\Pdf\\הגדה של פסח מדרש הגדה להמלבי''ם.pdf";
        }
    }
}
