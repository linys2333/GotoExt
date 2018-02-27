using System;
using Microsoft.VisualStudio.Shell;

namespace GotoExt.Command
{
    /// <summary>
    /// 命令组基类
    /// </summary>
    internal abstract class BaseGroup
    {
        protected BaseGroup(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            // 初始化子项
            InitCommand(package);
        }

        protected abstract void InitCommand(Package package);
    }
}
