using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace ArcCreate.Utility
{
    public static class YamlExtractor
    {
        public static void ExtractTo(Dictionary<string, string> dict, TextReader input)
        {
            var yaml = new YamlStream();
            yaml.Load(input);
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            ExtractTo(dict, mapping, "");
        }

        public static void ExtractListsTo(Dictionary<string, List<string>> dict, TextReader input)
        {
            var yaml = new YamlStream();
            yaml.Load(input);
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            ExtractListsTo(dict, mapping, "");
        }

        public static void ExtractTo(Dictionary<string, string> dict, YamlMappingNode node, string key)
        {
            foreach (KeyValuePair<YamlNode, YamlNode> child in node.Children)
            {
                string nodeKey = (child.Key as YamlScalarNode).Value;
                string newKey = string.IsNullOrEmpty(key) ? nodeKey : $"{key}.{nodeKey}";

                YamlNode value = child.Value;
                if (value is YamlScalarNode scalar)
                {
                    string leaf = scalar.Value;
                    dict.Add(newKey, leaf);
                }
                else
                {
                    ExtractTo(dict, value as YamlMappingNode, newKey);
                }
            }
        }

        public static void ExtractListsTo(Dictionary<string, List<string>> dict, YamlMappingNode node, string key)
        {
            foreach (KeyValuePair<YamlNode, YamlNode> child in node.Children)
            {
                string nodeKey = (child.Key as YamlScalarNode).Value;
                string newKey = string.IsNullOrEmpty(key) ? nodeKey : $"{key}.{nodeKey}";

                YamlNode value = child.Value;
                if (value is YamlSequenceNode sequence)
                {
                    List<string> list = new List<string>();
                    foreach (var children in sequence)
                    {
                        if (children is YamlScalarNode scalar)
                        {
                            list.Add(scalar.Value);
                        }
                    }

                    dict.Add(newKey, list);
                }
                else if (value is YamlScalarNode scalar)
                {
                    List<string> list = new List<string>() { scalar.Value };
                    dict.Add(newKey, list);
                }
                else if (value is YamlMappingNode map)
                {
                    ExtractListsTo(dict, map, newKey);
                }
            }
        }
    }
}