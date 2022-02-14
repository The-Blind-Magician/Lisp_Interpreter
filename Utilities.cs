﻿using System;
using System.IO;

namespace Lisp_Interpreter
{
    class Utilities
    {
        private string[] fileStream;
        private int numberOfLines = 0;
        private int lineNumber = 0;
        private int wordNumber = 0;

        public Utilities(string filePath)
        {
            Init_FileStream(filePath);
        }

        void Init_FileStream(string filePath)
        {
            fileStream = File.ReadAllLines(filePath);
        }
        public string Read_Next_Line_Of_File()
        {
            try
            {
                wordNumber = 0;
                return fileStream[lineNumber++];
            }
            catch(IndexOutOfRangeException e)
            {
                lineNumber = 0;
                //Console.Error.WriteLine(e.Message);
                //Console.Error.WriteLine("Index out of range. Returning first element.\nlineNumber set to: " + lineNumber);
                return "";
            }
        }
        public string Read_Next_Word_From_Line(string line)
        {
            try
            { 
                return line.Split(" ")[wordNumber++];
            }
            catch (IndexOutOfRangeException e)
            {
                wordNumber = 0;
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine("Index out of range. Returning first element.\nwordNumber set to: " + wordNumber);
                return line.Split(" ")[wordNumber++];
            }
        }
        public string Get_Line_At_Index(int i)
        {
            try
            {
                return fileStream[i];
            }
            catch (IndexOutOfRangeException e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine("Index out of range. Returning first element.");
                return fileStream[0];
            }
        }
        public int Get_Number_Of_Lines_In_File()
        {
            return numberOfLines;
        }
        public int Get_Current_Line_Number()
        {
            return lineNumber;
        }
    }
}