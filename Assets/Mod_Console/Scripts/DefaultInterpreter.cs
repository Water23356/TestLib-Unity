using CommandParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mod_Console
{
    public class DefaultInterpreter : Interpreter
    {
        public override Data Effectuate(string commandName, Data[] parameters)
        {
            Data data = EffectuateSuper(commandName, parameters);
            if(data.isError())
            {
                switch (commandName)
                {
                    case "clear":
                        ConsolePanel.Instance.Clear();
                        return Data.Empty;
                    case "Erinbone":
                        ConsolePanel.Instance.Print("This is the author Erinbone. This version is currently in the testing phase. Welcome to participate in the testing and report errors to us");
                        return Data.Empty;
                    case "exit":
                        Application.Quit();
                        return Data.Empty;
                    case "help":
                        ConsolePanel.Instance.Print("----------help----------");
                        ConsolePanel.Instance.Print("This is the help page, you can use help [page] to switch the current page of the help page");
                        int page = 1;
                        if(parameters.Length == 0)
                        {

                        }
                        else if (parameters[0].Type == DataType.Integer)
                        {
                            page = (int)parameters[0].Value;
                            if(page < 1 || page > 2)
                            {
                                ConsolePanel.Instance.PrintError("The specified help page does not exist: Page[1-1]");
                                page = 1;
                            }
                        }
                        else
                        {
                            return data;
                        }
                        switch(page)
                        {
                            case 1:
                                ConsolePanel.Instance.Print("clear      *clear console message");
                                ConsolePanel.Instance.Print("help [page]     *get help");
                                break;
                            case 2:
                                ConsolePanel.Instance.Print("Oh! Construction is underway here");
                                break;
                        }
                        ConsolePanel.Instance.Print($"-----------[{page}/1]-----------");
                        ConsolePanel.Instance.Print("----------help----------");
                        return Data.Empty;
                    default:
                        return data;
                }
            }
            return data;
        }

        public virtual Data EffectuateSuper(string commandName, Data[] parameters)
        {
            return Data.Error;
        }
    }
}
