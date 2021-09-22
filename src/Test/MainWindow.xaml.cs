using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<FCS.FCS> list = new List<FCS.FCS>();
        FCS.FCS TestData;
        private void open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "FCS文件|*.fcs",
                Multiselect = false
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var items = FCS.Factory.ReadFile(openFileDialog.FileName);
                if (items.Any())
                {
                    list.AddRange(items);
                    TestData = items[0];
                    c1.XSource = TestData.Measurements[1].Values;
                    c1.YSource = TestData.Measurements[4].Values;
                    
                }
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (TestData == null) return;
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "FCS文件|*.fcs"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                FCS.Factory.SaveToFCS31(saveFileDialog.FileName, list.ToArray());
            }

        }
    }
}
