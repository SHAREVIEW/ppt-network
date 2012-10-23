using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI
{
    class ListBoxItem
    {
        public string mText;
        public string mKey;
        public string mIP;
        public string mPath;
        public string mIntroduction;
        public ListBoxItem(string pKey,string pText,string pIP,string pPath,string pIntroduction)
        {
            mText = pText;
            mKey = pKey;
            mIP = pIP;
            mPath = pPath;
            mIntroduction = pIntroduction;
        }
        public override string ToString()
        {
           return mText;
        }

    }
}
