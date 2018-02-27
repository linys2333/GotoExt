using Microsoft.VisualStudio.Shell;

namespace GotoExt.Command
{
    /// <summary>
    /// 前端上下文组
    /// </summary>
    internal sealed class ScriptCtxGroup : BaseGroup
    {
        private ScriptCtxGroup(Package package) : base(package)
        {
        }

        protected override void InitCommand(Package package)
        {
            ToAPI.Initialize(package);
        }

        public static void Initialize(Package package) => new ScriptCtxGroup(package);
    }
}
