using System;
using GotoExt.Biz;
using GotoExt.Common;
using Microsoft.VisualStudio.Shell;

namespace GotoExt.Command
{
    /// <summary>
    /// 转到API命令
    /// </summary>
    internal sealed class ToAPI : BaseCommand<ToAPIBiz>
    {
        public override int CommandId => 0x0100;
        
        public override Guid CommandSet => new Guid("c1597039-3473-40d8-8fb0-792f9f126f7f");
        
        private ToAPI(Package package) : base(package)
        {
        }

        protected override void Query(OleMenuCommand myCommand)
        {
            string extName = Util.GetExtName(Dte.ActiveDocument.Name);
            myCommand.Visible = extName.Equals("js", StringComparison.OrdinalIgnoreCase);
        }

        public static void Initialize(Package package) => new ToAPI(package);
    }
}
