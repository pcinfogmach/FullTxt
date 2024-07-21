using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace PdfiumPreViewer.Controls
{
    public class CostumePopup : Popup
    {
        public static readonly DependencyProperty FocusTargetProperty =
            DependencyProperty.Register(
                nameof(FocusTarget),
                typeof(TextBox),
                typeof(CostumePopup),
                new PropertyMetadata(null));

        public TextBox FocusTarget
        {
            get { return (TextBox)GetValue(FocusTargetProperty); }
            set { SetValue(FocusTargetProperty, value); }
        }

        public CostumePopup()
        {
            this.Opened += OnOpenedHandler;
        }

        private void OnOpenedHandler(object sender, EventArgs e)
        {
            FocusTarget?.Focus();
        }
    }
}
