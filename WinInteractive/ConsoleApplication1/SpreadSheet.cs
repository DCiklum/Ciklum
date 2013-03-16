using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadSheet
{
    class SpreadSheet
    {
        private static Dictionary<string, string> cell;
        
        //<summary>
        // Creates dictionary collection data from Spreadsheet file
        //</summary>
        public static void Load(string filepath)
        {
            cell = new Dictionary<string, string>();
            string line;
            int col = (int)'@';
            int row = 0;
            StreamReader file;

            try
            {
                file = new StreamReader(filepath);
                while ((line = file.ReadLine()) != null)
                {
                    //ignore first line
                    if (row == 0)
                    {
                        ++row;
                        continue;
                    }

                    while (line.IndexOf("\t") > -1)
                    {
                        cell.Add(((char)++col).ToString() + row, line.Remove(line.IndexOf("\t")));
                        line = line.Remove(0, line.IndexOf("\t") + 1);
                    }
                    cell.Add(((char)++col).ToString() + row, line);

                    col = (int)'@';
                    ++row;
                }

                file.Close();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Файл 'c:\\input.txt' не найден");
            }
        }

        //<summary>
        // Displays data to Console
        //</summary>
        public static void ShowData()
        {
            foreach (object idic in cell)
            {
                System.Console.WriteLine(idic.ToString());
            }

            Console.ReadLine();
        }

        //<summary>
        // Calculates each cell in Dictionary collection
        //</summary>
        public static void Batch()
        {
            string[] cells = new string[cell.Count]; // array of cells Keys
            int i = 0;
            string resvalue;

            foreach (KeyValuePair<string, string> iDic in cell)
            {
                cells[i] = iDic.Key;
                i++;
            }            

            for (int j = 0; j < cell.Count; j++)
            {
                resvalue = CalcCell(cells[j]);
            }

        }

        //<summary>
        // Writes data to Spreadsheet file
        //</summary>
        public static void Save(string filepath)
        {
            StringBuilder linedata = new StringBuilder("");
            StreamWriter file = new StreamWriter(filepath);
            foreach (KeyValuePair<string, string> iDic in cell)
            {
                if ((linedata.Length > 0) & (iDic.Key[0] == (char)'A'))
                {
                    file.WriteLine(linedata.Remove(linedata.Length - 1, 1));
                    linedata = new StringBuilder(""); //linedata.AppendLine();
                }
                linedata.AppendFormat(iDic.Value + "\t");
            }
            file.WriteLine(linedata.Remove(linedata.Length - 1, 1));

            file.Close();
        }

        private static string CalcCell(string key)
        {
            string result;

            result = cell[key];
            cell[key] = "#Calculating"; // mark as #calculating to catch circular reference

            // cell has empty value
            if (string.IsNullOrEmpty(result))
            {
                cell[key] = "0";
            }

            // cell has some error value
            else if (result.Substring(0, 1) == "#")
            {
                if (result == "#Calculating")
                {
                    cell[key] = "#circular reference";
                }
                else
                {
                    cell[key] = result;
                }
            }

            // cell has lable value
            else if (result.Substring(0, 1) == "'")
            {
                cell[key] = result.Substring(1);
            }

            // cell has arithmetic value
            else if (result.Substring(0, 1) == "=")
            {
                cell[key] = CalculateArithmetic(result.Substring(1));
            }

            // cell has number value, label or thomething else...
            else
            {
                cell[key] = result;
            }

            return cell[key];
        }

        private static string CalculateArithmetic(string expression)
        {
            int result;
            string leftOp;
            string rightOp;

            if (int.TryParse(expression, out result))
            {
                return expression;
            }

            else if (expression.IndexOf("-") > 0)
            {
                leftOp = CalculateArithmetic(LeftOperand("-", expression));
                rightOp = CalculateArithmetic(RightOperand("-", expression));

                if (leftOp.Substring(0, 1) == "#")
                {
                    return leftOp;
                }
                else if (rightOp.Substring(0, 1) == "#")
                {
                    return rightOp;
                }
                else
                {
                    return (int.Parse(leftOp) - int.Parse(rightOp)).ToString();
                }
            }

            else if (expression.IndexOf("+") > 0)
            {
                leftOp = CalculateArithmetic(LeftOperand("+", expression));
                rightOp = CalculateArithmetic(RightOperand("+", expression));

                if (leftOp.Substring(0, 1) == "#")
                {
                    return leftOp;
                }
                else if (rightOp.Substring(0, 1) == "#")
                {
                    return rightOp;
                }
                else
                {
                    return (int.Parse(leftOp) + int.Parse(rightOp)).ToString();
                }
            }

            else if (expression.IndexOf("/") > 0)
            {
                leftOp = CalculateArithmetic(LeftOperand("/", expression));
                rightOp = CalculateArithmetic(RightOperand("/", expression));

                if (leftOp.Substring(0, 1) == "#")
                {
                    return leftOp;
                }
                else if (rightOp.Substring(0, 1) == "#")
                {
                    return rightOp;
                }
                else
                {
                    return (int.Parse(leftOp) / int.Parse(rightOp)).ToString();
                }
            }

            else if (expression.IndexOf("*") > 0)
            {
                leftOp = CalculateArithmetic(LeftOperand("*", expression));
                rightOp = CalculateArithmetic(RightOperand("*", expression));

                if (leftOp.Substring(0, 1) == "#")
                {
                    return leftOp;
                }
                else if (rightOp.Substring(0, 1) == "#")
                {
                    return rightOp;
                }
                else
                {
                    return (int.Parse(leftOp) * int.Parse(rightOp)).ToString();
                }
            }

            else if (cell.ContainsKey(expression))
            {
                return CalcCell(expression);
            }

            else
            {
                return "#error value";
            }
        }

        private static string RightOperand(string op, string expression)
        {
            return expression.Substring(expression.IndexOf(op) + 1);
        }

        private static string LeftOperand(string op, string expression)
        {
            return expression.Substring(0, expression.IndexOf(op));
        }
    }
}
