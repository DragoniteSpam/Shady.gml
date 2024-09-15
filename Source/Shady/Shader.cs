﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shady
{
    internal class Shader
    {
        public const string FullRegion = "full";
        public string Name { get; }
        public LinkedList<ShaderLine> Lines { get; }
        private readonly Dictionary<string, LinkedList<ShaderLine>> _regions;

        public Shader(string name)
        {
            Name = name;
            Lines = new LinkedList<ShaderLine>();

            _regions = new Dictionary<string, LinkedList<ShaderLine>>();
        }

        public void AddLine(int lineIndex, string line)
        {
            Lines.AddLast(new ShaderLine(Name, lineIndex, line));
        }

        public void AddToRegion(string regionName, ShaderLine shaderLine)
        {
            LinkedList<ShaderLine> region;

            if (!_regions.ContainsKey(regionName))
            {
                region = new LinkedList<ShaderLine>();
                _regions.Add(regionName, region);
            } else
            {
                region = _regions[regionName];
            }

            region.AddLast(shaderLine);
        }

        public LinkedList<ShaderLine>? GetRegion(string regionName)
        {
            if (_regions.ContainsKey(regionName))
            {
                return _regions[regionName];
            }
            else
            {
                return null;
            }
        }

        public void DebugConsole()
        {
            foreach (var line in Lines)
            {
                Console.WriteLine(line);
            }
        }
    }

    internal class ShaderLine
    {
        public int LineIndex { get; }
        public string Line { get; }
        public string ShaderName { get; }
        public ShaderLine(string shaderName, int lineIndex, string line)
        {
            LineIndex = lineIndex;
            Line = line;
            ShaderName = shaderName;
        }
    }
}
