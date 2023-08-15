using Grasshopper;
using Grasshopper.Kernel;
using Headless.Properties;
using System;
using System.Drawing;

namespace Headless
{
    public class HeadlessInfo : GH_AssemblyInfo
    {
        public override string Name => "Headless";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.libIcon;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "This liberary aims to provide plugins that are targetet for use with rhino compute";

        public override Guid Id => new Guid("bde85eeb-39f5-488e-a1a5-575c2abdf8c6");

        //Return a string identifying you or your company.
        public override string AuthorName => "Felix Brunold - VektorNode";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";


    }
}

//public class HeadlesCategoryIcon : GH_AssemblyPriority
//{
//    public override GH_LoadingInstruction PriorityLoad()
//    {
//        Instances.ComponentServer.AddCategoryIcon("Headles", Resources.libIcon);
//        Instances.ComponentServer.AddCategorySymbolName("Headles", 'H');
//        return GH_LoadingInstruction.Proceed;
//    }
//}