using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadSheet
{
    class Program
    {

        static void Main(string[] args)
        {
            // reading data from file
            SpreadSheet.Load("c:\\input.txt");
            
            // debug
            //SpreadSheet.ShowData();

            // some calculating
            SpreadSheet.Batch();

            // writing data to file
            SpreadSheet.Save("c:\\output.txt");
        }
    }
}
