﻿using System; // String, ...
using System.Collections.Generic; // List<T>, ...
using System.Windows; // Window, MessageBox, RoutedEventArgs, FontWeights, ...
using System.Windows.Controls; // TextBox, RichTextBox, ...
using System.Windows.Documents; // Run, Paragraph, FlowDocument, ...
using System.Windows.Input; // ApplicationCommands, ...
using System.Windows.Media; // Color, ColorConverter, SolidColorBrush, ...
using System.Text; // Encoding, ...
using System.IO; // Path, Directory, ...
using System.Text.RegularExpressions; // Regex, Match, ...
using Microsoft.Win32; // OpenFileDialog, SaveFileDialog, ...
using System.Diagnostics; // Stopwatch, ...
using System.Windows.Controls.Primitives; // MenuBase, ...
using System.Linq; // select, ...
using System.Xml.Linq; // XDocument, XElement, XAttribute, ...

namespace TMLTransliterators
{
    // Some common constants:
    public static class Constants
    {
        // App information:
        public static string title = "Simple Interactive TML (Transliteration Markup Language) Transliterators";
        public static string author = "Luka Tolić (Tolitch)";
        public static string source = "https://github.com/lukatolitch/";
        public static string license = "GNU General Public License v3.0";
        public static string version = "0.2.3";
        public static string date = "2023";

        // Default values:
        public static string default_transliterator_id = "...";
        public static string new_transliterator_id = "n";
        public static string default_transliterator_color = "#FFDDDDDD";
        public static System.Windows.Media.Color addition_color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF888888");
        public static System.Windows.Media.Color subtraction_color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#CCFFFFFF");
        public static string default_transliterator_font_family = "Times New Roman";
        public static string default_transliterator_description = "...";
        public static string windows_fonts_directory = "file:///C:/Windows/Fonts/";
        public static int initial_output_text_size = 20;
        public static int initial_input_text_size = 20;

        // Regular expressions:
        public static Regex regular_expression_for_whitespaces = new Regex(@"\s", RegexOptions.Compiled);
        public static Regex regular_expression_for_final_whitespaces = new Regex(@"\s$", RegexOptions.Compiled);
        public static Regex regular_expression_for_splitting = new Regex(@"(<[^\<>]+?>[^\<]+?</[^\<>]+?>)", RegexOptions.Compiled);
        public static Regex regular_expression_for_analyzing = new Regex(@"<(?'TRANSLITERATOR_ID'[^\<>]+?)>(?'TRANSLITERATION_TEXT'[^\<]+?)</\k'TRANSLITERATOR_ID'>", RegexOptions.Compiled);
        public static Regex regular_expression_for_marking_up = new Regex(@"(?'TRANSLITERATION_SEGMENT'<(?'TRANSLITERATOR_ID'[^\<>]+?)>(?'TRANSLITERATION_TEXT'[^\<]+?)</\k'TRANSLITERATOR_ID'>)", RegexOptions.Compiled);
        public static Regex regular_expression_for_characters = new Regex(@"(?'CONTAINS_AN_ALPHABETIC_CHARACTER'[\p{L}])", RegexOptions.Compiled);
    }

    // Interaction logic for "MainWindow.xaml":
    public partial class MainWindow : Window
    {
        // The field for the transliterators:
        Transliterators transliterators = new Transliterators();
        
        public string cell_content = "";
        public int current_tab_index = -1;

        public MainWindow()
        {
            InitializeComponent();

            // Set the title:
            Title = String.Format("{0} (Version: {1}) by {2}", Constants.title, Constants.version, Constants.author);

            // Set up the text boxes:
            input_text_box.SetValue(Paragraph.LineHeightProperty, 1.0);
            output_text_box.SetValue(Paragraph.LineHeightProperty, 1.0);

            // Set up the text size sliders:
            input_slider.Value = Constants.initial_input_text_size;
            output_slider.Value =  Constants.initial_output_text_size;

            // Try to load the transliteration tables from the local 'input' folder:
            string transliterators_input_directory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"inputs"));
            string transliterators_input_filename = "DefaultTransliterators.xml";
            string transliterators_input_path = System.IO.Path.Combine(transliterators_input_directory, transliterators_input_filename);
            LoadTransliterationDataGrids(transliterators_input_path, tab_control, false);

            // Recreate the context menus:
            RecreateInputContextMenu();
            RecreateOutputContextMenu();
        }

        public void LoadTransliterationDataGrids(string xml_path, TabControl tab_control, bool append)
        {
            try
            {
                // Try to load the XML document:
                XDocument xml_document = XDocument.Load(xml_path);
                
                if (append == false)
                {
                    // Clear the existing transliteration tables:
                    tab_control.Items.Clear();
                }

                // Get the transliterator nodes:
                IEnumerable<XElement> transliterator_nodes =
                    from transliterator_node in xml_document.Descendants("transliterator")
                    select transliterator_node;

                // Declare the string variables for the transliterator nodes' attributes:
                string id_string, color_string, font_family_string, description_string;

                // For each transliterator node:
                foreach (XElement transliterator_node in transliterator_nodes)
                {
                    // Get the transliteration node's attribute values:
                    XAttribute id_attribute = transliterator_node.Attribute("id");
                    id_string = (id_attribute != null) ? id_attribute.Value : Constants.default_transliterator_id;
                    XAttribute color_attribute = transliterator_node.Attribute("color");
                    color_string = (color_attribute != null) ? color_attribute.Value : Constants.default_transliterator_color;
                    XAttribute font_family_attribute = transliterator_node.Attribute("font_family");
                    font_family_string = (font_family_attribute != null) ? font_family_attribute.Value : Constants.default_transliterator_font_family;
                    XAttribute description_attribute = transliterator_node.Attribute("description");
                    description_string = (description_attribute != null) ? description_attribute.Value : Constants.default_transliterator_description;

                    // Get the map nodes:
                    IEnumerable<XElement> map_nodes =
                        from map_node in transliterator_node.Descendants("map")
                        select map_node;

                    // Initialize the transliteration data grid items:
                    TransliterationDataGridItems transliteration_items = new TransliterationDataGridItems();

                    // For each map node:
                    foreach (XElement map_node in map_nodes)
                    {
                        // Initialize a new transliteration data grid item:
                        TransliterationDataGridItem transliteration_item = new TransliterationDataGridItem();

                        // Get the map's comment attribute value:
                        XAttribute comment_attribute = map_node.Attribute("comment");
                        string comment_string = (comment_attribute != null) ? comment_attribute.Value : "...";
                        transliteration_item.Comment = comment_string;

                        // Get the map's input value:
                        string input_string = "...";
                        try { input_string = map_node.Descendants("input").First().Value; }
                        catch { }
                        transliteration_item.Input = input_string;

                        // Get the map's output value:
                        string output_string = "...";
                        try { output_string = map_node.Descendants("output").First().Value; }
                        catch { }
                        transliteration_item.Output = output_string;

                        // Add the transliteration item to transliteration items:
                        transliteration_items.Add(transliteration_item);
                    }

                    TransliterationDataGrid data_grid = new TransliterationDataGrid();
                    data_grid.ItemsSource = transliteration_items;
                    data_grid.id = id_string;
                    data_grid.color = color_string;
                    data_grid.font_family = font_family_string;
                    data_grid.description = description_string;

                    TabItem tab_item = new TabItem();
                    tab_item.Header = id_string;
                    System.Windows.Media.Color background_color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color_string);
                    tab_item.Background = new System.Windows.Media.SolidColorBrush(background_color);
                    //tab_item.BorderBrush = new System.Windows.Media.SolidColorBrush(background_color);
                    //tab_item.Foreground = new System.Windows.Media.SolidColorBrush(background_color);
                    //tab_item.Width = 100;
                    tab_item.MinWidth = 50;
                    tab_item.Content = data_grid;

                    tab_control.Items.Add(tab_item);
                    //tab_item.Focus();

                    SetDataGridStyle(data_grid);
                    BindDataGridEvents(data_grid);
                }

                // Focus on the first of the transliteration tables:
                if (tab_control.Items.Count > 0)
                {
                    TabItem first_tab_item = (TabItem)tab_control.Items[0];
                    first_tab_item.Focus();
                }
                else
                {
                }

                PropertiesChanged(false);
                DataGridChanged(true);
            }
            catch
            {
                //MessageBox.Show("XML Document Load Error!");
                DisplayMessage("XML Document Load Error!");
            }
        }

        public void DataGridChanged(bool data_grid_changed)
        {
            System.Windows.Media.Color color = new System.Windows.Media.Color() { A = 255, R = 0, G = 0, B = 0};
            if (data_grid_changed) { color.R = 255; }
            else{ color.G = 255; }
            compile_button.Background = new System.Windows.Media.SolidColorBrush(color);
            graph_tab_button.Background = new System.Windows.Media.SolidColorBrush(color);
        }

        public void PropertiesChanged(bool properties_changed)
        {
            System.Windows.Media.Color color = new System.Windows.Media.Color() { A = 255, R = 0, G = 0, B = 0 };
            if (properties_changed) { color.R = 255; }
            else { color.G = 255; }
            apply_tab_button.Background = new System.Windows.Media.SolidColorBrush(color);
        }

        public void UpdatePropertiesPanelDataGrid(TransliterationDataGrid data_grid)
        {
            id_text_box.Text = data_grid.id;
            color_text_box.Text = data_grid.color;
            font_text_box.Text = data_grid.font_family;
            description_text_box.Text = data_grid.description;
        }
        public void UpdatePropertiesPanelTextBox(TransliterationTextBox text_box)
        {
            id_text_box.Text = text_box.id;
            color_text_box.Text = text_box.color;
            font_text_box.Text = text_box.font_family;
            description_text_box.Text = text_box.description;
        }
        public void OnSelectedTabChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                ConvertAllTablesToDataGrids();
                TransliterationDataGrid data_grid = (TransliterationDataGrid)tab_control.SelectedContent;
                DisplayMessage(String.Format("Selected tab: Current={0} | Previous={1}", tab_control.SelectedIndex+1, this.current_tab_index+1));
                this.current_tab_index = tab_control.SelectedIndex;
                UpdatePropertiesPanelDataGrid(data_grid);
                PropertiesChanged(false);
            }
            catch
            {

            }
        }

        public void OnAboutMenuClick(object sender, RoutedEventArgs e)
        {
            string caption = "About";
            string text = String.Format("" +
                "{0}\n\n" +
                "[Version: {1} ({2})]\n\n" +
                "Original author: {3}\n" +
                "Source: {4}\n" +
                "License: {5}", Constants.title, Constants.version, Constants.date, Constants.author, Constants.source, Constants.license);
            MessageBox.Show(text, caption);
        }

        public void OnCompileButtonClick(object sender, RoutedEventArgs e)
        {
            // First check if there's any transliteration tables still in the text mode: if there are, convert them to the corresponding data grids
            ConvertAllTablesToDataGrids();

            if (tab_control.Items.Count > 0)
            {
                // Reset the transliterators:
                transliterators.Clear();

                // Create a compilation output window instance:
                CompilationWindow compilation_window = new CompilationWindow();

                // For each transliteration table:
                foreach (TabItem tab_item in tab_control.Items)
                {
                    TransliterationDataGrid data_grid = (TransliterationDataGrid)tab_item.Content;
                    TransliterationDataGridItems data_grid_items = (TransliterationDataGridItems)data_grid.ItemsSource;

                    // If the transliteration table has any items at all:
                    if (data_grid_items.Count > 0)
                    {
                        // If the transliteration talbe with the same ID does not already exist:
                        if (!transliterators.ContainsKey(data_grid.id))
                        {
                            transliterators.Add(data_grid.id, new Transliterator());
                            transliterators[data_grid.id].Load(data_grid.id, data_grid, compilation_window.compilation_text_box);
                            transliterators[data_grid.id].color = data_grid.color;
                            transliterators[data_grid.id].font_family = data_grid.font_family;
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }

                // Show the compilation output window:
                compilation_window.Show();

                TransliterateInputTextBox();
                DataGridChanged(false);
                RecreateInputContextMenu();
            }
            else
            {
                DisplayMessage("Nothing to compile!");
            }
        }

        public void OpenTransliterators(object sender, RoutedEventArgs e)
        {
            var open_file_dialog = new OpenFileDialog();
            open_file_dialog.FileName = "Transliterators";
            open_file_dialog.DefaultExt = ".xml";
            open_file_dialog.Filter = "Transliterators (.xml) |*.xml";
            bool? open_file_window_result = open_file_dialog.ShowDialog();
            if (open_file_window_result == true)
            {
                string path = open_file_dialog.FileName;
                LoadTransliterationDataGrids(path, tab_control, false);
                DataGridChanged(true);
                DisplayMessage("Transliteration tables loaded!");
            }
        }

        public void AppendTransliterators(object sender, RoutedEventArgs e)
        {
            var open_file_dialog = new OpenFileDialog();
            open_file_dialog.FileName = "Transliterators";
            open_file_dialog.DefaultExt = ".xml";
            open_file_dialog.Filter = "Transliterators (.xml) |*.xml";
            bool? open_file_window_result = open_file_dialog.ShowDialog();
            if (open_file_window_result == true)
            {
                string path = open_file_dialog.FileName;
                LoadTransliterationDataGrids(path, tab_control, true);
                DataGridChanged(true);
                DisplayMessage("Transliteration tables appended!");
            }
        }

        public void ClearTransliterators (object sender, RoutedEventArgs e)
        {
            try
            {
                tab_control.Items.Clear();
                DisplayMessage("All transliteration tables cleared!");
            }
            catch
            {
                DisplayMessage("Error: transliteration tables could not be cleared!");
            }

        }

        public void SaveTransliteratorsAs(object sender, RoutedEventArgs e)
        {
            // First check if there's any transliteration tables still in the text mode: if there are, convert them to the corresponding data grids
            ConvertAllTablesToDataGrids();

            var save_file_dialog = new SaveFileDialog();
            save_file_dialog.FileName = "Transliterators";
            save_file_dialog.DefaultExt = ".xml";
            save_file_dialog.Filter = "Transliterators (.xml) |*.xml";
            bool? result = save_file_dialog.ShowDialog();
            if (result == true)
            {
                string filename = save_file_dialog.FileName;
                string xml_string = transliterators.GetXmlString(tab_control);
                using (FileStream filestream = File.Create(filename))
                using (TextWriter text_writer = new StreamWriter(filestream))
                {
                    text_writer.Write(xml_string);
                }
                DisplayMessage("Transliteration tables saved!");
            }
        }
        public void OpenInputText(object sender, RoutedEventArgs e)
        {
            var open_file_dialog = new OpenFileDialog();
            open_file_dialog.FileName = "InputText";
            open_file_dialog.DefaultExt = ".txt";
            open_file_dialog.Filter = "Texts (.txt) |*.txt";
            bool? open_file_window_result = open_file_dialog.ShowDialog();
            if (open_file_window_result == true)
            {
                string filename = open_file_dialog.FileName;
                string input_text = File.ReadAllText(filename, Encoding.UTF8);
                input_text_box.Text = input_text;
                DisplayMessage("Input text loaded!");
            }
        }
        public void SaveInputTextAs(object sender, RoutedEventArgs e)
        {
            var save_file_dialog = new SaveFileDialog();
            save_file_dialog.FileName = "InputText";
            save_file_dialog.DefaultExt = ".txt";
            save_file_dialog.Filter = "Texts (.txt) |*.txt";
            bool? result = save_file_dialog.ShowDialog();
            if (result == true)
            {
                string filename = save_file_dialog.FileName;
                string input_text = input_text_box.Text;
                File.WriteAllText(filename, input_text);
                DisplayMessage("Input text saved!");
            }
        }

        public void SaveOutputTextAs(object sender, RoutedEventArgs e)
        {
            var save_file_dialog = new SaveFileDialog();
            save_file_dialog.FileName = "OutputText";
            save_file_dialog.DefaultExt = ".txt";
            save_file_dialog.Filter = "Texts (.txt) |*.txt";
            bool? result = save_file_dialog.ShowDialog();
            if (result == true)
            {
                string filename = save_file_dialog.FileName;
                TextRange output_text_range = new TextRange(output_text_box.Document.ContentStart, output_text_box.Document.ContentEnd);
                string output_text = output_text_range.Text;
                File.WriteAllText(filename, output_text);
                DisplayMessage("Output text saved!");
            }
        }

        public void SaveGraphAs(object sender, RoutedEventArgs e)
        {
            try
            {
                // Try this:
                TransliterationTable transliteration_table = (TransliterationTable)transliterators[id_text_box.Text].table;

                var save_file_dialog = new SaveFileDialog();
                save_file_dialog.FileName = "Graph";
                save_file_dialog.DefaultExt = ".graphml";
                save_file_dialog.Filter = "Graphs (.graphml) |*.graphml";
                bool? result = save_file_dialog.ShowDialog();
                if (result == true)
                {
                    Graph.Graph graph = new Graph.Graph();
                    int edge_index = 0;
                    foreach (int input_state in transliteration_table.Keys)
                    {
                        if (input_state == 0)
                        {
                            graph.AddNode(input_state.ToString(), input_state.ToString(), "start");
                        }

                        foreach (string input_string in transliteration_table[input_state].Keys)
                        {
                            TransliterationTableItem table_item = transliteration_table[input_state][input_string];
                            int output_state = table_item.OutputState;
                            string output_string = table_item.OutputString;
                            if (table_item.IsFinal)
                            {
                                graph.AddNode(output_state.ToString(), output_state.ToString(), "final");
                            }
                            else
                            {
                                graph.AddNode(output_state.ToString(), output_state.ToString(), "non-final");
                            }
                            graph.AddEdge(edge_index.ToString(), input_string + " → " + output_string, input_state.ToString(), output_state.ToString(), "default");
                            edge_index++;
                        }
                    }
                    string filename = save_file_dialog.FileName;
                    graph.SaveAsGraphML(filename);
                    DisplayMessage("Graph saved!");
                }
                else
                {
                }
            }
            catch
            {
                //MessageBox.Show("Graph save error! Have you compiled the transliterators?");
                DisplayMessage("Graph save error! Have you compiled the transliterators?");
            }
        }

        public void OnApplyTabButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TabItem tab_item = (TabItem)tab_control.SelectedItem;
                tab_item.Header = id_text_box.Text;
                System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color_text_box.Text);
                tab_item.Background = new SolidColorBrush(color);
                TransliterationDataGrid data_grid = (TransliterationDataGrid)tab_item.Content;
                data_grid.id = id_text_box.Text;
                data_grid.color = color_text_box.Text;
                data_grid.font_family = font_text_box.Text;
                data_grid.description = description_text_box.Text;
                SetDataGridStyle(data_grid);
                PropertiesChanged(false);
                DataGridChanged(true);
                RecreateInputContextMenu();
                DisplayMessage("Transliteration table properties changed!");
            }
            catch
            {
                DisplayMessage("Can't apply properties! No transliteration tables.");
            }
        }

        public void OnPropertyChanged(object sender, RoutedEventArgs e)
        {
            PropertiesChanged(true);
        }

        public void OnNewTabButtonClick(object sender, RoutedEventArgs e)
        {
            TabItem tab_item = new TabItem();
            tab_item.Header = Constants.new_transliterator_id;
            System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Constants.default_transliterator_color);
            tab_item.Background = new System.Windows.Media.SolidColorBrush(color);
            //tab_item.Width = 100;
            tab_item.MinWidth = 50;

            TransliterationDataGrid data_grid = new TransliterationDataGrid();
            data_grid.id = Constants.new_transliterator_id;
            data_grid.color = Constants.default_transliterator_color;
            data_grid.font_family = Constants.default_transliterator_font_family;
            data_grid.description = Constants.default_transliterator_description;

            TransliterationDataGridItem transliteration_item_pl = new TransliterationDataGridItem();
            transliteration_item_pl.Input = "n'";
            transliteration_item_pl.Output = "ń";
            transliteration_item_pl.Comment = "U+0144 LATIN SMALL LETTER N WITH ACUTE";
            
            TransliterationDataGridItem transliteration_item_ru = new TransliterationDataGridItem()
            {
                Input = "n",
                Output = "н",
                Comment = "U+043D CYRILLIC SMALL LETTER EN"
            };

            TransliterationDataGridItems transliteration_items = new TransliterationDataGridItems();
            transliteration_items.Add(transliteration_item_pl);
            transliteration_items.Add(transliteration_item_ru);
            data_grid.ItemsSource = transliteration_items;

            tab_item.Content = data_grid;
            tab_control.Items.Add(tab_item);
            tab_control.SelectedItem = tab_item;

            SetDataGridStyle(data_grid);
            BindDataGridEvents(data_grid);

            DataGridChanged(true);

            DisplayMessage("New transliteration table created!");
        }

        public void OnDeleteTabButtonClick(object sender, RoutedEventArgs e)
        {
            if (tab_control.Items.Count > 0)
            {
                TabItem selected_tab_item = (TabItem)tab_control.SelectedItem;
                tab_control.Items.Remove(selected_tab_item);
                DataGridChanged(true);
                DisplayMessage("Transliteration table deleted!");
            }
            else
            {
                DisplayMessage("Nothing to delete!");
            }
        }

        public void OnReverseTabButtonClick(object sender, RoutedEventArgs e)
        {
            // First check if there's any transliteration tables still in the text mode: if there are, convert them to the corresponding data grids
            ConvertAllTablesToDataGrids();

            if (tab_control.Items.Count > 0)
            {
                TabItem tab_item = (TabItem)tab_control.SelectedItem;
                TransliterationDataGrid data_grid = (TransliterationDataGrid)tab_item.Content;
                TransliterationDataGridItems data_grid_items = (TransliterationDataGridItems)data_grid.ItemsSource;
                data_grid.ItemsSource = null;
                string temporary = "";
                foreach (TransliterationDataGridItem transliteration_item in data_grid_items)
                {
                    temporary = transliteration_item.Input;
                    transliteration_item.Input = transliteration_item.Output;
                    transliteration_item.Output = temporary;
                }
                data_grid.ItemsSource = data_grid_items;
                DataGridChanged(true);
                DisplayMessage("Transliteration table reversed!");
            }
            else
            {
                DisplayMessage("Nothing to reverse!");
            }
        }

        public void OnModeTabButtonClick(object sender, RoutedEventArgs e)
        {
            if (tab_control.Items.Count > 0)
            {
                TabItem tab_item = (TabItem)tab_control.SelectedItem;
                try
                {
                    TransliterationDataGrid data_grid = (TransliterationDataGrid)tab_item.Content; // Try this first: if the item is a TransliterationDataGrid control...
                    tab_item.Content = ConvertTableToTextBox(data_grid);
                    DataGridChanged(true);
                    DisplayMessage("Transliteration table mode changed to text!");
                }
                catch (System.InvalidCastException icexception)
                {
                    TransliterationTextBox text_box = (TransliterationTextBox)tab_item.Content; // Then try this: if the item is a TransliterationTextBox control...
                    tab_item.Content = ConvertTableToDataGrid(text_box);
                    OnApplyTabButtonClick(tab_item.Content, null);
                    DataGridChanged(true);
                    DisplayMessage("Transliteration table mode changed to data grid!");
                }
                catch // Final catch...
                {
                    DisplayMessage("Transliteration table mode could not be changed!");
                }
            }
            else
            {
                DisplayMessage("No transliteration tables to change the mode of!");
            }
        }

        public TransliterationTextBox ConvertTableToTextBox(TransliterationDataGrid transliteration_data_grid)
        {
            TransliterationDataGridItems transliteration_data_grid_items = (TransliterationDataGridItems)transliteration_data_grid.ItemsSource;
            TransliterationTextBox transliteration_text_box = new TransliterationTextBox();
            foreach (TransliterationDataGridItem transliteration_item in transliteration_data_grid_items)
            {
                transliteration_text_box.AppendText(
                    String.Format("{0}\t{1}\t{2}\n",
                        transliteration_item.Input,
                        transliteration_item.Output,
                        transliteration_item.Comment
                        )
                    );
            }
            transliteration_text_box.id = transliteration_data_grid.id;
            transliteration_text_box.color = transliteration_data_grid.color;
            transliteration_text_box.font_family = transliteration_data_grid.font_family;
            transliteration_text_box.description = transliteration_data_grid.description;
            return (transliteration_text_box);
        }

        public TransliterationDataGrid ConvertTableToDataGrid(TransliterationTextBox transliteration_text_box)
        {
            TransliterationDataGridItems transliteration_data_grid_items = new TransliterationDataGridItems();
            string[] transliteration_text_box_lines = transliteration_text_box.Text.Split('\n');
            foreach (string transliteration_text_box_line in transliteration_text_box_lines)
            {
                string[] transliteration_text_box_entries = transliteration_text_box_line.Split("\t");
                TransliterationDataGridItem transliteration_data_grid_item = new TransliterationDataGridItem();
                if (transliteration_text_box_entries.Length > 0)
                {
                    transliteration_data_grid_item.Input = Constants.regular_expression_for_whitespaces.Replace(transliteration_text_box_entries[0], "");
                    if (transliteration_text_box_entries.Length > 1)
                    {
                        transliteration_data_grid_item.Output = Constants.regular_expression_for_whitespaces.Replace(transliteration_text_box_entries[1], "");
                        if (transliteration_text_box_entries.Length > 2)
                        {
                            transliteration_data_grid_item.Comment = Constants.regular_expression_for_final_whitespaces.Replace(transliteration_text_box_entries[2], "");
                        }
                    }
                }
                transliteration_data_grid_items.Add(transliteration_data_grid_item);
            }
            TransliterationDataGrid transliteration_data_grid = new TransliterationDataGrid();
            transliteration_data_grid.ItemsSource = transliteration_data_grid_items;
            transliteration_data_grid.id = transliteration_text_box.id;
            transliteration_data_grid.color = transliteration_text_box.color;
            transliteration_data_grid.font_family = transliteration_text_box.font_family;
            transliteration_data_grid.description = transliteration_text_box.description;
            return (transliteration_data_grid);
        }

        public void ConvertAllTablesToDataGrids()
        {
            foreach (TabItem tab_item in tab_control.Items)
            {
                try // Try this: the item is a TransliterationTextBox control
                {
                    TransliterationTextBox text_box = (TransliterationTextBox)tab_item.Content;
                    TransliterationDataGrid data_grid = ConvertTableToDataGrid(text_box); // Convert the text box control to the corresponding data grid control
                    tab_item.Content = data_grid;
                    tab_item.Header = id_text_box.Text;
                    System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color_text_box.Text);
                    tab_item.Background = new SolidColorBrush(color);
                    data_grid.id = id_text_box.Text;
                    data_grid.color = color_text_box.Text;
                    data_grid.font_family = font_text_box.Text;
                    data_grid.description = description_text_box.Text;
                    SetDataGridStyle(data_grid);
                    PropertiesChanged(false);
                    DataGridChanged(true);
                    DisplayMessage("Transliteration table converted from text to data grid!");
                }
                catch
                {

                }
            }
        }

        public void OnDuplicateTabButtonClick(object sender, RoutedEventArgs e)
        {
            // First check if there's any transliteration tables still in the text mode: if there are, convert them to the corresponding data grids
            ConvertAllTablesToDataGrids();

            if (tab_control.Items.Count > 0)
            {
                TabItem source_tab_item = (TabItem)tab_control.SelectedItem;
                TabItem target_tab_item = new TabItem();

                target_tab_item.Header = source_tab_item.Header + "-copy";
                target_tab_item.Background = source_tab_item.Background;

                TransliterationDataGrid source_data_grid = (TransliterationDataGrid)source_tab_item.Content;
                TransliterationDataGrid target_data_grid = DuplicateTransliterationDataGrid(source_data_grid);
                target_tab_item.Content = target_data_grid;
                SetDataGridStyle(target_data_grid);
                BindDataGridEvents(target_data_grid);

                tab_control.Items.Add(target_tab_item);
                tab_control.SelectedItem = target_tab_item;

                DataGridChanged(true);
                DisplayMessage("Transliteration table duplicated!");
            }
            else
            {
                DisplayMessage("Nothing to duplicate!");
            }
        }

        public TransliterationDataGrid DuplicateTransliterationDataGrid(TransliterationDataGrid source_data_grid)
        {
            TransliterationDataGrid target_data_grid = new TransliterationDataGrid();

            target_data_grid.id = source_data_grid.id + "-copy";
            target_data_grid.color = source_data_grid.color;
            target_data_grid.font_family = source_data_grid.font_family;
            target_data_grid.description = source_data_grid.description;

            TransliterationDataGridItems source_data_grid_items = (TransliterationDataGridItems)source_data_grid.ItemsSource;
            TransliterationDataGridItems target_data_grid_items = new TransliterationDataGridItems();
            foreach (TransliterationDataGridItem source_data_grid_item in source_data_grid_items)
            {
                TransliterationDataGridItem target_data_grid_item = new TransliterationDataGridItem();
                target_data_grid_item.Input = source_data_grid_item.Input;
                target_data_grid_item.Output = source_data_grid_item.Output;
                target_data_grid_item.Comment = source_data_grid_item.Comment;
                target_data_grid_items.Add(target_data_grid_item);
            }
            target_data_grid.ItemsSource = target_data_grid_items;

            return (target_data_grid);
        }

        public void OnInputTextBoxChanged(object sender, RoutedEventArgs e)
        {
            TransliterateInputTextBox();
        }

        public void OnDataGridCellEditStarting(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            TextBox cell_text_box = (TextBox)e.EditingElement;
            this.cell_content = cell_text_box.Text;
        }
        public void OnDataGridCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            TextBox cell_text_box = (TextBox)e.EditingElement;
            if (cell_text_box.Text != this.cell_content)
            {
                DataGridChanged(true);
            }
        }

        public void OnOutputSliderValueChanged(object sender, RoutedEventArgs e)
        {
            try // E.g., if the text box is initialized
            {
                int slider_value = (int)output_slider.Value;
                string slider_value_string = slider_value.ToString();
                output_slider_value_text_box.Content = slider_value_string + " pt";
                output_text_box.FontSize = slider_value;
                DisplayMessage(String.Format("Output font size: {0} pt", slider_value_string));
            }
            catch
            {
            }
        }
        public void OnInputSliderValueChanged(object sender, RoutedEventArgs e)
        {
            try // E.g., if the text box is initialized
            {
                int slider_value = (int)input_slider.Value;
                string slider_value_string = slider_value.ToString();
                input_slider_value_text_box.Content = slider_value_string + " pt";
                input_text_box.FontSize = slider_value;
                DisplayMessage(String.Format("Input font size: {0} pt", slider_value_string));
            }
            catch
            {
            }
        }
        public void TransliterateInputTextBox()
        {
            int transliteration_segments = 0;
            int transliteration_characters = 0;

            Stopwatch stop_watch = new Stopwatch();
            stop_watch.Start();

            // [1] TRANSLITERATE THE CONTENTS OF THE INPUT TEXT BOX:
            //TextRange text_range = new TextRange(input_text_box.Document.ContentStart, input_text_box.Document.ContentEnd);
            string text = input_text_box.Text;
            // [1.1] SPLIT THE INPUT TEXT TO TRANSLITERATION SEGMENTS:
            List<TransliterationSegment> transliteration_list = new List<TransliterationSegment>();
            string transliteration_id, transliteration_text, transliteration_font_family, transliterated_string;
            string[] splitted_strings = Constants.regular_expression_for_splitting.Split(text);
            foreach (string splitted_string in splitted_strings)
            {
                Match match = Constants.regular_expression_for_analyzing.Match(splitted_string);
                // [1.2.1] TRANSLITERATION MARKUP SEGMENT MATCH:
                if (match.Success)
                { 
                    transliteration_id = match.Groups["TRANSLITERATOR_ID"].Value;
                    transliteration_text = match.Groups["TRANSLITERATION_TEXT"].Value;
                    try // [1.2.1.1] IF THERE IS A TRANSLITERATOR WITH SUCH AN ID, TRANSLITERATE
                    {
                        transliterated_string = transliterators[transliteration_id].transliterate(transliteration_text);
                        transliteration_font_family = transliterators[transliteration_id].font_family;
                    }
                    catch // [1.2.1.2] IF THERE IS NO TRANSLITERATOR WITH SUCH AN ID, DO NOT TRANSLITERATE
                    {
                        transliterated_string = transliteration_text;
                        transliteration_font_family = Constants.default_transliterator_font_family;
                    }
                    TransliterationSegment transliteration_item = new TransliterationSegment(
                        transliteration_id,
                        transliterated_string,
                        transliteration_font_family
                        );
                    transliteration_list.Add(transliteration_item);
                    transliteration_segments++;
                    transliteration_characters += transliteration_text.Length;
                }
                // [1.2.2] NO TRANSLITERATION MARKUP SEGMENT MATCH:
                else
                {
                    // [1.2.2.1] DO NOT TRANSLITERATE:
                    TransliterationSegment transliteration_item = new TransliterationSegment(
                        "",
                        splitted_string,
                        Constants.default_transliterator_font_family
                        );
                    transliteration_list.Add(transliteration_item);
                }
            }
            // [2] WRITE TO THE OUTPUT TEXT BOX:
            FlowDocument output_flow_document = new FlowDocument();
            Paragraph paragraph = new Paragraph();
            foreach (TransliterationSegment transliteration_segment in transliteration_list)
            {
                Run output_run = new Run();
                output_run.Text = transliteration_segment.transliteration_text;
                if (transliterators.ContainsKey(transliteration_segment.transliterator_id))
                {
                    System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(transliterators[transliteration_segment.transliterator_id].color);
                    output_run.Background = new System.Windows.Media.SolidColorBrush(color);
                    //output_run.Foreground = new SolidColorBrush(Colors.Black);
                    output_run.FontFamily = new FontFamily(Constants.windows_fonts_directory + "#" + transliteration_segment.transliteration_font_family);
                    //output_run.FontWeight = FontWeights.Bold;
                    //output_run.FontStyle = FontStyles.Italic;
                    //output_run.FontSize = 30;
                }
                else
                {
                    output_run.Foreground = new System.Windows.Media.SolidColorBrush(Colors.Gray);
                }
                paragraph.Inlines.Add(output_run);
            }
            output_flow_document.Blocks.Add(paragraph);
            output_text_box.Document = output_flow_document;

            stop_watch.Stop();
            DisplayMessage(String.Format("Transliterated in {0:f5} seconds! [Transliteration segments: {1} | Total characters transliterated: {2}]", stop_watch.Elapsed.TotalMilliseconds / 1000, transliteration_segments, transliteration_characters));
        }

        public void DisplayMessage(string message)
        {
            message_text_box.Text = String.Format(">> {0}", message);
        }

        public void ClearTabControl()
        {
            TabItem tab_item = new TabItem();
            for (int i = tab_control.Items.Count - 1; i >= 0; i--)
            {
                tab_item = (TabItem)tab_control.Items[i];
                tab_control.Items.Remove(tab_item);
            }
        }

        public void SetDataGridStyle(TransliterationDataGrid data_grid)
        {
            //data_grid.MinColumnWidth = 50;
            data_grid.CanUserReorderColumns = false;
            data_grid.CanUserAddRows = true;
            data_grid.CanUserDeleteRows = true;
            data_grid.CanUserResizeRows = false;
            data_grid.FontSize = 16;
            //data_grid.FontWeight = FontWeights.Bold;

            System.Windows.Media.Color grid_lines_color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(data_grid.color) - Constants.subtraction_color;
            data_grid.HorizontalGridLinesBrush = new System.Windows.Media.SolidColorBrush(grid_lines_color);
            data_grid.VerticalGridLinesBrush = new System.Windows.Media.SolidColorBrush(grid_lines_color);

            System.Windows.Media.Color row_color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(data_grid.color);
            data_grid.RowBackground = new System.Windows.Media.SolidColorBrush(row_color);
            System.Windows.Media.Color alternating_row_color = row_color + Constants.addition_color;
            data_grid.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(alternating_row_color);
        }

        public void BindDataGridEvents(TransliterationDataGrid data_grid)
        {
            data_grid.PreparingCellForEdit += OnDataGridCellEditStarting;
            data_grid.CellEditEnding += OnDataGridCellEditEnding;
        }

        public void RecreateInputContextMenu()
        {
            MenuBase context_menu = new ContextMenu();

            MenuItem cut_item = new MenuItem();
            cut_item.Command = ApplicationCommands.Cut;
            context_menu.Items.Add(cut_item);

            MenuItem copy_item = new MenuItem();
            copy_item.Command = ApplicationCommands.Copy;
            context_menu.Items.Add(copy_item);

            MenuItem select_all_item = new MenuItem();
            select_all_item.Command = ApplicationCommands.SelectAll;
            context_menu.Items.Add(select_all_item);

            context_menu.Items.Add(new Separator());

            MenuItem clear_all_text = new MenuItem();
            clear_all_text.Header = "Clear All Input Text...";
            clear_all_text.Click += ClearAllInputText;
            context_menu.Items.Add(clear_all_text);

            context_menu.Items.Add(new Separator());

            MenuItem open_text = new MenuItem();
            open_text.Header = "Open Input Text...";
            open_text.Click += OpenInputText;
            context_menu.Items.Add(open_text);

            MenuItem save_text = new MenuItem();
            save_text.Header = "Save Input Text As...";
            save_text.Click += SaveInputTextAs;
            context_menu.Items.Add(save_text);

            if (transliterators.Count > 0)
            {
                context_menu.Items.Add(new Separator());

                foreach (KeyValuePair<string, Transliterator> transliterator_pair in transliterators)
                {
                    MenuItem tag_item = new MenuItem();
                    tag_item.Header = transliterator_pair.Key;
                    System.Windows.Media.Color background_color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(transliterator_pair.Value.color);
                    tag_item.Background = new System.Windows.Media.SolidColorBrush(background_color);
                    tag_item.FontWeight = FontWeights.Bold;
                    tag_item.Click += InsertTag;
                    context_menu.Items.Add(tag_item);
                }
            }
            else
            {
            }

            input_text_box.ContextMenu = (ContextMenu)context_menu;
        }

        public void InsertTag(object sender, RoutedEventArgs e)
        {
            MenuItem menu_item = (MenuItem)sender;
            input_text_box.SelectedText = String.Format("<{0}>{1}</{0}>", menu_item.Header.ToString(), input_text_box.SelectedText);
        }

        public void ClearAllInputText(object sender, RoutedEventArgs e)
        {
            input_text_box.Clear();
        }

        public void RecreateOutputContextMenu()
        {
            MenuBase context_menu = new ContextMenu();

            MenuItem copy_item = new MenuItem();
            copy_item.Command = ApplicationCommands.Copy;
            context_menu.Items.Add(copy_item);

            MenuItem select_all_item = new MenuItem();
            select_all_item.Command = ApplicationCommands.SelectAll;
            context_menu.Items.Add(select_all_item);

            context_menu.Items.Add(new Separator());

            MenuItem save_text = new MenuItem();
            save_text.Header = "Save Output Text As...";
            save_text.Click += SaveOutputTextAs;
            context_menu.Items.Add(save_text);

            output_text_box.ContextMenu = (ContextMenu)context_menu;
        }
    }

    // Data source for the DataGrid:
    public class TransliterationDataGridItem
    {
        private string input = "";
        private string output = "";
        private string comment = "";

        public string Input
        {
            get{ return (this.input); }
            set{ this.input = value; }
        }

        public string Output
        {
            get{ return (this.output); }
            set{ this.output = value; }
        }

        public string Comment
        {
            get{ return (this.comment); }
            set{ this.comment = value; }
        }
    }

    public class TransliterationDataGridItems : List<TransliterationDataGridItem>
    {
        public TransliterationDataGridItems() : base()
        {
        }
    }

    // The DataGrid for the transliteration data:
    public class TransliterationDataGrid : DataGrid
    {
        public string id = Constants.default_transliterator_id;
        public string color = Constants.default_transliterator_color;
        public string font_family = Constants.default_transliterator_font_family;
        public string description = Constants.default_transliterator_description;

        public TransliterationDataGrid() : base()
        {

        }
    }

    public class TransliterationTextBox : TextBox
    {
        public string id = Constants.default_transliterator_id;
        public string color = Constants.default_transliterator_color;
        public string font_family = Constants.default_transliterator_font_family;
        public string description = Constants.default_transliterator_description;

        public TransliterationTextBox() : base()
        {
            this.AcceptsReturn = true;
            this.AcceptsTab = true;
            this.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Visible;
            this.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Visible;
            this.FontSize = 14;
        }
    }

    // The structure used in the transliteration process:
    public class TransliterationSegment
    {
        public string transliterator_id = Constants.default_transliterator_id;
        public string transliteration_text = "";
        public string transliteration_font_family = Constants.default_transliterator_font_family;

        public TransliterationSegment(string id, string text, string font_family)
        {
            this.transliterator_id = id;
            this.transliteration_text = text;
            this.transliteration_font_family = font_family;
        }
    }
}