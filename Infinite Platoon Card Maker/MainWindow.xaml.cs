using System;
using System.Runtime.InteropServices;
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
using Microsoft.Win32;
using System.Text.RegularExpressions;
using Excel = Microsoft.Office.Interop.Excel;

namespace Infinite_Platoon_Card_Maker
{
    public partial class MainWindow : Window
    {

        Paragraph effectText = new Paragraph();
        Paragraph flavorText = new Paragraph();
        int currentCard = 0;


        class heroCard
        {
            string name;
            int soul;
            int atk;
            int def;
            string type;
            string effect;
            string flavor;
            string boost1;
            string boost2;
            string boost3;

            heroCard(string n, int s, int a, int d, string t, string e, string f, string b1, string b2, string b3)
            {
                name = n;
                soul = s;
                atk = a;
                def = d;
                type = t;
                effect = e;
                flavor = f;
                boost1 = b1;
                boost2 = b2;
                boost3 = b3;
            }
        };


        public MainWindow()
        {
            InitializeComponent();
            FlavorInput.Text = "AAAAA";
        }

        private void frameLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.InitialDirectory = Environment.CurrentDirectory.ToString() + "\\Components\\Frames";
            Console.WriteLine(Environment.CurrentDirectory.ToString());
            op.Title = "Select a frame: ";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
           
            if (op.ShowDialog() == true)
            {
                cardframe.Source = new BitmapImage(new Uri(op.FileName));
            }
        }

        private void illustrationLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.InitialDirectory = Environment.CurrentDirectory.ToString() + "\\Components\\Illustrations";
            op.Title = "Select an illustration: ";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            
            if (op.ShowDialog() == true)
            {
                illustration.Source = new BitmapImage(new Uri(op.FileName));
            }
        }

        private void NameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HeroName.Content = NameInput.Text;

            if (NameInput.Text.Length < 25)
            {
                HeroName.FontSize = 24;
            }
            else
            {
                HeroName.FontSize = 24 - Math.Floor((double)((NameInput.Text.Length - 10) / 4));
            }
        }

        private void TypeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HeroType.Content = TypeInput.Text;
        }

        private void EffectInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange temp = new TextRange(EffectInput.Document.ContentStart, EffectInput.Document.ContentEnd);
            Console.WriteLine(temp.Text);

            effectText.Inlines.Clear();

            effectText.Inlines.Add(new Run(temp.Text));

            updateDescription();
        }

        private void FlavorInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            flavorText.Inlines.Clear();
            flavorText.Inlines.Add(new Italic(new Run(FlavorInput.Text)));

            updateDescription();
        }

        private void updateDescription()
        {
            Description.Document.Blocks.Clear();

            Description.Document.Blocks.Add(effectText);
            Description.Document.Blocks.Add(flavorText);
        }

        private void readExcel(string fileDirectory)
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(fileDirectory);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            for (int i = 1; i <= xlRange.Rows.Count; i++)
            {
                for (int j = i; j <= xlRange.Columns.Count; j++ )
                {
                    if (xlRange[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                        Console.Write(xlRange.Cells[i, j].Value2.ToString() + "\t");
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
        }

        private void AttackInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            onlyNumbers(e);
        }

        private void DefenseInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            onlyNumbers(e);
        }

        private void SoulInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            onlyNumbers(e);
        }

        private void AttackInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Attack.Content = AttackInput.Text;
        }

        private void DefenseInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Defense.Content = DefenseInput.Text;
        }

        private void SoulInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Soul.Content = SoulInput.Text;
        }

        private void AttackBoostInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            AttackBoost.Text = AttackBoostInput.Text;
        }

        private void DefenseBoostInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            DefenseBoost.Text = DefenseBoostInput.Text;
        }

        private void onlyNumbers(TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void EffectBoostInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            AbilityBoost.Text = EffectBoostInput.Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Rect rect = new Rect(new Point(0, 0), new Point(cardArea.Margin.Left + cardArea.ActualWidth + 20, cardArea.Margin.Top + cardArea.ActualHeight));
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Width,
                (int)rect.Height, 100d, 100d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(cardArea);

            var crop = new CroppedBitmap(rtb, new Int32Rect((int)Math.Floor(cardArea.Margin.Left + 20), (int)Math.Floor(cardArea.Margin.Top), (int)Math.Floor(cardArea.ActualWidth), (int)Math.Floor(cardArea.ActualHeight)));

            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(crop));

            //save to memory stream
            using (var fs = System.IO.File.OpenWrite( System.AppDomain.CurrentDomain.BaseDirectory + "/Heroes/" + HeroName.Content + ".png"))
            {
                pngEncoder.Save(fs);
            }
        }

        private void PublicationInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            PublicationInfo.Text = PublicationInput.Text;
        }

        private void clearName_Click(object sender, RoutedEventArgs e)
        {
            NameInput.Text = "";
            SoulInput.Text = "";   
            illustration.Source = null;
            TypeInput.Text = "";
            EffectInput.Document.Blocks.Clear();
            FlavorInput.Text = "";
            AttackInput.Text = "";
            DefenseInput.Text = "";
            PublicationInfo.Text = "";


        }

        private void nextCardButton_Click(object sender, RoutedEventArgs e)
        {
            currentCard++;

            readExcel(Environment.CurrentDirectory.ToString() + "\\HeroCards.xlsx");
        }

        private void lastCardButton_Click(object sender, RoutedEventArgs e)
        {
            currentCard--;

        }

    }
}
