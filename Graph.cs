using System.Collections.Generic; // List<>, ...
using System.Xml.Linq; // XDocument, XElement, XAttribute, ...

namespace Graph
{
    public static class Defaults
    {
        public static float default_node_width = (float)50.0;
        public static float default_node_height = (float)50.0;
        public static string default_node_shape = "ellipse";

        public static string default_node_fill_color = "#AAAAAA";
        public static string non_final_node_fill_color = "#FFCCCC";
        public static string final_node_fill_color = "#CCCCFF";
        public static string start_node_fill_color = "#FFFFFF";
        public static string default_node_boder_type = "line";
        public static float default_node_border_width = (float)1.0;
        public static string default_node_border_color = "#000000";
        public static string start_node_border_color = "#FFFFFF";

        public static bool default_node_label_visible = true;
        public static string default_node_label_text_color = "#000000";
        public static string default_node_label_font_family = "Dialog";
        public static string default_node_label_font_style = "plain";
        public static int default_node_label_font_size = 12;

        public static string default_edge_type = "PolyLineEdge";
        public static string arc_edge_type = "ArcEdge";
        public static string loop_edge_type = "PolyLineEdge";

        public static string default_edge_line_type = "line";
        public static string default_edge_line_color = "#AAAAAA";
        public static float default_edge_line_width = (float)1.0;
        public static string default_edge_arrow_source = "none";
        public static string default_edge_arrow_target = "standard";

        public static bool default_edge_label_visible = true;
        public static string default_edge_label_text_color = "#000000";
        public static string default_edge_label_font_family = "Dialog";
        public static string default_edge_label_font_style = "bold";
        public static int default_edge_label_font_size = 16;

        public static string xml_preserve_whitespace = "preserve";
    }

    public class Node
    {
        public string id;
        public string label;

        public float width;
        public float height;
        public string shape;

        public string fill_color;
        public string border_color;
        public string border_type;
        public float border_width;

        public bool label_visible;
        public string label_text_color;
        public string label_font_family;
        public string label_font_style;
        public int label_font_size;

        public string xml_preserve_whitespace;

        public Node(string id, string label, string type)
        {
            this.id = id;
            this.label = label;

            this.width = Defaults.default_node_width;
            this.height = Defaults.default_node_height;
            this.shape = Defaults.default_node_shape;

            switch (type)
            {
                case "start":
                    this.fill_color = Defaults.start_node_fill_color;
                    this.border_color = Defaults.start_node_border_color;
                    this.border_type = Defaults.default_node_boder_type;
                    this.border_width = Defaults.default_node_border_width;
                    break;
                case "non-final":
                    this.fill_color = Defaults.non_final_node_fill_color;
                    this.border_color = Defaults.default_node_border_color;
                    this.border_type = Defaults.default_node_boder_type;
                    this.border_width = Defaults.default_node_border_width;
                    break;
                case "final":
                    this.fill_color = Defaults.final_node_fill_color;
                    this.border_color = Defaults.default_node_border_color;
                    this.border_type = Defaults.default_node_boder_type;
                    this.border_width = Defaults.default_node_border_width;
                    break;
                default:
                    this.fill_color = Defaults.default_node_fill_color;
                    this.border_color = Defaults.default_node_border_color;
                    this.border_type = Defaults.default_node_boder_type;
                    this.border_width = Defaults.default_node_border_width;
                    break;
            }

            this.label_visible = Defaults.default_node_label_visible;
            this.label_text_color = Defaults.default_node_label_text_color;
            this.label_font_family = Defaults.default_node_label_font_family;
            this.label_font_style = Defaults.default_node_label_font_style;
            this.label_font_size = Defaults.default_node_label_font_size;

            this.xml_preserve_whitespace = Defaults.xml_preserve_whitespace;
        }
    }

    public class Edge
    {
        public string id;
        public string label;
        public string source;
        public string target;
        public string type;

        public string line_type;
        public string line_color;
        public float line_width;
        public string arrow_source;
        public string arrow_target;

        public bool label_visible;
        public string label_text_color;
        public string label_font_family;
        public string label_font_style;
        public int label_font_size;

        public string xml_preserve_whitespace;

        public Edge(string id, string label, string source, string target, string type)
        {
            this.id = id;
            this.label = label;
            this.source = source;
            this.target = target;

            switch (type)
            {
                case "arc":
                    this.type = Defaults.arc_edge_type;
                    break;
                case "loop":
                    this.type = Defaults.loop_edge_type;
                    break;
                default:
                    this.type = Defaults.default_edge_type;
                    break;
            }

            this.line_type = Defaults.default_edge_line_type;
            this.line_color = Defaults.default_edge_line_color;
            this.line_width = Defaults.default_edge_line_width;
            this.arrow_source = Defaults.default_edge_arrow_source;
            this.arrow_target = Defaults.default_edge_arrow_target;

            this.label_visible = Defaults.default_edge_label_visible;
            this.label_text_color = Defaults.default_edge_label_text_color;
            this.label_font_family = Defaults.default_edge_label_font_family;
            this.label_font_style = Defaults.default_edge_label_font_style;
            this.label_font_size = Defaults.default_edge_label_font_size;

            this.xml_preserve_whitespace = Defaults.xml_preserve_whitespace;
        }
    }

    public class Graph
    {
        public List<Node> nodes;
        public List<Edge> edges;

        public Graph()
        {
            this.nodes = new List<Node>();
            this.edges = new List<Edge>();
        }

        public void AddNode(string id, string label, string type)
        {
            this.nodes.Add(new Node(id, label, type));
        }

        public void AddEdge(string id, string label, string source, string target, string type)
        {
            this.edges.Add(new Edge(id, label, source, target, type));
        }

        public Node GetNodeByID (string id)
        {
            Node node = null;
            foreach (Node n in nodes)
            {
                if(n.id == id)
                {
                    node = n;
                    break;
                }
                else
                {
                }
            }
            return (node);
        }

        public void SaveAsGraphML(string path)
        {
            XDeclaration xml_declaration = new XDeclaration("1.0", "UTF-8", "no");
            XDocument xml_document = new XDocument(xml_declaration);

            XComment xml_comment = new XComment("Generated from the pseudo-transducer data by Luka Tolić (Tolitch), 2023");
            xml_document.Add(xml_comment);

            XNamespace xmlns = "http://graphml.graphdrawing.org/xmlns";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace y = "http://www.yworks.com/xml/graphml";

            XElement graphml_node =
                new XElement(xmlns + "graphml",
                    new XAttribute("xmlns", "http://graphml.graphdrawing.org/xmlns"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XAttribute(XNamespace.Xmlns + "y", "http://www.yworks.com/xml/graphml"),
                    new XAttribute(xsi + "schemaLocation", "http://graphml.graphdrawing.org/xmlns http://www.yworks.com/xml/schema/graphml/1.1/ygraphml.xsd")
                );
            xml_document.Add(graphml_node);

            graphml_node.Add(
                new XElement(xmlns + "key",
                    new XAttribute("for", "node"),
                    new XAttribute("id", "d1"),
                    new XAttribute("yfiles.type", "nodegraphics")
                    )
                );
            graphml_node.Add(
                new XElement(xmlns + "key",
                    new XAttribute("for", "edge"),
                    new XAttribute("id", "d2"),
                    new XAttribute("yfiles.type", "edgegraphics")
                    )
                );


            foreach (Node node in this.nodes)
            {
                XElement xml_node = new XElement(xmlns + "node",
                    new XAttribute("id", node.id),
                        new XElement(xmlns + "data",
                            new XAttribute("key", "d1"),
                            new XElement(y + "ShapeNode",
                                new XElement(y + "Geometry",
                                    new XAttribute("height", node.height),
                                    new XAttribute("width", node.width)
                                ),
                                new XElement(y + "Shape",
                                    new XAttribute("type", node.shape)
                                ),
                                new XElement(y + "Fill",
                                    new XAttribute("color", node.fill_color)
                                ),
                                new XElement(y + "BorderStyle",
                                    new XAttribute("color", node.border_color),
                                    new XAttribute("type", node.border_type),
                                    new XAttribute("width", node.border_width)
                                ),
                                new XElement(y + "NodeLabel",
                                    new XAttribute("visible", node.label_visible),
                                    new XAttribute("textColor", node.label_text_color),
                                    new XAttribute("fontFamily", node.label_font_family),
                                    new XAttribute("fontSize", node.label_font_size),
                                    new XAttribute("fontStyle", node.label_font_style),
                                    new XAttribute(XNamespace.Xml + "space", node.xml_preserve_whitespace),
                                    new XText(node.label)
                                    )
                            )
                        )
                    );
                graphml_node.Add(xml_node);
            }

            foreach (Edge edge in this.edges)
            {
                XElement xml_edge = new XElement(xmlns + "edge",
                    new XAttribute("id", edge.id),
                    new XAttribute("source", edge.source),
                    new XAttribute("target", edge.target),
                    new XElement(xmlns + "data",
                    new XAttribute("key", "d2"),
                    new XElement(y + edge.type,
                        new XElement(y + "LineStyle",
                            new XAttribute("type", edge.line_type),
                            new XAttribute("color", edge.line_color),
                            new XAttribute("width", edge.line_width)
                        ),
                        new XElement(y + "Arrows",
                            new XAttribute("source", edge.arrow_source),
                            new XAttribute("target", edge.arrow_target)
                        ),
                        new XElement(y + "EdgeLabel",
                            new XAttribute("visible", edge.label_visible),
                            new XAttribute("textColor", edge.label_text_color),
                            new XAttribute("fontFamily", edge.label_font_family),
                            new XAttribute("fontSize", edge.label_font_size),
                            new XAttribute("fontStyle", edge.label_font_style),
                            new XAttribute(XNamespace.Xml + "space", edge.xml_preserve_whitespace),
                            new XText(edge.label)
                            )
                        )
                    )
                );
                graphml_node.Add(xml_edge);
            }

            xml_document.Save(path);
        }
    }
}
