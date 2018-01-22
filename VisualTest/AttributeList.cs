using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualTest
{
    public class AttributeList
    {
        public ArrayList myObjectList;
        
        public string myName { get; set; }
        double max;
        double min;
        public List<double> myTime;

        public AttributeList(string name)
        {
            myObjectList = new ArrayList();
            myTime = new List<double>();
            this.myName=name;
        }
        public void setMax()
        {
             max=-9999999999;
            try
            {
                foreach (double number in myObjectList)
                {
                    if (number > max)
                    {
                        max = number;
                    }
                }
            }
            catch
            {

            }
            
            
        }
        public void setMin()
        {
            min=999999999999;
            try
            {
                foreach (double number in myObjectList)
                {
                    if (number < min)
                    {
                        min = number;
                    }
                }
            }
            catch
            {

            }
            
            
        }
        public double getMax()
        {
            return max;
        }
        public double getMin()
        {
            return min;
        }
    }
}
