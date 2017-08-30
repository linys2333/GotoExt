using System;
using Microsoft.VisualStudio.Shell;

namespace GotoExt
{
    internal sealed class CodeWinGroup
    {
        private CodeWinGroup(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            // 初始化子项
            ToSQL.Initialize(package);
        }

        public static void Initialize(Package package) => new CodeWinGroup(package);
    }
}
