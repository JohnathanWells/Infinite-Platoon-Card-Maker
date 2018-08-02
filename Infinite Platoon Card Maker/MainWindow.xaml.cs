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
using HtmlAgilityPack;
using HTMLConverter;


namespace Infinite_Platoon_Card_Maker
{
    public partial class MainWindow : Window
    {
        enum cardtypes { Hero, Spell, Trap, Item };
        readonly int[] ATT_NUM_BY_TYPE = { 10, 10, 10, 10 };

        string effectText;
        string flavorText;
        BlockCollection effectBlock;
        BlockCollection flavorBlock;
        int currentCard = 0;
        cardtypes typeSelected;
        List<heroCard>[] CardLists = new List<heroCard>[4];

        class heroCard
        {
            public string name;
            public int soul;
            public int atk;
            public int def;
            public string type;
            public string effect;
            //public FlowDocument effect;
            public string flavor;
            public string boost1;
            public string boost2;
            public string boost3;

            public heroCard(string n, int s, int a, int d, string t, string e, string f, string b1, string b2, string b3)
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
            typeSelected = cardtypes.Hero;
            loadExcelIntoList(typeSelected);
            fillFieldsWithData(typeSelected); 
        }

        #region excelFunctions

        private List<string> readExcel(string fileDirectory)
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(fileDirectory);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            List<string> lines = new List<string>();

            for (int i = 1; i <= xlRange.Rows.Count; i++)
            {
                for (int j = 1; j <= xlRange.Columns.Count; j++)
                {
                    if (xlRange[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                    {
                        //Object x = xlRange.Cells[i, j].Value2;
                        //xlRange.Cells[i, j].Copy();
                        //Console.WriteLine(Clipboard.GetText(TextDataFormat.Rtf).ToString());
                        //lines.Add(Clipboard.GetText(TextDataFormat.Rtf));
                        lines.Add(xlRange.Cells[i, j].Value2.ToString());
                    }
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

            return lines;
        }

        private void loadExcelIntoList(cardtypes type)
        {
            switch (type)
            {
                case cardtypes.Hero:
                    {
                        List<heroCard> heroes = new List<heroCard>();
                        List<string> List = readExcel(Environment.CurrentDirectory.ToString() + "\\HeroCards.xlsx");
                        int offset = 0;
                        int rows = List.Count / ATT_NUM_BY_TYPE[(int)type];

                        for (int n = 1; n < rows; n++)
                        {
                            offset = 10 * n;

                            heroes.Add(new heroCard((List[offset + 0]), int.Parse((List[offset + 1])), int.Parse((List[2 + offset])), int.Parse((List[3 + offset])), (List[4 + offset]), (List[5 + offset]), (List[6 + offset]), ((List[7 + offset]) == "" || (List[7 + offset]) == " ") ? " " : string.Concat("[", List[7 + offset], "]"), string.Concat("[", returnWithSign((List[8 + offset])) + " ATK]"), string.Concat("[", returnWithSign((List[9 + offset])), " DEF]")));
                        }

                        CardLists[(int)cardtypes.Hero] = heroes;
                        return;
                    }
            };
            return;
        }

        public string turnFlowDocumentToString(FlowDocument from)
        {
            return new TextRange(from.ContentStart, from.ContentEnd).Text;
        }

        private void clearName_Click(object sender, RoutedEventArgs e)
        {
            NameInput.Text = "";
            SoulInput.Text = "";
            illustration.Source = null;
            TypeInput.Text = "";
            EffectInput.Text = "";
            //EffectInput.Document.Blocks.Clear();
            FlavorInput.Text = "";
            AttackInput.Text = "";
            DefenseInput.Text = "";
            PublicationInfo.Text = "";


        }

        private void nextCardButton_Click(object sender, RoutedEventArgs e)
        {
            currentCard = Clamp(currentCard + 1, 0, CardLists[(int)typeSelected].Count - 1);

            fillFieldsWithData(typeSelected);
        }

        private void lastCardButton_Click(object sender, RoutedEventArgs e)
        {
            currentCard = Clamp(currentCard - 1, 0, CardLists[(int)typeSelected].Count - 1);
            //polyline.Points.Add(new Point(cardArea.Margin.Left + 18, cardArea.Margin.Top));
            //polyline.Points.Add(new Point((this.Content as FrameworkElement).ActualWidth - cardArea.Margin.Right - 11, (this.Content as FrameworkElement).ActualHeight - cardArea.Margin.Bottom + 9));
            fillFieldsWithData(typeSelected);
        }

        #endregion

        #region OnChangedFunctions

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

            //if (NameInput.Text.Length < 25)
            //{
            //    HeroName.FontSize = 24;
            //}
            //else
            //{
            //    HeroName.FontSize = 24 - Math.Floor((double)((NameInput.Text.Length - 10) / 4));
            //}
        }

        private void TypeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HeroType.Content = TypeInput.Text;
        }

        private void EffectInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            effectText = EffectInput.Text;
            //effectBlock = EffectInput.Document.Blocks;
            //TextRange temp = new TextRange(EffectInput.Document.ContentStart, EffectInput.Document.ContentEnd);

            //effectText.Inlines.Clear();

            //effectText.Inlines.Add(new Run(temp.Text));

            updateDescription();
        }

        private void FlavorInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            //flavorText = FlavorInput.Text;

            //flavorText.Inlines.Clear();
            //flavorText.Inlines.Add(new Italic(new Run(FlavorInput.Text)));

            updateDescription();
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

        private void EffectBoostInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            AbilityBoost.Text = EffectBoostInput.Text;
        }

        #endregion

        #region PreviewTextInput

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

        private void PublicationInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            PublicationInfo.Text = PublicationInput.Text;
        }

        #endregion

        #region savingFunctions
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            saveCard();
        }

        private void saveCard()
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(cardArea);
            double dpi = 96d;
            int odp = 0;
            int boundWidth = (int)bounds.Width;
            int boundHeight = (int)bounds.Height;
            //Console.Write(( (float)boundWidth / (float)boundHeight) + "\t" + ((float)boundHeight / (float)boundWidth) + "\n");


            RenderTargetBitmap rtb = new RenderTargetBitmap(boundWidth + (int)(((float)boundHeight / (float)boundHeight) * odp), boundHeight + (int)(((float)boundHeight / (float)boundWidth) * odp), dpi, dpi, System.Windows.Media.PixelFormats.Default);


            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(cardArea);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            rtb.Render(dv);

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();

                pngEncoder.Save(ms);
                ms.Close();

                System.IO.File.WriteAllBytes(System.AppDomain.CurrentDomain.BaseDirectory + "/Heroes/" + HeroName.Content + ".png", ms.ToArray());
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region helperFunctions
        private void onlyNumbers(TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void fillFieldsWithData(cardtypes type)
        {
            switch (type)
            {
                case cardtypes.Hero:
                {
                    NameInput.Text = CardLists[(int)type][currentCard].name;
                    SoulInput.Text = CardLists[(int)type][currentCard].soul.ToString();
                    illustration.Source = null;
                    TypeInput.Text = CardLists[(int)type][currentCard].type;
                    //EffectInput.Text = CardLists[(int)type][currentCard].effect;
                    EffectInput.AppendText(CardLists[(int)type][currentCard].effect);
                    //EffectInput.Document = CardLists[(int)type][currentCard].effect;
                    FlavorInput.Text = CardLists[(int)type][currentCard].flavor;
                    AttackInput.Text = CardLists[(int)type][currentCard].atk.ToString();
                    DefenseInput.Text = CardLists[(int)type][currentCard].def.ToString();
                    AttackBoostInput.Text = CardLists[(int)type][currentCard].boost2;
                    DefenseBoostInput.Text = CardLists[(int)type][currentCard].boost3;
                    EffectBoostInput.Text = CardLists[(int)type][currentCard].boost1;
                    //PublicationInfo.Text = listOfHeroes[currentCard].;
                    return;
                }
            }
        }

        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public string returnWithSign(string str)
        {
            if (float.Parse(str) > 0)
                return "+" + str;
            else
                return str;
        }

        private void updateDescription()
        {
            Description.Document.Blocks.Clear();

            Description.Document = createClockFromString(effectText);
            //Description.Document.Blocks.Add(effectText);
            Description.AppendText(flavorText);
            //Description.Document.Blocks.Add(new Paragraph(new Run(effectText + "\n\n" + flavorText)));

            //Console.Write("DESCRIPTION: " + Description.Document.Blocks.);
        }

        private FlowDocument createClockFromString(string str)
        {
            if (str != null)
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(str);

                var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body");

                FlowDocument temp = new FlowDocument();

                if (htmlBody != null)
                {
                    temp.DataContext = HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml(htmlBody.ToString(), false);

                }

                return temp;

            }
            else
                return new FlowDocument();
        }


        #endregion
    }
}
