using System; // Console, ...
using System.Collections.ObjectModel; // Collection, ...
using System.Collections.Generic; // Dictionary, KeyValuePair, SortedSet, Stack, ...
using System.Text; // StringBuilder, ...
using System.IO; // Path, File, FileStream, StreamReader, TextReader, ...
using System.Windows.Controls; // RichTextBox, ...
using System.Globalization; // TextInfo, CultureInfo, ...
using System.Xml; // XmlDocument, XmlNode, XmlElement, ...
using System.Text.RegularExpressions; // Match, ...

namespace TMLTransliterators
{

    public class TransliterationMap : Dictionary<string, string>
    {
        public TransliterationMap() : base()
        {
        }

        public void ParseDataGrid(DataGrid data_grid)
        {
            TransliterationDataGridItems transliteration_items = (TransliterationDataGridItems)data_grid.ItemsSource;
            foreach (TransliterationDataGridItem transliteration_item in transliteration_items)
            {
                string[] inputs = Constants.regular_expression_for_whitespaces.Replace(transliteration_item.Input, "").Split(",");
                string[] outputs = Constants.regular_expression_for_whitespaces.Replace(transliteration_item.Output, "").Split(",");
                string output;

                foreach (string input in inputs)
                {
                    TextInfo text_info = CultureInfo.CurrentCulture.TextInfo;

                    // [1] LOWERCASE:
                    if (this.TryGetValue(text_info.ToLower(input), out output))
                    { this[text_info.ToLower(input)] = text_info.ToLower(outputs[0]); }
                    else { this.Add(text_info.ToLower(input), text_info.ToLower(outputs[0])); }

                    Match input_match = Constants.regular_expression_for_characters.Match(input);
                    if (input_match.Success) // If the input string is a string with at least one alphabetic character:
                    {
                        // [2] TITLECASE:
                        if (this.TryGetValue(text_info.ToTitleCase(input), out output))
                        { this[text_info.ToTitleCase(input)] = text_info.ToTitleCase(outputs[0]); }
                        else { this.Add(text_info.ToTitleCase(input), text_info.ToTitleCase(outputs[0])); }

                        // [3] UPPERCASE:
                        if (this.TryGetValue(text_info.ToUpper(input), out output))
                        { this[text_info.ToUpper(input)] = text_info.ToUpper(outputs[0]); }
                        else { this.Add(text_info.ToUpper(input), text_info.ToUpper(outputs[0])); }
                    }
                    else // If the input string is a string with not a single alphabetic character:
                    {
                        Match output_match = Constants.regular_expression_for_characters.Match(outputs[0]);
                        if (output_match.Success) // If the first output string is a string with at least one alphabetic character:
                        {
                            // [4] DOUBLECASE:
                            if (this.TryGetValue(input+input, out output))
                            { this[input+input] = text_info.ToUpper(outputs[0]); }
                            else { this.Add(input+input, text_info.ToUpper(outputs[0])); }
                        }
                        else // If the first output string is a string with not a single alphabetic character:
                        {
                            // [5] NON-ALPHABETICS
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder string_builder = new StringBuilder();
            string_builder.Append(">> Constructing the transliteration map:\n\n");
            int i = 1;
            foreach (KeyValuePair<string, string> pair in this)
            {
                string_builder.Append(String.Format("[{0}] \"{1}\" -> \"{2}\"\n", i, pair.Key, pair.Value));
                i++;
            }
            return (string_builder.ToString());
        }

    }

    public class TransliterationTreeNode
    {

        protected TransliterationTreeNode supernode;
        protected TransliterationTreeNodes subnodes;

        private string input_string;
        private string output_string;
        private bool is_final;
        private int id;

        public TransliterationTreeNode()
        {
            this.Supernode = null;
            this.Subnodes = new TransliterationTreeNodes();

            this.input_string = null;
            this.output_string = null;
            this.is_final = false;
            this.id = -1;
        }

        public TransliterationTreeNode(string input_string, string output_string, bool is_final, int id)
        {
            this.Supernode = null;
            this.Subnodes = new TransliterationTreeNodes();

            this.input_string = input_string;
            this.output_string = output_string;
            this.is_final = is_final;
            this.id = id;
        }

        public TransliterationTreeNode Supernode
        {
            get { return (this.supernode); }
            set { this.supernode = value; }
        }

        public TransliterationTreeNodes Subnodes
        {
            get { return (this.subnodes); }
            set { this.subnodes = value; }
        }

        public string InputString
        {
            get { return (this.input_string); }
            set { this.input_string = value; }
        }

        public string OutputString
        {
            get { return (this.output_string); }
            set { this.output_string = value; }
        }

        public bool IsFinal
        {
            get { return (this.is_final); }
            set { this.is_final = value; }
        }

        public int ID
        {
            get { return (this.id); }
            set { this.id = value; }
        }

        public void SetParent(TransliterationTreeNode supernode)
        {
            this.Supernode = supernode;
            supernode.AddSubnode(this);
        }

        public void AddSubnode(TransliterationTreeNode subnode)
        {
            this.Subnodes.Add(subnode);
            subnode.Supernode = this;
        }

    }

    public class TransliterationTreeNodes : Collection<TransliterationTreeNode>
    {
        public TransliterationTreeNodes() : base()
        {
        }
    }

    public class TransliterationTree
    {

        private TransliterationTreeNode root;

        public TransliterationTree()
        {
            this.Root = new TransliterationTreeNode();
        }

        public TransliterationTree(TransliterationTreeNode root)
        {
            this.root = root;
        }

        public TransliterationTreeNode Root
        {
            get { return (this.root); }
            set { this.root = value; }
        }

        public void ParseTransliterationMap(TransliterationMap map)
        {
            // [1.1] DECLARATION:
            int i, l, d; // i = input character index; l = input string length; d = current node id
            string c, s, z; // c = input character (as string); s = output string; z = output string for input character
            TransliterationTreeNode n, fn, nn; // n = current node; fn = found (sub)node; nn = new node

            // [1.2] INITIALIZATION:
            this.Root.ID = 0; // Set root node's id to 0
            d = 1; // Set current node's id to 1
            n = this.Root; // Set the current node to refer to the root node

            // [2] PROCESSING:
            foreach (KeyValuePair<string, string> m in map) // For each pair of input and output strings in map
            {
                s = m.Value; // Get the current output string
                l = m.Key.Length; // Get the length of the current input string
                for (i = 0; i < l; i++) // For each character of the current input string
                {
                    c = m.Key[i].ToString(); // Get the current input character (as a string)

                    // [3] CLASSIFICATION AND ACTION:
                    fn = this.FindSubnodeByInputString(n, c); // Try to find a subnode (of the current node) that contains the current input character
                    // [3.1] NEW NODE:
                    if (fn == null) // If the subnode with the given input character does not already exist
                    {
                        nn = new TransliterationTreeNode(); // Create a new node

                        // [3.1.1] LAST CHARACTER:
                        if (i == l - 1) // If it is the last character of the input string
                        {
                            this.SetNode(nn, c, s, true, d); // Set the new node
                        }
                        // [3.1.2] NOT THE LAST CHARACTER:
                        else
                        {
                            // [3.1.2.1] RECOGNIZED INPUT CHARACTER:
                            if (map.TryGetValue(c, out z)) // If the current input character is itself an input string defined in the map, and get it's output string
                            {
                                // [3.1.2.1.1] FIRST CHARACTER:
                                if (i == 0) // If the current input character is the first (initial) character of the current input string
                                {
                                    this.SetNode(nn, c, z, true, d); // Set the new node
                                }
                                // [3.1.2.1.2] NOT THE FIRST CHARACTER:
                                else // If the current input character is not the first (initial) character of the current input string
                                {
                                    this.SetNode(nn, c, z, false, d); // Set the new node
                                }

                            }
                            // [3.1.2.2] UNRECOGNIZED INPUT CHARACTER:
                            else // If the current input character is not itself an input string defined in the map
                            {
                                this.SetNode(nn, c, c, false, d); // Set the new node
                            }
                        }
                        n.AddSubnode(nn); // Add the new node as a subnode to the current node
                        n = nn; // Set the current node to refer to the new node
                        d++; // Increment the current node id variable
                    }
                    // [3.2] EXISTING NODE:
                    else // If the subnode with the given input character already exists
                    {
                        if (i == l - 1) // If it is the last character of the input string
                        {
                            this.SetNode(fn, fn.InputString, s, true, fn.ID);  // Set the found node
                        }
                        n = fn; // Set the current node to refer to the found node
                    }
                } // for

                // [4] RESET THE CURRENT NODE:
                n = this.Root;
            } // foreach
        }

        public void SetNode(TransliterationTreeNode node, string input_character, string output_string, bool is_final, int id)
        {
            node.InputString = input_character;
            node.OutputString = output_string;
            node.IsFinal = is_final;
            node.ID = id;
        }

        public TransliterationTreeNode FindSubnodeByInputString(TransliterationTreeNode node, string input_string)
        {
            foreach (TransliterationTreeNode subnode in node.Subnodes)
            {
                if (subnode.InputString.Equals(input_string))
                {
                    return (subnode);
                }
            }
            return (null);
        }

        public TransliterationTreeNode FindNodeByInputString(string input_string)
        {
            TransliterationTreeNode found_node;
            found_node = this.FindNodeByInputStringSubroutine(this.Root, input_string);
            return (found_node);
        }

        private TransliterationTreeNode FindNodeByInputStringSubroutine(TransliterationTreeNode node, string input_string)
        {
            if (node.InputString.Equals(input_string))
            {
                return (node);
            }
            else
            {
                foreach (TransliterationTreeNode sub_node in node.Subnodes)
                {
                    TransliterationTreeNode found_node;
                    found_node = this.FindNodeByInputStringSubroutine(sub_node, input_string);
                    if (found_node != null)
                    {
                        return (found_node);
                    }
                }
            }
            return (null);
        }

        public void RemoveNodeByInputString(string input_string)
        {
            TransliterationTreeNode found_node;
            found_node = this.FindNodeByInputString(input_string);
            if (found_node != null)
            {
                TransliterationTreeNode super_node = found_node.Supernode;
                super_node.Subnodes.Remove(found_node);
                found_node.Supernode = null;
            }
        }

        public override string ToString()
        {
            StringBuilder string_builder = new StringBuilder();
            string_builder.Append(">> Constructing the transliteration tree:\n\n");
            if (this.Root != null)
            {
                string_builder.Append(this.GetSubtreeString(this.Root, 0));
            }
            else
            {
                string_builder.Append("- The tree is empty...\n");
            }
            return (string_builder.ToString());
        }

        private string GetSubtreeString(TransliterationTreeNode node, int depth)
        {
            StringBuilder string_builder = new StringBuilder();
            string prefix = new string('\t', depth);
            if (node.IsFinal)
            {
                string_builder.Append(String.Format("{0}[{1}]\"{2}\" -> \"{3}\" (F)\n", prefix, node.ID, node.InputString, node.OutputString));
            }
            else
            {
                string_builder.Append(String.Format("{0}[{1}]\"{2}\" -> \"{3}\"\n", prefix, node.ID, node.InputString, node.OutputString));
            }
            foreach (TransliterationTreeNode sub_node in node.Subnodes)
            {
                string_builder.Append(GetSubtreeString(sub_node, depth + 1));
            }
            return (string_builder.ToString());
        }

    }

    public class TransliterationTableItem
    {

        private int output_state;
        private string output_string;
        private bool is_final;

        public TransliterationTableItem()
        {
            this.OutputState = -1;
            this.OutputString = null;
            this.IsFinal = false;
        }

        public TransliterationTableItem(int output_state, string output_string, bool is_final)
        {
            this.OutputState = output_state;
            this.OutputString = output_string;
            this.IsFinal = is_final;
        }

        public int OutputState
        {
            get { return (this.output_state); }
            set { this.output_state = value; }
        }

        public string OutputString
        {
            get { return (this.output_string); }
            set { this.output_string = value; }
        }

        public bool IsFinal
        {
            get { return (this.is_final); }
            set { this.is_final = value;}
        }
    }

    public class TransliterationTable : Dictionary<int, Dictionary<string, TransliterationTableItem>>
    {

        private int start_state;

        public TransliterationTable() : base()
        {
            this.start_state = -1;
        }

        public int StartState
        {
            get { return (this.start_state); }
            set { this.start_state = value; }
        }
        public void SetTransition(int input_state, string input_string, int output_state, string output_string, bool is_final)
        {
            TransliterationTableItem table_item = new TransliterationTableItem(output_state, output_string, is_final);
            if (this.ContainsKey(input_state))
            {
                if (this[input_state].ContainsKey(input_string))
                {
                    this[input_state][input_string] = table_item;
                }
                else
                {
                    this[input_state].Add(input_string, table_item);
                }
            }
            else
            {
                Dictionary<string, TransliterationTableItem> column = new Dictionary<string, TransliterationTableItem>();
                column.Add(input_string, table_item);
                this.Add(input_state, column);
            }
        }

        public void ParseTransliterationTree(TransliterationTree tree)
        {
            this.start_state = tree.Root.ID;
            this.ParseTransliterationTreeSubroutine(tree.Root);
        }

        private void ParseTransliterationTreeSubroutine(TransliterationTreeNode node)
        {
            foreach (TransliterationTreeNode subnode in node.Subnodes)
            {
                this.SetTransition(node.ID, subnode.InputString, subnode.ID, subnode.OutputString, subnode.IsFinal); ;
                ParseTransliterationTreeSubroutine(subnode);
            }
        }

        public override string ToString()
        {

            SortedSet<int> input_states = new SortedSet<int>();
            foreach (int input_state in this.Keys)
            {
                input_states.Add(input_state);
            }

            SortedSet<string> input_strings = new SortedSet<string>();
            foreach (int input_state in this.Keys)
            {
                foreach (string input_string in this[input_state].Keys)
                {
                    input_strings.Add(input_string);
                }
            }

            StringBuilder string_builder = new StringBuilder();
            string_builder.Append(">> Constructing the transliteration table:\n\n");

            foreach (string input_string in input_strings)
            {
                string_builder.Append(String.Format("\t[{0}]", input_string));
            }
            string_builder.Append("\n");

            foreach (int input_state in input_states)
            {
                string_builder.Append(String.Format("[{0}]\t", input_state));
                foreach (string input_string in input_strings)
                {
                    TransliterationTableItem table_item;
                    if (this[input_state].TryGetValue(input_string, out table_item))
                    {
                        if (table_item.IsFinal)
                        {
                            string_builder.Append(String.Format("{1},F,{0}\t", table_item.OutputState, table_item.OutputString));
                        }
                        else
                        {
                            string_builder.Append(String.Format("{1},{0}\t", table_item.OutputState, table_item.OutputString));
                        }

                    }
                    else
                    {
                        string_builder.Append("\t");
                    }
                }
                string_builder.Append("\n");
            }
            return string_builder.ToString();
        }
    }

    public class TransliterationAssemblerItem
    {

        private string output_string;
        private bool is_final;

        public TransliterationAssemblerItem()
        {
            this.OutputString = null;
            this.IsFinal = false;
        }

        public TransliterationAssemblerItem(string output_string, bool is_final)
        {
            this.OutputString = output_string;
            this.IsFinal = is_final;
        }

        public string OutputString
        {
            get { return (this.output_string); }
            set { this.output_string = value; }
        }

        public bool IsFinal
        {
            get { return (this.is_final); }
            set { this.is_final = value; }
        }
    }

    public class TransliterationAssembler
    {
        public Stack<TransliterationAssemblerItem> stack;
        public StringBuilder non_finals;

        public TransliterationAssembler()
        {
            this.stack = new Stack<TransliterationAssemblerItem>();
            this.non_finals = new StringBuilder();
        }

        public void ReduceToString(StringBuilder output_string)
        {
            TransliterationAssemblerItem stack_item;
            while (this.stack.Count > 0)
            {
                stack_item = this.stack.Pop();
                if (stack_item.IsFinal)
                {
                    output_string.Append(stack_item.OutputString);
                    this.stack.Clear();
                    output_string.Append(this.non_finals.ToString());
                    this.non_finals.Clear();
                }
                else
                {
                    this.non_finals.Append(stack_item.OutputString);
                }
            }
            if (this.non_finals.Length > 0)
            {
                output_string.Append(ToReversedString(this.non_finals));
                this.non_finals.Clear();
            }
        }
        
        public string ToReversedString(StringBuilder string_builder)
        {
            char[] string_builder_characters = string_builder.ToString().ToCharArray();
            Array.Reverse(string_builder_characters);
            return (new string(string_builder_characters));
        }
    }

    public class Transliterator
    {
        private TransliterationMap map;
        private TransliterationTree tree;
        public TransliterationTable table;
        private TransliterationAssembler assembler;
        private StringBuilder output_string;

        public string color = Constants.default_transliterator_color;
        public string font_family = Constants.default_transliterator_font_family;

        public Transliterator()
        {

            this.map = new TransliterationMap();
            this.tree = new TransliterationTree();
            this.table = new TransliterationTable();
            this.assembler = new TransliterationAssembler();
            this.output_string = new StringBuilder();
        }

        public void Load(string transliterator_id, DataGrid data_grid, TextBox compilation_text_box)
        {
            compilation_text_box.AppendText("--------------------------------------\n");
            compilation_text_box.AppendText("Transliterator: " + transliterator_id + "\n");
            compilation_text_box.AppendText("--------------------------------------\n");

            this.map.ParseDataGrid(data_grid); // Parse the data grid to a transliteration map
            compilation_text_box.AppendText(this.map.ToString());
            compilation_text_box.AppendText("\n");

            this.tree.ParseTransliterationMap(this.map); // Parse the transliteration map to a transliteration tree
            compilation_text_box.AppendText(this.tree.ToString());
            compilation_text_box.AppendText("\n");

            this.table.ParseTransliterationTree(this.tree); // Parse the trasliteration tree to a transliteration table
            compilation_text_box.AppendText(this.table.ToString());
            compilation_text_box.AppendText("\n");
        }

        public string transliterate(string input_string)
        {
            {
                this.output_string.Clear();
                int current_state = this.table.StartState;
                int input_string_index = 0;
                string input_character;
                Dictionary<string, TransliterationTableItem> columns;
                TransliterationTableItem table_item;
                while (input_string_index < input_string.Length)
                {
                    input_character = input_string[input_string_index].ToString();
                    if (table.TryGetValue(current_state, out columns))
                    {
                        if (columns.TryGetValue(input_character, out table_item))
                        {
                            TransliterationAssemblerItem stack_item = new TransliterationAssemblerItem();
                            stack_item.OutputString = table_item.OutputString;
                            stack_item.IsFinal = table_item.IsFinal;
                            this.assembler.stack.Push(stack_item);
                            current_state = table_item.OutputState;
                            input_string_index++;
                        }
                        else
                        {
                            this.assembler.ReduceToString(this.output_string);
                            current_state = this.table.StartState;
                            if (!table[current_state].ContainsKey(input_character))
                            {
                                this.output_string.Append(input_character);
                                input_string_index++;
                            }
                        }
                    }
                    else
                    {
                        this.assembler.ReduceToString(this.output_string);
                        current_state = this.table.StartState;
                        if (!table[current_state].ContainsKey(input_character))
                        {
                            this.output_string.Append(input_character);
                            input_string_index++;
                        }
                    }
                }
                this.assembler.ReduceToString(this.output_string);
                return (this.output_string.ToString());
            }
        }

    }

    public class Transliterators : Dictionary<string, Transliterator>
    {
        public Transliterators() : base()
        {
        }
        public string GetXmlString(TabControl tab_control)
        {
            XmlDocument xml_document = new XmlDocument();
            XmlElement transliterators_node = xml_document.CreateElement("transliterators");
            xml_document.AppendChild(transliterators_node);

            foreach (TabItem tab_item in tab_control.Items)
            {
                TransliterationDataGrid data_grid = (TransliterationDataGrid)tab_item.Content;

                XmlElement transliterator_node = xml_document.CreateElement("transliterator");

                XmlAttribute transliterator_id = xml_document.CreateAttribute("id");
                transliterator_id.Value = data_grid.id;
                transliterator_node.Attributes.Append(transliterator_id);

                XmlAttribute transliterator_color = xml_document.CreateAttribute("color");
                transliterator_color.Value = data_grid.color;
                transliterator_node.Attributes.Append(transliterator_color);

                XmlAttribute transliterator_font_family = xml_document.CreateAttribute("font_family");
                transliterator_font_family.Value = data_grid.font_family;
                transliterator_node.Attributes.Append(transliterator_font_family);

                XmlAttribute transliterator_description = xml_document.CreateAttribute("description");
                transliterator_description.Value = data_grid.description;
                transliterator_node.Attributes.Append(transliterator_description);

                transliterators_node.AppendChild(transliterator_node);

                List<TransliterationDataGridItem> transliteration_items = (List<TransliterationDataGridItem>)data_grid.ItemsSource;
                foreach (TransliterationDataGridItem transliteration_item in transliteration_items)
                {
                    XmlElement map_node = xml_document.CreateElement("map");
                    XmlAttribute map_comment = xml_document.CreateAttribute("comment");
                    map_comment.Value = transliteration_item.Comment;
                    map_node.Attributes.Append(map_comment);
                    transliterator_node.AppendChild(map_node);

                    XmlElement input_node = xml_document.CreateElement("input");
                    input_node.InnerText = transliteration_item.Input;
                    map_node.AppendChild(input_node);

                    XmlElement output_node = xml_document.CreateElement("output");
                    output_node.InnerText = transliteration_item.Output;
                    map_node.AppendChild(output_node);
                }
            }
            return (GetPrettyXMLString(xml_document));
        }
        static string GetPrettyXMLString(XmlDocument xml_document)
        {
            XmlWriterSettings xml_writer_settings = new XmlWriterSettings();
            xml_writer_settings.Encoding = Encoding.UTF8;
            xml_writer_settings.Indent = true;
            xml_writer_settings.NewLineOnAttributes = false;
            using (StringWriterUTF8 string_writer = new StringWriterUTF8())
            {
                using (XmlWriter xml_writer = XmlWriter.Create(string_writer, xml_writer_settings))
                {
                    xml_document.Save(xml_writer);
                }
                string_writer.Flush();
                return (string_writer.ToString());
            }
        }
    }

    public class StringWriterUTF8 : StringWriter
    {
        // Source: Yishai Galatzer, https://stackoverflow.com/questions/25730816/how-to-return-xml-as-utf-8-instead-of-utf-16, 2023-03-04
        public override Encoding Encoding
        {
            get { return (new UTF8Encoding(false)); }
        }
    }
}
