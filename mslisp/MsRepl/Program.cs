﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using MsLisp;
using MsLisp.Lexical;
using MsLisp.Datums;


namespace MsRepl
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser();
            string expr = "";


            // add C-c keyboard event listener
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPress);

            // first load std.lisp
            var lex = new Lexer(File.ReadAllText("std.lisp"));
            Evaluator.Eval(parser.Parse(lex.Tokenize()));

            // then load repl.lisp
            lex = new Lexer(File.ReadAllText("repl.lisp"));
            Evaluator.Eval(parser.Parse(lex.Tokenize()));

            // then load test.lisp
            lex = new Lexer(File.ReadAllText("test.lisp"));
            Evaluator.Eval(parser.Parse(lex.Tokenize()));

            Evaluator.environment.Add("*prompt*", new Atom("mslisp"));

            // repl
            while (true)
            {
                try
                {
                    IDatum prompt = Evaluator.environment["*prompt*"];
                    Console.Write(string.Format("{0}> ", (string)prompt.Value));

                    expr += Console.ReadLine();

                    // todo: need a better way to count parenthesis
                    var open = expr.Count(c => c == '(');
                    var close = expr.Count(c => c == ')');

                    if (open != close)
                        continue;


                    // have lexer count parens?
                    // lexer should count line numbers.
                    var lexer = new Lexer(expr);
                    var tokens = parser.Parse(lexer.Tokenize());

                    // eval and print
                    tokens.ForEach((list) =>
                    {
                        IDatum eval = Evaluator.Eval(list, Evaluator.environment);
                        Console.WriteLine(eval.ToString());
                    });


                    // cleanup
                    expr = "";
                }
                catch (MsLisp.ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                    expr = "";
                }
                catch (SyntaxException ex)
                {
                    Console.WriteLine(ex.Message);
                    expr = "";
                }
                catch (TypeException ex)
                {
                    Console.WriteLine(ex.Message);
                    expr = "";
                }
                catch (Exception ex)
                {
                    ReplError(ex);
                    expr = "";
                }
            }
        }


        static void CancelKeyPress(Object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Ctrl-C pressed.");
            Console.WriteLine("Exiting MsLisp.");
        }

        static void ReplError(Exception ex)
        {
            var trace = new StackTrace(ex);
            for (int i = 0; i < trace.FrameCount; i++)
            {
                var frame = trace.GetFrame(i);
                Console.WriteLine("Line {0}: {1}", frame.GetFileLineNumber(), frame.GetMethod());
            }
        }
    }
}
