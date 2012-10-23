using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI
{
    class ComboBoxItem
    {
        public string mText;

        public string mvalue;

        public ComboBoxItem(string pText, string pValue)
        {
            mText = pText;
            mvalue = pValue;
        }

        public override string ToString()
        {
            return mText;
        }
    }
}
