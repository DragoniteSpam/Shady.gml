﻿using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static Shady.Parser;

namespace Shady
{
    internal class Program
    {
        static Parser parser = new Parser();
        static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

        static void Main(string[] args)
        {
            string projectPath = @"C:\Projects\VisualStudio\Shady.gml\Example";
            //string projectPath = @"C:\Users\MusNik\Documents\GameMakerStudio2\CloseYourEyes";
            string shadersPath = projectPath + @"\shaders";

            string[] shaderFiles = Directory.GetFiles(projectPath, "*.fsh", SearchOption.AllDirectories);

            foreach (string shaderFile in shaderFiles)
            {
                string shaderName = Path.GetFileNameWithoutExtension(shaderFile);
                Shader shader = ParseShader(shaderFile);
                shaders.Add(shaderName, shader);
            }

            const bool forceNonParallel = true;
            var options = new ParallelOptions { MaxDegreeOfParallelism = forceNonParallel ? 1 : -1 };

            Parallel.ForEach(shaders, options, ParseTokens);
        }

        private static Shader ParseShader(string path)
        {
            Shader shader = new Shader(Path.GetFileName(path));

            IEnumerable<string> lines = File.ReadLines(path);
            int i = -1;

            foreach (string line in lines)
            {
                i++;
                shader.AddLine(i, line);
            }

            return shader;
        }

        private static void ParseTokens(KeyValuePair<string, Shader> shaderKeyValue)
        {
            Shader shader = shaderKeyValue.Value;
            int level = 0;

            LinkedListNode<ShaderLine>? currentNode = shader.Lines.First;
            while (currentNode != null)
            {
                ShaderLine shaderLine = currentNode.Value;
                string line = Regex.Replace(shaderLine.Line, @"^\s+", "");  /// remove leading whitespaces
                string remainingLine;

                // Parse Shady tokens
                Token? pragma = parser.Match(line, TokenType.Shady);
                if (pragma != null)
                {
                    //Console.WriteLine(pragma.Value);
                    remainingLine = Regex.Replace(pragma.RemainingInput, @"\s", ""); /// remove all whitespaces

                    TokenType previousToken = TokenType.Shady;
                    List<TokenType> expectedTokens = new List<TokenType>() { TokenType.Import, TokenType.Inline, TokenType.Variant };
                    List<Token> lineTokens = new List<Token>();

                    while (expectedTokens.Count != 0)
                    {
                        try
                        {
                            Token token = parser.Expect(remainingLine, expectedTokens, previousToken);
                            //Console.WriteLine($"{token.Value}");

                            remainingLine = token.RemainingInput;
                            previousToken = token.TokenType;
                            expectedTokens.Clear();

                            switch (token.TokenType)
                            {
                                case TokenType.Import:
                                case TokenType.Inline:
                                case TokenType.Variant:
                                    expectedTokens.Add(TokenType.OpenParen);
                                    break;

                                case TokenType.OpenParen:
                                    expectedTokens.Add(TokenType.Identifier);
                                    break;

                                case TokenType.Identifier:
                                    expectedTokens.Add(TokenType.Dot);
                                    expectedTokens.Add(TokenType.CloseParen);
                                    break;

                                case TokenType.Dot:
                                    expectedTokens.Add(TokenType.Identifier);
                                    expectedTokens.Add(TokenType.CloseParen);
                                    break;

                                default:

                                    break;
                            }

                            lineTokens.Add(token);
                        }
                        catch (UnexpectedExpression e)
                        {
                            expectedTokens.Clear();
                            Console.WriteLine($"[Shady] Syntax Error {Path.GetFileName(shaderLine.ShaderName)}, line {shaderLine.LineIndex + 1}: {e.Message}");
                        }
                    }

                    //Console.Write($"{index} ");
                    lineTokens.ForEach(token => Console.Write($"[{token.Value}]"));
                    Console.WriteLine();

                    if (lineTokens.Count > 0)
                    {
                        switch (lineTokens[0].TokenType)
                        {
                            case TokenType.Import:
                                int indexShader = 2;
                                int indexIdentifier = 4;

                                break;
                        }
                    }
                }

                Console.WriteLine($"{shaderLine.ShaderName}.{level}: {shaderLine.Line}");
                currentNode = currentNode.Next;
            }
        }
    }
}