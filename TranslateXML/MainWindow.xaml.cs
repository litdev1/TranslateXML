using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using LibreTranslate.Net;

namespace TranslateXML
{
    enum eEngine { AZURE, LIBRE }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private eEngine engine = eEngine.LIBRE;

        public MainWindow()
        {
            InitializeComponent();

            engine = File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/LIBRE.txt") ? eEngine.LIBRE : eEngine.AZURE;

            languages = translator.GetLanguagesForTranslate();
            languageNames = translator.GetLanguageNames();
            if (null == languages || null == languageNames)
            {
                if (MessageBox.Show("Translator failed to initialise\n\nDo you want to see help information before exiting?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    ShowHelp();
                }
                this.Close();
            }

            int i;
            if (engine == eEngine.LIBRE)
            {
                i = 0;
                foreach (string language in languages)
                {
                    try
                    {
                        LanguageCode.FromString(language);
                    }
                    catch (Exception ex)
                    {
                        languageNames[i] = "*" + languageNames[i];
                    }
                    i++;
                }
            }

            i = 0;
            ListBoxItem item;
            foreach (string languageName in languageNames)
            {
                string itemText = languageName + " (" + languages[i++] + ")";
                item = new ListBoxItem();
                item.Content = itemText;
                listBoxFrom.Items.Add(item);
                if (languageName.StartsWith("English", StringComparison.InvariantCultureIgnoreCase)) en = item;
                if (languageName.StartsWith("Spanish", StringComparison.InvariantCultureIgnoreCase)) es = item;

                item = new ListBoxItem();
                item.Content = itemText;
                listBoxTo.Items.Add(item);
                if (languageName.StartsWith("Norwegian", StringComparison.InvariantCultureIgnoreCase)) no = item;
            }

            comboBox.SelectedIndex = 0;

            textBoxOutput.Text = Settings.GetValue("OUTPUTPATH");
            if (null == textBoxOutput.Text || !Directory.Exists(textBoxOutput.Text))
            {
                textBoxOutput.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            textBoxOutput.Text.Trim(new char[] { '\\' });

            listBoxTo.SelectedItem = no;
            listBoxTo.ScrollIntoView(listBoxTo.SelectedItem);

            SBpath = Settings.GetValue("SBPATH");
            if (null == SBpath || !Directory.Exists(SBpath))
            {
                SBpath = (Environment.Is64BitOperatingSystem ? "C:\\Program Files (x86)" : "C:\\Program Files") + "\\Microsoft\\Small Basic\\";
            }
            if (!SBpath.EndsWith("\\")) SBpath += "\\";
            item = new ListBoxItem();
            item.Content = SBpath + "SmallBasicLibrary.xml";
            comboBoxInput.Items.Add(item);
            item = new ListBoxItem();
            item.Content = SBpath + "Strings.es.resx";
            comboBoxInput.Items.Add(item);
            item = new ListBoxItem();
            item.Content = "C:\\Users\\steve\\source\\repos\\litdev1\\SB-IDE\\SB-Prime\\Properties\\Strings.resx";
            comboBoxInput.Items.Add(item);

            string[] files = Directory.GetFiles(SBpath + "\\lib");
            foreach (string file in files)
            {
                if (file.EndsWith(".xml"))
                {
                    item = new ListBoxItem();
                    item.Content = file;
                    comboBoxInput.Items.Add(item);
                }
            }

            List<string> xmlPaths = Settings.GetValues("XMLPATH");
            foreach (string path in xmlPaths)
            {
                if (Directory.Exists(path))
                {
                    files = Directory.GetFiles(path);
                    foreach (string file in files)
                    {
                        if (file.EndsWith(".xml"))
                        {
                            item = new ListBoxItem();
                            item.Content = file;
                            comboBoxInput.Items.Add(item);
                        }
                    }
                }
            }

            List<string> xmlFiles = Settings.GetValues("XMLFILE");
            foreach (string file in xmlFiles)
            {
                if (File.Exists(file))
                {
                    item = new ListBoxItem();
                    item.Content = file;
                    comboBoxInput.Items.Add(item);
                }
            }

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private Translator2 translator = new Translator2();
        private List<string> languages;
        private List<string> languageNames;
        private int iFrom;
        private List<int> iTo = new List<int>();
        private int numRemaining;
        private string xmlFrom;
        private string xmlTo;
        private int nodeCount;
        private int totalCount;
        private ListBoxItem en;
        private ListBoxItem es;
        private ListBoxItem no;
        private string SBpath = null;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private bool bTranslating = false;

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!bTranslating) progress.Value = 0;
            if (!bTranslating) textBoxProgress.Text = "";
            buttonTranslate.Content = bTranslating ? "Cancel" : "Translate";
            Cursor = bTranslating ? Cursors.Wait : Cursors.Arrow;
            buttonAddXML.IsEnabled = !bTranslating;
            comboBoxInput.IsEnabled = !bTranslating;
            textBoxOutput.IsEnabled = !bTranslating;
            comboBox.IsEnabled = !bTranslating;
            listBoxFrom.IsEnabled = !bTranslating;
            listBoxTo.IsEnabled = !bTranslating;
        }

        private void Worker(Object obj)
        {
            bTranslating = true;
            XmlDocument doc = (XmlDocument)((Object[])obj)[0];
            string xmlTo = (string)((Object[])obj)[1];
            int _iTo = (int)((Object[])obj)[2];
            Parse(doc.DocumentElement, _iTo);
            if (bTranslating) doc.Save(xmlTo);
            numRemaining--;
            if (numRemaining == 0) bTranslating = false; // All complete?
        }

        private void Parse(XmlNode node, int _iTo)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text)
                {
                    nodeCount++;
                    if (bTranslating)
                    {
                        switch (engine)
                        {
                            case eEngine.AZURE:
                                child.Value = translator.Translate(child.Value, languages[iFrom], languages[_iTo]);
                                break;
                            case eEngine.LIBRE:
                                child.Value = translator.Translate2(child.Value, languages[iFrom], languages[_iTo]);
                                break;
                        }

                        Dispatcher.Invoke(() => {
                            progress.Value = nodeCount;
                            textBoxProgress.Text = string.Format("{0} % Complete", 100 * nodeCount / totalCount);
                        });
                    }
                }
                if (child.Name != "resheader") Parse(child, _iTo);
            }
        }

        private void buttonTranslate_Click(object sender, RoutedEventArgs e)
        {
            if (bTranslating)
            {
                bTranslating = false;
                buttonTranslate.Content = "Translate";
            }
            else
            {
                progress.Value = 0;
                xmlFrom = comboBoxInput.Text;
                if (!File.Exists(xmlFrom)) return;

                numRemaining = iTo.Count;
                foreach (int _iTo in iTo)
                {
                    xmlTo = textBoxOutput.Text + "\\" + Path.GetFileNameWithoutExtension(xmlFrom) + "." + languages[_iTo].Substring(0, 2) + Path.GetExtension(xmlFrom);
                    if (comboBox.SelectedIndex == 1) xmlTo = xmlTo.Replace(".es.", ".");

                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlFrom);
                    if (_iTo == iTo[0])
                    {
                        nodeCount = 0;
                        Parse(doc.DocumentElement, _iTo);
                        totalCount = nodeCount * iTo.Count;
                        progress.Maximum = totalCount;
                    }

                    nodeCount = 0;
                    Thread thread = new Thread(new ParameterizedThreadStart(Worker));
                    thread.Start(new Object[] { doc, xmlTo, _iTo });
                    buttonTranslate.Content = "Cancel";
                }
            }
        }

        private void listBoxFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listbox = (ListBox)sender;
            iFrom = listbox.SelectedIndex;
            labelFrom.Content = "Translate from " + languageNames[iFrom];
        }

        private void listBoxTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listbox = (ListBox)sender;
            iTo.Clear();
            if (listbox.SelectedItems.Count == 1)
            {
                iTo.Add(listbox.SelectedIndex);
                labelTo.Content = "Translate to " + languageNames[listbox.SelectedIndex];
            }
            else if (listbox.SelectedItems.Count > 1)
            {
                foreach (ListBoxItem item in listbox.SelectedItems)
                {
                    for (int i = 0; i <  languageNames.Count; i++)
                    {
                        string itemLanguage = item.Content.ToString();
                        if (itemLanguage.StartsWith(languageNames[i]))
                        {
                            iTo.Add(i);
                            break;
                        }
                    }
                }
                labelTo.Content = "Translate to multiple (" + iTo.Count + ")";
            }
            else
            {
                labelTo.Content = "Translate to no selection";
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    comboBoxInput.SelectedIndex = 0;
                    listBoxFrom.SelectedItem = en;
                    break;
                case 1:
                    comboBoxInput.SelectedIndex = 1;
                    listBoxFrom.SelectedItem = es;
                    break;
                case 2:
                    comboBoxInput.SelectedIndex = 2;
                    listBoxFrom.SelectedItem = en;
                    break;
            }
            listBoxFrom.ScrollIntoView(listBoxFrom.SelectedItem);
        }

        private void ShowHelp()
        {
            //string text = "This application uses Microsoft Translator.";
            //text += "\n\nA free (2000000 characters a month) option is avalable,";
            //text += "\nusing a clientID and a clientSecret key.";
            //text += "\n\nCreate a file called TranslateXML.settings in the same folder as the application ";
            //text += "and edit this file to contain 2 lines replacing xxx with your clientID and clientSecret.";
            //text += "\n\nxxx # CLIENTID";
            //text += "\nxxx # CLIENTSECRET";
            //text += "\n\nOther optional settings include # SBPATH, # OUTPUTPATH, # XMLFILE and # XMLPATH";
            //text += "\n\nDo you want to visit the Microsoft Translator site?";
            //if (MessageBox.Show(text, "Information", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            //{
            //    Process.Start("https://www.microsoft.com/en-us/translator/getstarted.aspx");
            //}
            string text = "This application uses Google Translator.";
            text += "\n\nOptionally create a file called TranslateXML.settings in the same folder as the application ";
            text += "and edit this file to contain settings.";
            text += "\n\nEach setting is on a separate line with a value followed by # and a setting name.";
            text += "\n\nSettings include # SBPATH, # OUTPUTPATH, # XMLFILE and # XMLPATH";
            MessageBox.Show(text, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void buttonHelp_Click(object sender, RoutedEventArgs e)
        {
            ShowHelp();
        }

        private void comboBoxInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxInput.SelectedIndex != 1)
            {
                listBoxFrom.SelectedItem = en;
                listBoxFrom.ScrollIntoView(listBoxFrom.SelectedItem);
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            dlg.InitialDirectory = textBoxOutput.Text;
            Nullable<bool> result = dlg.ShowDialog(this);
            if (result == true)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = dlg.FileName;
                comboBoxInput.Items.Add(item);
                comboBoxInput.SelectedIndex = comboBoxInput.Items.Count - 1;
            }
        }
    }
}
