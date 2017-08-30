using Microsoft.VisualStudio.Shell;

namespace GotoExt.Command
{
    internal sealed class CodeWinGroup : BaseGroup
    {
        private CodeWinGroup(Package package) : base(package)
        {
        }

        protected override void InitCommand(Package package)
        {
            ToSQL.Initialize(package);
        }

        public static void Initialize(Package package) => new CodeWinGroup(package);
    }
}
