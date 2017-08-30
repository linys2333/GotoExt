using System;
using Microsoft.VisualStudio.Shell;

namespace GotoExt
{
    internal sealed class ScriptCtxGroup
    {
        private ScriptCtxGroup(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            // 初始化子项
            ToAPI.Initialize(package);
        }

        public static void Initialize(Package package) => new ScriptCtxGroup(package);
    }
}
