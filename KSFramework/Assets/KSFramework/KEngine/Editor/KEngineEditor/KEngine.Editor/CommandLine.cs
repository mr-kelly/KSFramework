#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: CommandLine.cs
// Date:     2015/12/03
// Author:  Kelly
// Email: 23110388@qq.com
// Github: https://github.com/mr-kelly/KEngine
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.

#endregion

using System.Collections.Generic;

namespace KEngine.Editor
{
    public class CommandArgs
    {
        public Dictionary<string, string> ArgPairs
        {
            get { return mArgPairs; }
        }

        public List<string> Params
        {
            get { return mParams; }
        }

        private List<string> mParams = new List<string>();
        private Dictionary<string, string> mArgPairs = new Dictionary<string, string>();
    }

    public class CommandLine
    {
        public static CommandArgs Parse(string[] args)
        {
            char[] kEqual = new char[] {'='};
            char[] kArgStart = new char[] {'-', '\\'};
            CommandArgs ca = new CommandArgs();

            int ii = -1;
            string token = NextToken(args, ref ii);

            while (token != null)
            {
                if (IsArg(token))
                {
                    string arg = token.TrimStart(kArgStart).TrimEnd(kEqual);
                    string value = null;

                    if (arg.Contains("="))
                    {
                        string[] r = arg.Split(kEqual, 2);

                        if (r.Length == 2 && r[1] != string.Empty)
                        {
                            arg = r[0];
                            value = r[1];
                        }
                    }

                    while (value == null)
                    {
                        if (ii > args.Length)
                            break;

                        string next = NextToken(args, ref ii);
                        if (next != null)
                        {
                            if (IsArg(next))
                            {
                                ii--;
                                value = "true";
                            }
                            else if (next != "=")
                            {
                                value = next.TrimStart(kEqual);
                            }
                        }
                    }
                    ca.ArgPairs.Add(arg, value);
                }
                else if (token != string.Empty)
                {
                    ca.Params.Add(token);
                }

                token = NextToken(args, ref ii);
            }
            return ca;
        }

        private static bool IsArg(string arg)
        {
            return (arg.StartsWith("-") || arg.StartsWith("\\"));
        }

        private static string NextToken(string[] args, ref int ii)
        {
            ii++;
            while (ii < args.Length)
            {
                string cur = args[ii].Trim();
                if (cur != string.Empty)
                {
                    return cur;
                }
                ii++;
            }
            return null;
        }
    }
}