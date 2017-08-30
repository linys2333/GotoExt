using System;
using GotoExt.Biz;
using GotoExt.Common;
using Microsoft.VisualStudio.Shell;

namespace GotoExt.Command
{
    /// <summary>
    /// 转到SQL命令
    /// </summary>
    internal sealed class ToSQL : BaseCommand<ToSQLBiz>
    {
        public override int CommandId => 0x0200;
        
        public override Guid CommandSet => new Guid("c1597039-3473-40d8-8fb0-792f9f126f7f");
        
        private ToSQL(Package package) : base(package)
        {
        }

        protected override void Query(OleMenuCommand myCommand)
        {
            string extName = Util.GetExtName(Dte.ActiveDocument.Name);
            myCommand.Visible = extName.Equals("cs", StringComparison.OrdinalIgnoreCase);
        }
        
        public static void Initialize(Package package) => new ToSQL(package);
    }
}
