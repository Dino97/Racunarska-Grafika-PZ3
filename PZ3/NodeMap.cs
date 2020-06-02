using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;

namespace PZ3
{
    class NodeMap
    {
        public List<PowerNode> Nodes { get; set; }
        public List<PowerLine> Lines { get; set; }

        public Dictionary<long, PowerNode> IdToNodeDictionary { get; set; }



        public static NodeMap LoadFromXML(string path, Predicate<PowerNode> filterFunction)
        {
            NodeMap nodeMap = new NodeMap();
            nodeMap.Nodes = new List<PowerNode>(64);
            nodeMap.Lines = new List<PowerLine>(64);
            nodeMap.IdToNodeDictionary = new Dictionary<long, PowerNode>(64);

            XmlDocument xml = new XmlDocument();
            XmlNodeList xmlNodes;
            xml.Load(path);

            // Load substations
            xmlNodes = xml.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode xmlNode in xmlNodes)
            {
                SubstationNode node = new SubstationNode();
                node.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                node.Name = xmlNode.SelectSingleNode("Name").InnerText;

                double x = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                double y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);
                double lat, lon;

                MathUtility.ToLatLon(x, y, 34, out lat, out lon);

                node.X = lon;
                node.Y = lat;

                //if (lon < xMin || lon > xMax || lat < yMin || lat > yMax)
                if (filterFunction(node) == false)
                    continue;

                nodeMap.Nodes.Add(node);
                nodeMap.IdToNodeDictionary.Add(node.Id, node);
            }

            // Load nodes
            xmlNodes = xml.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode xmlNode in xmlNodes)
            {
                NodeNode node = new NodeNode();
                node.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                node.Name = xmlNode.SelectSingleNode("Name").InnerText;

                double x = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                double y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);
                double lat, lon;

                MathUtility.ToLatLon(x, y, 34, out lat, out lon);

                node.X = lon;
                node.Y = lat;

                if (filterFunction(node) == false)
                    continue;

                nodeMap.Nodes.Add(node);
                nodeMap.IdToNodeDictionary.Add(node.Id, node);
            }

            // Load switches
            xmlNodes = xml.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode xmlNode in xmlNodes)
            {
                SwitchNode node = new SwitchNode();
                node.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                node.Name = xmlNode.SelectSingleNode("Name").InnerText;
                node.Status = xmlNode.SelectSingleNode("Status").InnerText.Equals("Open");

                double x = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                double y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);
                double lat, lon;

                MathUtility.ToLatLon(x, y, 34, out lat, out lon);

                node.X = lon;
                node.Y = lat;

                if (filterFunction(node) == false)
                    continue;

                nodeMap.Nodes.Add(node);
                nodeMap.IdToNodeDictionary.Add(node.Id, node);
            }

            // Load lines
            xmlNodes = xml.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode xmlNode in xmlNodes)
            {
                PowerLine line = new PowerLine();
                line.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                line.Name = xmlNode.SelectSingleNode("Name").InnerText;
                line.IsUnderground = xmlNode.SelectSingleNode("IsUnderground").InnerText.Equals("true");
                line.R = float.Parse(xmlNode.SelectSingleNode("R").InnerText);
                line.ConductorMaterial = xmlNode.SelectSingleNode("ConductorMaterial").InnerText;
                line.LineType = xmlNode.SelectSingleNode("LineType").InnerText;
                line.ThermalConstantHeat = long.Parse(xmlNode.SelectSingleNode("ThermalConstantHeat").InnerText);
                line.FirstEnd = long.Parse(xmlNode.SelectSingleNode("FirstEnd").InnerText);
                line.SecondEnd = long.Parse(xmlNode.SelectSingleNode("SecondEnd").InnerText);
                line.Vertices = new List<Point>();


                foreach (XmlNode vertexNode in xmlNode.SelectNodes("Vertices/Point"))
                {
                    Point p = new Point();
                    
                    double x = double.Parse(vertexNode.SelectSingleNode("X").InnerText);
                    double y = double.Parse(vertexNode.SelectSingleNode("Y").InnerText);

                    double lat, lon;
                    MathUtility.ToLatLon(x, y, 34, out lat, out lon);

                    p.X = lon;
                    p.Y = lat;

                    line.Vertices.Add(p);
                }

                if (!nodeMap.IdToNodeDictionary.ContainsKey(line.FirstEnd) || !nodeMap.IdToNodeDictionary.ContainsKey(line.SecondEnd))
                    continue;

                nodeMap.IdToNodeDictionary[line.FirstEnd].ConnectionCount++;
                nodeMap.IdToNodeDictionary[line.SecondEnd].ConnectionCount++;
                nodeMap.Lines.Add(line);
            }

            return nodeMap;
        }
    }
}