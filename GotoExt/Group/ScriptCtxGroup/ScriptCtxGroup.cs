using Microsoft.VisualStudio.Shell;

namespace GotoExt.Command
{
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
